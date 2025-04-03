using HotelReservation.Areas.FrontDesk.ViewModels;
using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace HotelReservation.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Policy = "FrontDeskOnly")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Display Dashboard
        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;

            // Total reservations in the system
            var totalReservations = await _context.Reservations.CountAsync();

            // Today's check-ins (reservations with check-in date today and status CheckedIn)
            var todaysCheckIns = await _context.Reservations.CountAsync(r => r.CheckInDate == today && r.Status == ReservationStatus.CheckedIn);

            // Today's check-outs (reservations with check-out date today and status CheckedOut)
            var todaysCheckOuts = await _context.Reservations.CountAsync(r => r.CheckOutDate == today && r.Status == ReservationStatus.CheckedOut);

            // Today's revenue: Sum of total price for paid reservations that checked in today
            var todaysRevenue = await _context.Reservations
                .Where(r => r.IsPaid && r.CheckInDate == today)
                .SumAsync(r => r.TotalPrice);

            // Room occupancy: percentage of rooms currently occupied vs. total rooms
            var totalRooms = await _context.Rooms.CountAsync();
            var occupiedRooms = await _context.Rooms.CountAsync(r => r.Status == RoomStatus.Occupied);
            double occupancyPercentage = totalRooms > 0 ? ((double)occupiedRooms / totalRooms) * 100 : 0;

            // Housekeeping requests: (for demo, count rooms with status "Maintenance")
            var housekeepingRequests = await _context.Rooms.CountAsync(r => r.Status == RoomStatus.Maintenance);

            var viewModel = new DashboardViewModel
            {
                TotalReservations = totalReservations,
                TodaysCheckIns = todaysCheckIns,
                TodaysCheckOuts = todaysCheckOuts,
                TodaysRevenue = todaysRevenue,
                RoomOccupancyPercentage = occupancyPercentage,
                HousekeepingRequests = housekeepingRequests
            };

            return View(viewModel);
        }
    }
}




















