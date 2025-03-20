using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Models
{
    public enum UserRole
    {
        Guest,
        Admin,
        Housekeeping,
        FrontDesk
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }

        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Guest;

        public string? ProfilePicture { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ New Fields for Email Verification
        public string? EmailVerificationCode { get; set; }  // Stores the verification code
        public bool IsEmailVerified { get; set; } = false;  // Email verification status
    }
}
