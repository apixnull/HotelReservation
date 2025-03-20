using System.ComponentModel.DataAnnotations;

namespace HotelReservation.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
    