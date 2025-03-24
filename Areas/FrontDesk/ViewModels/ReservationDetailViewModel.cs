namespace HotelReservation.Areas.FrontDesk.ViewModels
{
    public class ReservationDetailViewModel
    {
        public int ReservationId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string GuestPhone { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public decimal TotalPrice { get; set; }
        public string SpecialRequest { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public string Status { get; set; } = string.Empty;

        // Room Details
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public string? RoomDescription { get; set; }
        public string? RoomImage1 { get; set; }

        // Payment Details
        public decimal? PaymentAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? TransactionDate { get; set; }
    }
}
