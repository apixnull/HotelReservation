using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Services
{
    public class UserRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _adminSecretKey;

        public UserRegistrationService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _adminSecretKey = configuration["AdminSettings:SecretKey"]
               ?? throw new ArgumentNullException(nameof(configuration), "Admin secret key is missing from configuration.");
        }

        // Helper method to check if an email already exists.
        private async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> RegisterUserAsync(RegisterFormModel model)
        {
            if (await EmailExistsAsync(model.Email))
                return false; // Instead of throwing an exception, return false.

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                Email = model.Email,
                Password = hashedPassword,  // Ensure this is the correct property name.
                Role = UserRole.Guest,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

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
    }
}
