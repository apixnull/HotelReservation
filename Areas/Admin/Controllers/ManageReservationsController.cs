using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservation.Data;
using HotelReservation.Models;
using System.Threading.Tasks;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class ManageReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManageReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Index Page
        public async Task<IActionResult> Index(string search, string status, string payment)
        {
            var reservations = _context.Reservations
                .Include(r => r.Room)
                .AsQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(search))
            {
                string lowerSearch = search.ToLower();
                reservations = reservations.Where(r =>
                    (r.GuestName ?? "").ToLower().Contains(lowerSearch) ||
                    (r.GuestEmail ?? "").ToLower().Contains(lowerSearch) ||
                    (r.BookingReference ?? "").ToLower().Contains(lowerSearch));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReservationStatus>(status, out var parsedStatus))
            {
                reservations = reservations.Where(r => r.Status == parsedStatus);
            }
             
            // Apply payment filter
            if (!string.IsNullOrEmpty(payment))
            {
                bool isPaid = payment == "Paid";
                reservations = reservations.Where(r => r.IsPaid == isPaid);
            }

            return View(await reservations.ToListAsync());
        }


        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Details Page
        public async Task<IActionResult> Details(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Payment) // Include payment details if available
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }


        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reservation model)
        {
            if (id != model.ReservationId)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            // Update only allowed fields.
            reservation.Status = model.Status;
            reservation.IsPaid = model.IsPaid;

            if (model.Status == ReservationStatus.Cancelled)
            {
                reservation.CancellationReason = model.CancellationReason;
            }
            else
            {
                reservation.CancellationReason = null;
            }

            await _context.SaveChangesAsync();

            // Optional: Send an email notification if needed.
            // await _emailService.SendStatusUpdateEmail(reservation.GuestEmail, reservation.Status, reservation.CancellationReason);

            return RedirectToAction(nameof(Index));
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
