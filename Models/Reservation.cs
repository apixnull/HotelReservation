using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Models
{
    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }

    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        [Required]
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        public string? GuestPhone { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public int Adults { get; set; }

        [Required]
        public int Children { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public Payment? Payment { get; set; } // ✅ Payment relationship remains

        // 🔹 New Fields Added Below 🔹
        [Required]
        public string BookingReference { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

        public bool IsPaid { get; set; } = false;

        public string? SpecialRequest { get; set; }

        public string? CancellationReason { get; set; }

        public DateTime? ActualCheckIn { get; set; } // should i remove this ?
        public DateTime? ActualCheckOut { get; set; } // // should i remove this ?

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
