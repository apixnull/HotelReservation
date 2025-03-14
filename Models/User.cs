using System.ComponentModel.DataAnnotations;
using System.Data;

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
        public string Email { get; set; } = String.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Guest;

        public string? ProfilePicture { get; set; }

        [Required]  
        public string Password { get; set; }  = String.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now; 
    }
}
