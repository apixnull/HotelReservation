using System.ComponentModel.DataAnnotations;
using HotelReservation.Models;
using Microsoft.AspNetCore.Http;

namespace HotelReservation.Areas.Admin.ViewModels
{
    public class EditAccountViewModel
    {
        public int UserId { get; set; } // 🔹 Required for editing users

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "User role selection is required.")]
        [Display(Name = "User Role")]
        public UserRole Role { get; set; }

        [MaxLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
       
        public string? FirstName { get; set; }

        [MaxLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        public string? LastName { get; set; }

        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string? Phone { get; set; }

        // ✅ Profile Picture Upload
        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }

        public string? CurrentProfilePicture { get; set; } // ✅ Store existing profile picture path
    }
}
