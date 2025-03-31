using HotelReservation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace YourNamespace.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Policy = "FrontDesk,Admin")]
    public class GuestSearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuestSearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Display Search inputs
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                TempData["Error"] = "Please enter a search term.";
                return RedirectToAction("Index");
            }

            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .Where(r => r.GuestName!.Contains(searchTerm) ||
                            r.GuestEmail!.Contains(searchTerm) ||
                            r.BookingReference.Contains(searchTerm))
                .ToListAsync();

            return View("Index", reservations);
        }
    }
}
