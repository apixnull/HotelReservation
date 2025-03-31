using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Models
{
    public class CleaningLog
    {
        [Key]
        public int CleaningLogId { get; set; }

        [Required]
        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public Room? Room { get; set; }

        [Required]
        public string CleanedBy { get; set; } = string.Empty; // Housekeeper's name or ID

        [Required]
        public DateTime CleaningDate { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; } // Optional remarks
    }
}
