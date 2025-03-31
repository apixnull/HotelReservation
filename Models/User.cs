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

   
        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Guest;

        public string? ProfilePicture { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // New fields for tracking user activity:
        public DateTime? LastLoginDate { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ New Fields for Email Verification
        public string? EmailVerificationCode { get; set; }  // Stores the verification code
        public bool IsEmailVerified { get; set; } = false;  // Email verification status
    }
}
