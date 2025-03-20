using System.ComponentModel.DataAnnotations;

namespace HotelReservation.ViewModels
{
    public class BookingViewModel
    {
        public int RoomId { get; set; } // ✅ Hidden field in the form

        // 🔹 User details (if logged in)
        public string? UserId { get; set; }

        // 🔹 Guest details (for non-logged-in users)
        [Required(ErrorMessage = "Guest name is required.")]
        public string GuestName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string GuestEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        public string GuestPhone { get; set; } = string.Empty;

        // ✅ Check-in & Check-out
        [Required(ErrorMessage = "Check-in date is required.")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required.")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        // ✅ Occupancy
        [Required(ErrorMessage = "Number of adults is required.")]
        [Range(1, 10, ErrorMessage = "Must be between 1 and 10 adults.")]
        public int Adults { get; set; }

        [Required(ErrorMessage = "Number of children is required.")]
        [Range(0, 10, ErrorMessage = "Must be between 0 and 10 children.")]
        public int Children { get; set; }

        // ✅ Total Price (Calculated)
        [Required]
        public decimal TotalPrice { get; set; }
    }
}
