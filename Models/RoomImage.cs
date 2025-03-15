using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Models
{
    public class RoomImage
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        // Optional additional metadata
        public string? Caption { get; set; }
        public string? AltText { get; set; }
        public int SortOrder { get; set; } = 0;

        // Foreign key for Room
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } = null!;
    }
}
