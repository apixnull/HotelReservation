using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Models
{
    public enum RoomType
    {
        Standard,
        Deluxe,
        Suite
    }

    public enum RoomStatus
    {
        Available,
        Booked,
        Occupied,
        Maintenance
    }

    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required, MaxLength(50)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        public RoomType RoomType { get; set; }

        [Required]
        [Range(0, 99999.99)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public RoomStatus Status { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(1, 10)]
        public int MaxOccupancy { get; set; } // ✅ Added Max Occupancy

        public DateTime LastStatusUpdate { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public string? Image1 { get; set; }

        [MaxLength(255)]
        public string? Image2 { get; set; }

        public DateTime? LastCleaned { get; set; } // ✅ NEW: Track last cleaned date

        public bool NeedsCleaning { get; set; } = false; // ✅ NEW: Indicates if cleaning is required
    }
}
