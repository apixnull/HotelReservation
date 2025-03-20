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
        [Required]
        [MaxLength(20)]
        public string GCashNumber { get; set; } = string.Empty;  // Phone number used for GCash payment

        [Required]
        [MaxLength(50)]
        public string GCashTransactionId { get; set; } = string.Empty; // Unique transaction ID from GCash
    }
}
