using System;
using System.ComponentModel.DataAnnotations;
using HotelReservation.Models;

namespace HotelReservation.ViewModels
{
    public class CheckoutViewModel
    {
        public int RoomId { get; set; }
        public Room? Room { get; set; }

        [Required]
        [StringLength(100)]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string GuestEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string GuestPhone { get; set; } = string.Empty;

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Number of adults must be at least 1.")]
        public int Adults { get; set; }

        [Required]
        [Range(0, 10, ErrorMessage = "Number of children cannot be negative.")]
        public int Children { get; set; }

        [Required]
        [Range(0, 999999.99)]
        public decimal TotalPrice { get; set; }

        public string BookingReference { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

        public bool IsPaid { get; set; } = false;

        public string? SpecialRequest { get; set; }
    }
}
