using HotelReservation.ViewModels;
using HotelReservation.Services;
using Microsoft.AspNetCore.Mvc;
using HotelReservation.Data;
using Newtonsoft.Json;
using System.Security.Claims;
using HotelReservation.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingService _bookingService;
        private readonly ApplicationDbContext _context; // ✅ Injecting DbContext

        public BookingController(BookingService bookingService, ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
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
        // Reservation
        [HttpGet]
        public async Task<IActionResult> Reservation(int id)
        {
            var room = await _bookingService.GetRoomDetails(id);
            if (room == null)
            {
                return NotFound();
            }

            // Create the view model and populate with room details
            var reservationViewModel = new ReservationViewModel
            {
                RoomId = room.RoomId,
                TotalPrice = room.Price,
                RoomImage1 = room.Image1,
                RoomImage2 = room.Image2,
                RoomType = room.RoomType.ToString(),
                RoomDescription = room.Description,
                MaxOccupancy = room.MaxOccupancy,
                BookingReference = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()
            };

            // If the user is authenticated, prepopulate guest details
            if (User.Identity?.IsAuthenticated == true)
            {
                // Use ClaimTypes.NameIdentifier to retrieve the user id
                var userId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
                var user = await _context.Users.FindAsync(userId);

                if (user != null)
                {
                    reservationViewModel.GuestName = $"{user.FirstName} {user.LastName}";
                    reservationViewModel.GuestEmail = user.Email;
                    reservationViewModel.GuestPhone = user.Phone ?? string.Empty;
                }
            }

            return View(reservationViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservation(ReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Create a pending reservation record
            var reservation = new Reservation
            {
                RoomId = model.RoomId,
                // If the user is authenticated, UserId may already be set
                UserId = model.UserId,
                GuestName = model.GuestName,
                GuestEmail = model.GuestEmail,
                GuestPhone = model.GuestPhone,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                Adults = model.Adults,
                Children = model.Children,
                TotalPrice = model.TotalPrice,
                BookingReference = model.BookingReference,
                IsPaid = false,
                SpecialRequest = model.SpecialRequest,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Optionally store the reservation ID in TempData to retrieve later in the checkout process
            TempData["ReservationId"] = reservation.ReservationId;

            // Redirect to the checkout page
            return RedirectToAction("Checkout", "Payment");
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        [HttpGet]
        public async Task<IActionResult> MyReservationSummary(int id)
        {
            // Retrieve the reservation including its related Room and Payment details
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            return View(reservation);
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
