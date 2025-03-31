using System.Security.Claims;
using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }




        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Authenticate User

        public async Task<bool> AuthenticateUser(LoginFormModel model)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                Console.WriteLine("🚨 ERROR: HttpContext is null. Cannot authenticate.");
                return false;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                Console.WriteLine("❌ Login failed: Invalid credentials.");
                return false;
            }

            var claimsPrincipal = CreateUserClaimsPrincipal(user);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                AllowRefresh = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(2)
            };

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            httpContext.User = claimsPrincipal;

            Console.WriteLine($"✅ User {user.Email} authenticated successfully.");

            // ✅ Debugging: Check if authentication cookie is set in response
            var cookieHeader = httpContext.Response.Headers["Set-Cookie"].ToString(); 

            return !string.IsNullOrEmpty(cookieHeader);
        }




        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Signout User
        public async Task SignOutAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                Console.WriteLine("🚨 ERROR: HttpContext is null. Cannot sign out.");
                return;
            }

            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            Console.WriteLine("✅ User signed out successfully.");
        }


        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Helper Methods
        private ClaimsPrincipal CreateUserClaimsPrincipal(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("ProfilePicture", user.ProfilePicture ?? "/images/default-profile.png")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}
