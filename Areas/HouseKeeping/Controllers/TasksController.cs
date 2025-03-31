using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace HotelReservation.Areas.Housekeeping.Controllers
{
    [Area("Housekeeping")]
    [Authorize(Policy = "HousekeepingOnly")]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Housekeeping/Tasks/Index
        public async Task<IActionResult> Index()
        {
            // Get all cleaning requests, including room information.
            var tasks = await _context.HousekeepingRequests
                .Include(t => t.Room)
                .OrderByDescending(t => t.RequestDate)
                .ToListAsync();
            return View(tasks);
        }

        // POST: Housekeeping/Tasks/MarkAsCompleted/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsCompleted(int id)
        {
            var request = await _context.HousekeepingRequests.FindAsync(id);
            if (request == null)
            {
                TempData["Error"] = "Housekeeping task not found.";
                return RedirectToAction("Index");
            }

            // Update the request status to Completed.
            request.Status = RequestStatus.Completed;
            // Optionally update the room's cleaning fields:
            if (request.Room != null)
            {
                request.Room.LastCleaned = System.DateTime.UtcNow;
                request.Room.NeedsCleaning = false;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Task marked as completed.";
            return RedirectToAction("Index");
        }
    }
}
