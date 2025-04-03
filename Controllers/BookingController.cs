using HotelReservation.ViewModels;
using HotelReservation.Services;
using Microsoft.AspNetCore.Mvc;
using HotelReservation.Data;
using Newtonsoft.Json;
using System.Security.Claims;
using HotelReservation.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
        public async Task<IActionResult> Search(int MaxOccupancy, string? RoomType, DateTime? CheckInDate, DateTime? CheckOutDate)
        {
            
            var availableRooms = await _bookingService.SearchAvailableRooms(MaxOccupancy, RoomType);

            // Format dates into Y-m-d format to match flatpickr
            ViewData["CheckInDate"] = CheckInDate;
            ViewData["CheckOutDate"] = CheckOutDate;

            return View("Search", availableRooms);
        }


        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Reservation
        [HttpGet]
        public async Task<IActionResult> Reservation(int id, DateTime checkin, DateTime checkout)
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
                BookingReference = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                CheckInDate = checkin,
                CheckOutDate = checkout
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
            // Debugging: Log the received form values
            Console.WriteLine($"🔍 Debug - Received CheckInDate: {model.CheckInDate}");
            Console.WriteLine($"🔍 Debug - Received CheckOutDate: {model.CheckOutDate}");


            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Ensure UserId is set if the user is authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                model.UserId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            }


            // Fetch the room first to update its status
            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null || room.Status != RoomStatus.Available)
            {
                TempData["Error"] = "Sorry, the room is no longer available.";
                return RedirectToAction("Index", "Home"); // Or return to the reservation form
            }

            // Update the room status to "Pending"
            room.Status = RoomStatus.Pending;
            room.LastStatusUpdate = DateTime.UtcNow;


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
         
         
            // Save both the reservation and room update
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
