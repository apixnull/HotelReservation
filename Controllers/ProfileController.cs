using HotelReservation.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        // GET: Profile/Index
        // GET: Profile/Index
        public IActionResult Index()
        {
            // Simulating fetching user data (you would typically pull this data from your database)
            var userProfile = new UserProfileViewModel
            {
                FirstName = "John",  // Replace with actual user's first name
                LastName = "Doe",    // Replace with actual user's last name
                Phone = "+1234567890", // Replace with actual user's phone number
                Email = "johndoe@example.com", // Replace with actual user's email
                IsEmailVerified = false,  // Replace with actual email verification status
                ProfilePictureUrl = "/images/default-profile.jpg"  // Replace with actual profile picture URL
            };

            return View(userProfile);
        }

    }
}
