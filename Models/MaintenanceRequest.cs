using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Models
{
    public class MaintenanceRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public Room? Room { get; set; }

        [Required]
        public string Issue { get; set; } = string.Empty;

        [Required]
        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    }

    public enum MaintenanceStatus
    {
        Pending,
        InProgress,
        Completed
    }
}
