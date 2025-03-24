using System.ComponentModel.DataAnnotations;
using HotelReservation.Models;

namespace HotelReservation.Areas.FrontDesk.ViewModels
{
    public class SearchRoomViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Max occupancy must be between 1 and 10.")]
        public int MaxOccupancy { get; set; }

        [Required]
        public RoomType SelectedRoomType { get; set; }

        public List<Room>? AvailableRooms { get; set; } // Stores search results
    }
}
