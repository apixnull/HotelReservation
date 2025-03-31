using System;
using System.Collections.Generic;
using HotelReservation.Models;

namespace HotelReservation.Areas.Housekeeping.ViewModels
{
    public class HousekeepingDashboardViewModel
    {
        public int TotalRooms { get; set; }
        public int RoomsNeedingCleaning { get; set; }
        public int RoomsCleanedToday { get; set; }
        public int PendingMaintenanceRequests { get; set; }

        // Optionally, you can include a list of rooms that need cleaning
        public List<RoomViewModel> RoomsToClean { get; set; } = new List<RoomViewModel>();
    }

    public class RoomViewModel
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public DateTime? LastCleaned { get; set; }
    }
}
