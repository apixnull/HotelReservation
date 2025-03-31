using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Models
{
    public class HousekeepingRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public Room? Room { get; set; }

        [Required]
        public string RequestDescription { get; set; } = string.Empty;

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    }

    public enum RequestStatus
    {
        Pending,
        InProgress,
        Completed
    }
}
