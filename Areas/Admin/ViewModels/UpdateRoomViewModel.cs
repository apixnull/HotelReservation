using HotelReservation.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Areas.Admin.ViewModels
{
    public class UpdateRoomViewModel
    {
        [Required]
        public int RoomId { get; set; }

        [Required, StringLength(50, ErrorMessage = "Room number cannot exceed 50 characters.")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        public RoomType RoomType { get; set; }

        [Required]
        [Range(0, 99999.99, ErrorMessage = "Price must be between 0 and 99,999.99.")]
        public decimal Price { get; set; }

        [Required]
        public RoomStatus Status { get; set; }

        public string? Description { get; set; }

        public string? Amenities { get; set; }

        // New images to add (if any). Existing images are handled separately.
        public List<IFormFile>? Images { get; set; } = new List<IFormFile>();

        // Existing image paths for display (not for upload)
        public List<string>? ExistingImagePaths { get; set; } = new List<string>();

    }
}
