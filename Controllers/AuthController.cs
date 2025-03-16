using Microsoft.AspNetCore.Mvc;
using HotelReservation.ViewModels;
using HotelReservation.Services;
using System.Security.Claims;
using HotelReservation.Models;
using Microsoft.EntityFrameworkCore;
using HotelReservation.Data;

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

        /*********************************************/
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

            TempData["Success"] = "Login successful.";
            return RedirectToAction("Index", "Home");
        }

        /*********************************************/
        // Register normal user
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

            if (!await _userRegistrationService.RegisterUserAsync(model))
            {
                TempData["Error"] = "Email is already in use.";
                return View(model);
            }

            // Auto-login after successful registration
            var loginModel = new LoginFormModel { Email = model.Email, Password = model.Password, RememberMe = false };
            if (await _authService.AuthenticateUser(loginModel))
            {
                TempData["Success"] = "Registration successful. You are now logged in.";
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Registration successful, but auto-login failed.";
            return RedirectToAction("Login");
        }

        /*********************************************/
        // Register admin
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

        /*********************************************/
        // Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutAsync();
            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction("Login");
        }

        /************************************************/
        // UnAuthorized Acccess
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

    }
}
