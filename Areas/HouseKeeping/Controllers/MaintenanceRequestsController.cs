using System;
using System.Linq;
using System.Threading.Tasks;
using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Areas.Housekeeping.Controllers
{
    [Area("Housekeeping")]
    public class MaintenanceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Housekeeping/MaintenanceRequests
        public async Task<IActionResult> Index()
        {
            var requests = await _context.MaintenanceRequests
                .Include(m => m.Room)
                .OrderByDescending(m => m.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        // GET: Housekeeping/MaintenanceRequests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Housekeeping/MaintenanceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceRequest model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields correctly.";
                return View(model);
            }

            // Retrieve the room number from the form (this input isn't bound to the model)
            var roomNumber = Request.Form["RoomNumber"].ToString();
            if (string.IsNullOrEmpty(roomNumber))
            {
                TempData["Error"] = "Room number is required.";
                return View(model);
            }

            // Find the room by its room number
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
            if (room == null)
            {
                TempData["Error"] = "Selected room does not exist.";
                return View(model);
            }

            // Set the RoomId in the model based on the found room
            model.RoomId = room.RoomId;
            model.RequestDate = DateTime.UtcNow;
            model.Status = MaintenanceStatus.Pending;

            _context.MaintenanceRequests.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Maintenance request submitted successfully.";
            return RedirectToAction("Index");
        }

    }
}
