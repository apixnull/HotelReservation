using System;
using System.Collections.Generic;
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
        [Column(TypeName = "decimal(10,2)")] // ✅ Fix decimal precision issue
        public decimal Price { get; set; }

        [Required]
        public RoomStatus Status { get; set; }

        public string? Description { get; set; }

        public string? Amenities { get; set; }

        // Real-time availability tracking: last status update
        public DateTime LastStatusUpdate { get; set; } = DateTime.UtcNow;

        // ✅ Navigation Property for Room Images (One-to-Many)
        public virtual List<RoomImage> Images { get; set; } = new List<RoomImage>();
    }
}
