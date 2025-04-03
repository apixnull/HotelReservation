using HotelReservation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Policy = "FrontDeskOnly")] 
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Display Billings and Payments
        // Optional filter: "Paid" or "Unpaid"
        public async Task<IActionResult> Index(string filter)
        {
            var reservationsQuery = _context.Reservations
                .Include(r => r.Room)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                if (filter == "Paid")
                {
                    reservationsQuery = reservationsQuery.Where(r => r.IsPaid);
                }
                else if (filter == "Unpaid")
                {
                    reservationsQuery = reservationsQuery.Where(r => !r.IsPaid);
                }
            }

            var reservations = await reservationsQuery.ToListAsync();
            return View(reservations);
        }
    }
}
