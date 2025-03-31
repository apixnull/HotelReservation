using HotelReservation.Areas.FrontDesk.ViewModels;
using HotelReservation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace HotelReservation.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Policy = "FrontDesk,Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Index
        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;

            // Calculate total reservations
            var totalReservations = await _context.Reservations.CountAsync();

            // Calculate today's check-ins (assuming check-in date is today)
            var todaysCheckIns = await _context.Reservations
                .Where(r => r.CheckInDate == today)
                .CountAsync();

            // Calculate today's revenue for paid reservations (using total price)
            var todaysRevenue = await _context.Reservations
                .Where(r => r.IsPaid && r.CheckInDate == today)
                .SumAsync(r => r.TotalPrice);

            var viewModel = new ReportsViewModel
            {
                TotalReservations = totalReservations,
                TodaysCheckIns = todaysCheckIns,
                TodaysRevenue = todaysRevenue
            };

            return View(viewModel);
        }
    }
}
