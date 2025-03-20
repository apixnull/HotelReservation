using System.ComponentModel.DataAnnotations;

namespace HotelReservation.ViewModels
{
    public class EmailVerificationViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits.")]
        public string Code { get; set; } = string.Empty;
    }
}
