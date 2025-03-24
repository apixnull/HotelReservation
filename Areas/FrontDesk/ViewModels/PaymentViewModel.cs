using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Areas.FrontDesk.ViewModels
{
    public class PaymentViewModel
    {
        public int ReservationId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public string? PaymentMethod { get; set; }
        public string? GCashNumber { get; set; }

        // ✅ Add missing properties for Summary page
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string? RoomNumber { get; set; }
        public string? RoomImage1 { get; set; }
    }
}
