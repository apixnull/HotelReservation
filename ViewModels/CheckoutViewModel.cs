using System;
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        public int ReservationId { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        // For GCash payment (to be augmented when integrating PayMongo)
        [Required]
        [MaxLength(20)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "Invalid GCash number format.")]
        public string GCashNumber { get; set; } = string.Empty;
    }
}
