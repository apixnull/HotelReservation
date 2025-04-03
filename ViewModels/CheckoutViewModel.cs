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

        // Payment Method (Cash or GCash)
        public string PaymentMethod { get; set; } = string.Empty;

        // For GCash payment (to be augmented when integrating PayMongo)
        [MaxLength(20)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "Invalid GCash number format.")]
        public string? GCashNumber { get; set; }

        // For Cash payment
        [MaxLength(255)]
        public string? CashReceivedBy { get; set; } // Name of the cashier handling payment
    }
}