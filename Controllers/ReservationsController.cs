using HotelReservation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Find My Booking Page
        public IActionResult FindMyBooking()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(MyBookings));
            }
            return View();
        }

        // POST: Find My Booking for Guests
        [HttpPost]
        public async Task<IActionResult> FindMyBooking(string email, string bookingReference)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(bookingReference))
            {
                TempData["Error"] = "Please enter both Email and Booking Reference.";
                return RedirectToAction("FindMyBooking");
            }

            var reservations = await _context.Reservations
                .Where(r => r.GuestEmail == email && r.BookingReference == bookingReference)
                .Include(r => r.Room)       // Include Room details
                .Include(r => r.Payment)    // Include Payment details
                .FirstOrDefaultAsync(); // ✅ Returns a single reservation;

            if (reservations == null)
            {
                TempData["Error"] = "No booking found with the provided details.";
                return RedirectToAction("FindMyBooking");
            }

            return View("GuestBookings", reservations);
        }


        // GET: My Bookings (For Logged-In Users)
        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return View(reservations);
        }
    }
}
