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


        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Search
        public async Task<IActionResult> Search(int MaxOccupancy, string? RoomType)
        {
            var availableRooms = await _bookingService.SearchAvailableRooms(MaxOccupancy, RoomType);
            return View("Search", availableRooms);
        }


        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Book


        [HttpGet]
        public async Task<IActionResult> Book(int id)
        {
            var room = await _bookingService.GetRoomDetails(id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room); // ✅ Pass Room Model Directly
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(int RoomId, DateTime CheckInDate, DateTime CheckOutDate, decimal TotalPrice)
        {
            if (CheckInDate < DateTime.Today || CheckOutDate <= CheckInDate)
            {
                ModelState.AddModelError("", "Invalid check-in/check-out dates.");
                return RedirectToAction("Book", new { id = RoomId });
            }

            // ✅ Store values in TempData
            TempData["RoomId"] = RoomId;
            TempData["CheckInDate"] = CheckInDate.ToString("yyyy-MM-dd");
            TempData["CheckOutDate"] = CheckOutDate.ToString("yyyy-MM-dd");
            TempData["TotalPrice"] = TotalPrice.ToString();

            // ✅ Ensure TempData persists
            TempData.Keep("RoomId");
            TempData.Keep("CheckInDate");
            TempData.Keep("CheckOutDate");
            TempData.Keep("TotalPrice");

            return RedirectToAction("Checkout");
        }



        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Checkout - Review and Confirm Booking
        // Checkout - Review and Confirm Booking
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!TempData.ContainsKey("RoomId") ||
                !TempData.ContainsKey("CheckInDate") ||
                !TempData.ContainsKey("CheckOutDate") ||
                !TempData.ContainsKey("TotalPrice"))
            {
                return RedirectToAction("Search");
            }

            int RoomId = Convert.ToInt32(TempData["RoomId"]);
            DateTime CheckInDate = DateTime.Parse(TempData["CheckInDate"]?.ToString() ?? "");
            DateTime CheckOutDate = DateTime.Parse(TempData["CheckOutDate"]?.ToString() ?? "");
            decimal TotalPrice = decimal.Parse(TempData["TotalPrice"]?.ToString() ?? "0"); // 🔹 Convert string back to decimal

            TempData.Keep("RoomId");
            TempData.Keep("CheckInDate");
            TempData.Keep("CheckOutDate");
            TempData.Keep("TotalPrice");

            var room = await _bookingService.GetRoomDetails(RoomId);
            if (room == null)
            {
                return NotFound();
            }

            var checkoutModel = new CheckoutViewModel
            {
                RoomId = RoomId,
                CheckInDate = CheckInDate,
                CheckOutDate = CheckOutDate,
                TotalPrice = TotalPrice,
                Room = room
            };

            return View(checkoutModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var reservation = await _bookingService.CreateReservation(model, User);
            if (reservation == null)
            {
                ModelState.AddModelError("", "Something went wrong. Please try again.");
                return View(model);
            }

            return RedirectToAction("Confirmation", new { id = reservation.ReservationId });
        }




        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // GetRoomDetails
        public async Task<IActionResult> GetRoomDetails(int id)
        {
            var room = await _bookingService.GetRoomDetails(id);
            if (room == null)
            {
                return Content("<p class='text-danger'>Room not found.</p>");
            }
            return PartialView("_RoomDetailsPartial", room);
        }

    }
}
