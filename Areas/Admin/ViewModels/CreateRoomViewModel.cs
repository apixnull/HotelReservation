using System.ComponentModel.DataAnnotations;
using HotelReservation.Models;

namespace HotelReservation.Areas.Admin.ViewModels
{
    public class CreateRoomViewModel
    {
        [Required(ErrorMessage = "Room Number is required.")]
        [MaxLength(50, ErrorMessage = "Room Number cannot exceed 50 characters.")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room Type is required.")]
        public RoomType RoomType { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, 99999.99, ErrorMessage = "Price must be between 0 and 99,999.99.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Room Status is required.")]
        public RoomStatus Status { get; set; }

        [Required(ErrorMessage = "Max Occupancy is required.")]
        [Range(1, 10, ErrorMessage = "Max Occupancy must be between 1 and 10.")]
        public int MaxOccupancy { get; set; } // ✅ Added Max Occupancy

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [MaxLength(255, ErrorMessage = "Amenities list cannot exceed 255 characters.")]

        // ✅ Upload fields for images
        [Display(Name = "Room Image 1")]
        public IFormFile? Image1 { get; set; }

        [Display(Name = "Room Image 2")]
        public IFormFile? Image2 { get; set; }
    }
}
