﻿using System.Security.Claims;
using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace YourNamespace.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Policy = "FrontDeskOnly")]
    public class CheckInController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckInController(ApplicationDbContext context)
        {
            _context = context;
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Display Check in for today
        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var reservations = await _context.Reservations
                .Where(r => r.CheckInDate.Date == today)
                .Include(r => r.Room)
                .ToListAsync();

            return View(reservations); // Passing full list directly
        }



        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Mark The Reseravtion Status as Checkin 
        [HttpPost]
        public async Task<IActionResult> CheckInGuest(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room) // ✅ Include Room data
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var frontDeskUserId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            reservation.Status = ReservationStatus.CheckedIn;
            reservation.ActualCheckIn = DateTime.UtcNow;
            reservation.CheckedInBy = frontDeskUserId;

            if (reservation.Room != null)
            {
                reservation.Room.Status = RoomStatus.Occupied; // ✅ Set room as occupied
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Guest checked in successfully!";
            return RedirectToAction("Index");
        }

    }
}
