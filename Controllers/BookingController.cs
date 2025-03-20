// BookingController.cs
using System.Security.Claims;
using HotelReservation.Models;
using HotelReservation.ViewModels;
using HotelReservation.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingService _bookingService;

        public BookingController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task<IActionResult> Search(int MaxOccupancy, string? RoomType)
        {
            var availableRooms = await _bookingService.SearchAvailableRooms(MaxOccupancy, RoomType);
            return View("Search", availableRooms);
        }

        public async Task<IActionResult> GetRoomDetails(int id)
        {
            var room = await _bookingService.GetRoomDetails(id);
            if (room == null)
            {
                return Content("<p class='text-danger'>Room not found.</p>");
            }
            return PartialView("_RoomDetailsPartial", room);
        }

        public async Task<IActionResult> Book(int id)
        {
            var viewModel = await _bookingService.GetBookingDetails(id, User);
            if (viewModel == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var reservation = await _bookingService.CreateBooking(model, User);
            if (reservation == null)
            {
                ModelState.AddModelError("", "Selected room is no longer available.");
                return View(model);
            }

            return RedirectToAction("Confirmation", new { id = reservation.ReservationId });
        }
    }
}
