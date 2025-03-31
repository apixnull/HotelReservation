namespace HotelReservation.Areas.FrontDesk.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalReservations { get; set; }
        public int TodaysCheckIns { get; set; }
        public int TodaysCheckOuts { get; set; }
        public decimal TodaysRevenue { get; set; }
        public double RoomOccupancyPercentage { get; set; }
        public int HousekeepingRequests { get; set; }
    }
}
