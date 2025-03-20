using System.ComponentModel.DataAnnotations;

namespace HotelReservation.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
