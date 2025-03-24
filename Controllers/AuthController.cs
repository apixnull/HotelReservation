using Microsoft.AspNetCore.Mvc;
using HotelReservation.ViewModels;
using HotelReservation.Services;
using System.Security.Claims;

namespace HotelReservation.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly UserRegistrationService _userRegistrationService;


        public AuthController(AuthService authService, UserRegistrationService userRegistrationService)
        {   
            _authService = authService;
            _userRegistrationService = userRegistrationService;
        }


        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Login 
        [HttpGet]
        public IActionResult Login(string? warning)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                TempData["Error"] = "Invalid Action.";
                return RedirectToAction("Index", "Home"); // Redirect logged-in users
            }

            if (!string.IsNullOrEmpty(warning))
            {
                TempData["Warning"] = warning;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginFormModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var isAuthenticated = await _authService.AuthenticateUser(model);
            if (!isAuthenticated)
            {
                TempData["Error"] = "Invalid Email or Password.";
                return View(model);
            }

            var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Admin")
            {
                TempData["Success"] = "Welcome Admin!";
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            else if (userRole == "FrontDesk")
            {
                TempData["Success"] = "Welcome Front Desk!";
                return RedirectToAction("Index", "Dashboard", new { area = "FrontDesk" });
            }


            TempData["Success"] = "Login successful.";
            return RedirectToAction("Index", "Home");
        }



        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Register for Guest
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                TempData["Error"] = "Invalid Action.";
                return RedirectToAction("Index", "Home"); // Redirect logged-in users
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterFormModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userRegistrationService.RegisterUserAsync(model);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("VerifyEmail", new { email = model.Email });
        }





        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Register for Admin
        [HttpGet]
        public IActionResult RegisterAdmin()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                TempData["Error"] = "Invalid Action.";
                return RedirectToAction("Index", "Home"); // Redirect logged-in users
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> RegisterAdmin(AdminRegisterFormModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!await _userRegistrationService.RegisterAdminAsync(model))
            {
                TempData["Error"] = "Email is already in use or secret key is invalid.";
                return View(model);
            }

            // Auto-login after successful admin registration
            var loginModel = new LoginFormModel { Email = model.Email, Password = model.Password, RememberMe = false };
            if (await _authService.AuthenticateUser(loginModel))
            {
                TempData["Success"] = "Admin registration successful. You are now logged in.";
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Admin registration successful, but auto-login failed.";
            return RedirectToAction("Login");
        }




        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutAsync();
            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction("Login");
        }




        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Unauthorized Access
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }




        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Verify Email
        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Invalid request. No email provided.";
                return RedirectToAction("Register");
            }

            var model = new EmailVerificationViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(EmailVerificationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var isVerified = await _userRegistrationService.VerifyEmailAsync(model.Email, model.Code);
            if (!isVerified)
            {
                TempData["Error"] = "Invalid verification code or email already verified.";
                return View(model);
            }

            TempData["Success"] = "Email verified successfully!, You can login now";
            return RedirectToAction("Login");
        }
        

        [HttpGet]
        public async Task<IActionResult> ResendVerification(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Invalid email.";
                return RedirectToAction("VerifyEmail");
            }

            var success = await _userRegistrationService.ResendVerificationCodeAsync(email);
            if (!success)
            {
                TempData["Error"] = "Failed to resend verification code. Email may already be verified.";
                return RedirectToAction("VerifyEmail");
            }

            TempData["Success"] = "New verification code sent!";
            return RedirectToAction("VerifyEmail");
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Forgot Password

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userRegistrationService.SendPasswordResetLinkAsync(model.Email);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("ForgotPassword"); // ✅ Prevents reloading the same failed form
            }

            TempData["Success"] = "Password reset link has been sent to your email.";
            return RedirectToAction("Login");
        }


        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Reset Password
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Invalid or expired password reset request.";
                return RedirectToAction("ForgotPassword");
            }

            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return View(model);
            }

            // ✅ Call the method correctly with only email & new password
            var result = await _userRegistrationService.ResetPasswordAsync(model.Email, model.NewPassword);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(model);
            }

            TempData["Success"] = "Password reset successful! You can now log in.";
            return RedirectToAction("Login");
        }
    }
}
