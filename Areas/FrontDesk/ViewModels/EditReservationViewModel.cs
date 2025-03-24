using System.ComponentModel.DataAnnotations;
using HotelReservation.Models;

namespace HotelReservation.Areas.FrontDesk.ViewModels
{
    public class EditReservationViewModel
    {
        public int ReservationId { get; set; }

        [Required]
        [Display(Name = "Reservation Status")]
        public ReservationStatus Status { get; set; }

        [Display(Name = "Paid")]
        public bool IsPaid { get; set; }
    }
}
