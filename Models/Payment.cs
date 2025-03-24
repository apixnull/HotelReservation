using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Models
{
    public enum PaymentStatus
    {
        Success,
        Failed,
        Pending
    }

    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int ReservationId { get; set; }
        [ForeignKey("ReservationId")]
        public required Reservation Reservation { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        // ✅ GCash Payment Details
        
        [MaxLength(20)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "Invalid GCash number format.")]
        public string? GCashNumber { get; set; } = string.Empty;  // Phone number used for GCash payment

        [MaxLength(50)]
        public string? GCashTransactionId { get; set; } = string.Empty; // Unique transaction ID from GCash

        // ✅ New Fields
        public string? PaymentGatewayReference { get; set; } // PayPal/Stripe reference

        public DateTime? PaidAt { get; set; } // When payment was completed

        public bool IsRefunded { get; set; } = false;
        public DateTime? RefundedAt { get; set; } // Date of refund
        public string? RefundTransactionId { get; set; } // Refund transaction ID
        public string? PaymentMethod { get; set; }
    }
}
