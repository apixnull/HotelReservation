using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace YourNamespace.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Policy = "FrontDesk,Admin")]
    public class CheckOutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckOutController(ApplicationDbContext context)
        {
            _context = context;
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Display Reservation at needs to be checkout Today
        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var checkedInGuests = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.CheckedIn)
                .Include(r => r.Room)
                .Include(r => r.CheckedInByUser)
                .ToListAsync();

            return View(checkedInGuests);
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Mark Reservation status as Checkout
        [HttpPost]
        public async Task<IActionResult> CheckOutGuest(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = ReservationStatus.CheckedOut;
            reservation.ActualCheckOut = DateTime.UtcNow;

            if (reservation.Room != null)
            {
                reservation.Room.Status = RoomStatus.Available;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Guest checked out successfully!";

            // 🔹 Redirect to receipt page after checkout
            return RedirectToAction("Receipt", new { id = reservation.ReservationId });
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Generate Receipt
        public async Task<IActionResult> Receipt(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.ReservationId == id);
             
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

    }
}
