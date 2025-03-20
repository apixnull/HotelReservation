using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.ViewModels;
using Microsoft.EntityFrameworkCore;
using HotelReservation.Helpers;


namespace HotelReservation.Services
{
    public class UserRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService; // ✅ Inject EmailService
        private readonly string _adminSecretKey;
        private readonly VerimailService _verimailService;

        public UserRegistrationService(ApplicationDbContext context, IConfiguration configuration, EmailService emailService, VerimailService zeroBounceService)
        {
           
            _context = context;
            _emailService = emailService;
            _verimailService = zeroBounceService;
            _adminSecretKey = configuration["AdminSettings:SecretKey"]
               ?? throw new ArgumentNullException(nameof(configuration), "Admin secret key is missing from configuration.");
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Register a Guest User
        public async Task<ResponseResult> RegisterUserAsync(RegisterFormModel model)
        {
            if (await EmailExistsAsync(model.Email))
                return ResponseResult.Fail("Email is already in use.");

            // ✅ Validate email using Verimail
            var isValidEmail = await _verimailService.IsValidEmailAsync(model.Email);
            if (!isValidEmail)
                return ResponseResult.Fail("Invalid email address. Please enter a real email.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var verificationCode = GenerateVerificationCode();

            var user = new User
            {
                Email = model.Email,
                Password = hashedPassword,
                Role = UserRole.Guest,
                CreatedAt = DateTime.UtcNow,
                EmailVerificationCode = verificationCode
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // ✅ Send email verification
            await _emailService.SendEmailAsync(user.Email, "Verify Your Email",
                $"Your verification code is: <b>{verificationCode}</b>. Enter this code to complete your registration.");

            return ResponseResult.Ok("Registration successful! Please verify your email.");
        }





        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Verify the email of the Guest User
        public async Task<bool> VerifyEmailAsync(string email, string code)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.IsEmailVerified || string.IsNullOrEmpty(user.EmailVerificationCode) || user.EmailVerificationCode.Trim() != code.Trim())
                return false;

            user.IsEmailVerified = true;
            user.EmailVerificationCode = null;
            await _context.SaveChangesAsync();
            return true;
        }



        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Resend the verification code to the Guest User
        public async Task<bool> ResendVerificationCodeAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.IsEmailVerified) return false;

            var newCode = GenerateVerificationCode();
            user.EmailVerificationCode = newCode;
            await _context.SaveChangesAsync();

            // ✅ Use EmailService
            await _emailService.SendEmailAsync(user.Email, "New Verification Code",
                $"Your new verification code is: <b>{newCode}</b>. Enter this code to complete your registration.");

            return true;
        }



        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Register an Admin User
        public async Task<bool> RegisterAdminAsync(AdminRegisterFormModel model)
        {
            if (await EmailExistsAsync(model.Email))
                return false; // Return false instead of throwing an exception.

            if (model.SecretKey != _adminSecretKey)
                return false; // Return false for invalid secret key.

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var admin = new User
            {
                Email = model.Email,
                Password = hashedPassword,  // Ensure this is the correct property name.
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();
            return true;
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Send password reset link async
        public async Task<ResponseResult> SendPasswordResetLinkAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return ResponseResult.Fail("Email not found");
            }

            // ✅ Generate a secure token based on email and timestamp
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var rawToken = $"{email}-{timestamp}-SecretKey"; // Replace "SecretKey" with a strong secret
            var token = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken)));

            var resetLink = $"http://localhost:5139/Auth/ResetPassword?email={email}&token={Uri.EscapeDataString(token)}";

            // ✅ Send email with the reset link
            await _emailService.SendEmailAsync(email, "Password Reset Request",
                $"Click the link to reset your password: <a href='{resetLink}'>Reset Password</a>");

            return ResponseResult.Ok("Password reset link sent to your email.");
        }



        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Reset password reset link async
        public async Task<ResponseResult> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return ResponseResult.Fail("User not found.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return ResponseResult.Ok("Your password has been reset successfully.");
        }





        /***********************************************************************************************************/
        //                                                                                                         
        // HELPER METHODS                  
        //
        /***********************************************************************************************************/

        // check if email already exist in the database
        private async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }


        // generate code for Verification
        private static string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public bool IsResetTokenValid(string email, string token)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var rawToken = $"{email}-{timestamp}-SecretKey";
            var expectedToken = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken)));

            return token == expectedToken;
        }

    }
}
