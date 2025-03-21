using System;
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.ViewModels
{
    public class ReservationViewModel
    {
        // Reservation Details
        [Required]
        public int RoomId { get; set; }

        public int? UserId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Guest name cannot exceed 100 characters.")]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string GuestEmail { get; set; } = string.Empty;

        [Required]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string GuestPhone { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Adults must be between 1 and 20.")]
        public int Adults { get; set; }

        [Required]
        [Range(0, 20, ErrorMessage = "Children must be between 0 and 20.")]
        public int Children { get; set; }

        [Required]
        [Range(0, 999999.99, ErrorMessage = "Total price must be a valid amount.")]
        public decimal TotalPrice { get; set; }

        [Required]
        public string BookingReference { get; set; } = string.Empty;

        public bool IsPaid { get; set; }

        [StringLength(500, ErrorMessage = "Special request cannot exceed 500 characters.")]
        public string? SpecialRequest { get; set; }

        // Additional Reservation Fields
        public string? CancellationReason { get; set; }
        public DateTime? ActualCheckIn { get; set; }
        public DateTime? ActualCheckOut { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Room Details for Display Purposes
        public string? RoomImage1 { get; set; }
        public string? RoomImage2 { get; set; }
        public string? RoomType { get; set; }
        public string? RoomDescription { get; set; }
        public int? MaxOccupancy { get; set; }
        public string? RoomNumber { get; set; }
    }
}
