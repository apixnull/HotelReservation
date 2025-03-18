using HotelReservation.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Areas.Admin.ViewModels
{
    public class EditRoomViewModel
    {
        public int RoomId { get; set; } // ✅ Required for updating existing room

        [Required(ErrorMessage = "Room Number is required.")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room Type is required.")]
        public RoomType RoomType { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, 99999.99, ErrorMessage = "Price must be between 0 and 99,999.99")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Room Status is required.")]
        public RoomStatus Status { get; set; }

        [Required(ErrorMessage = "Max Occupancy is required.")]
        [Range(1, 10, ErrorMessage = "Max Occupancy must be between 1 and 10.")]
        public int MaxOccupancy { get; set; } // ✅ Added Max Occupancy

        public string? Description { get; set; }
        public string? Amenities { get; set; }

        // ✅ Existing Image Paths (For Display)
        public string? ExistingImage1 { get; set; }
        public string? ExistingImage2 { get; set; }

        // ✅ New Images (If updating)
        [Display(Name = "Replace Image 1")]
        public IFormFile? Image1 { get; set; }

        [Display(Name = "Replace Image 2")]
        public IFormFile? Image2 { get; set; }
    }
}
