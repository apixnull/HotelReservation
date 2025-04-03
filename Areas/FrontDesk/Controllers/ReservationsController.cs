using System.Security.Claims;
using HotelReservation.Areas.FrontDesk.ViewModels;
using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.Services;
using HotelReservation.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace YourNamespace.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Policy = "FrontDeskOnly")]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public ReservationsController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;   
        }




        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Index
        public async Task<IActionResult> Index()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .ToListAsync();

            // No need to modify the actual date values, but format them for display in the view
            foreach (var reservation in reservations)
            {
                // Convert to local time if needed and format the dates
                reservation.CheckInDate = reservation.CheckInDate.ToLocalTime();
                reservation.CheckOutDate = reservation.CheckOutDate.ToLocalTime();
            }

            return View(reservations);
        }






        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/

        public IActionResult SearchRoom()
        {
            return View(new SearchRoomViewModel { AvailableRooms = new List<Room>() });
        }
        //  Search Rooms

        [HttpPost]
        public async Task<IActionResult> SearchRoom(SearchRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("SearchRoom", model);
            }

            var availableRooms = await _context.Rooms
              .AsNoTracking()
              .Where(r =>
                  r.MaxOccupancy >= model.MaxOccupancy &&
                  (model.SelectedRoomType == 0 || r.RoomType == model.SelectedRoomType) && // ✅ No more warning
                  r.Status == RoomStatus.Available &&
                  !_context.Reservations
                      .Where(res => res.RoomId == r.RoomId && res.Status != ReservationStatus.CheckedOut)
                      .Any(res => model.CheckInDate < res.CheckOutDate && model.CheckOutDate > res.CheckInDate)
              )
              .ToListAsync();


            model.AvailableRooms = availableRooms;
            return View("SearchRoom", model);
        }







        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Create
        public async Task<IActionResult> Create(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId);
            if (room == null)
            {
                return NotFound();
            }

            // Create the view model and populate with room details
            var reservationViewModel = new ReservationViewModel
            {
                RoomId = room.RoomId,
                TotalPrice = room.Price,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                RoomImage1 = room.Image1,
                RoomImage2 = room.Image2,
                RoomType = room.RoomType.ToString(),
                RoomDescription = room.Description,
                MaxOccupancy = room.MaxOccupancy,
                BookingReference = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()
            };

            return View(reservationViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Ensure UserId is set if the user is authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                model.UserId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (model.UserId == 0)
                {
                    TempData["Error"] = "User identification failed.";
                    return RedirectToAction("Index", "Home");
                }
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
                UserId = model.UserId,  // UserId will be set if authenticated
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

            try
            {
                // Save reservation and update room status in one transaction
                _context.Reservations.Add(reservation);
                _context.Rooms.Update(room);  // Update the room status as well
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Log exception (optional: Log to file, database, etc.)
                TempData["Error"] = "There was an error while processing your reservation. Please try again later.";
                return RedirectToAction("Index", "Home");
            }

            // Optionally store the reservation ID in TempData to retrieve later in the checkout process
            TempData["ReservationId"] = reservation.ReservationId;

            // Redirect to the checkout page
            return RedirectToAction("Checkout", "Payment", new { area = "FrontDesk" });
        }






        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Details
        public async Task<IActionResult> Details(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var viewModel = new ReservationDetailViewModel
            {
                ReservationId = reservation.ReservationId,
                BookingReference = reservation.BookingReference,
                GuestName = reservation.GuestName ?? "N/A",
                GuestEmail = reservation.GuestEmail ?? "N/A",
                GuestPhone = reservation.GuestPhone ?? "N/A",
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                Adults = reservation.Adults,
                Children = reservation.Children,
                TotalPrice = reservation.TotalPrice,
                SpecialRequest = reservation.SpecialRequest ?? string.Empty,
                IsPaid = reservation.IsPaid,
                Status = reservation.Status.ToString(),

                // Room details
                RoomNumber = reservation.Room?.RoomNumber,
                RoomType = reservation.Room?.RoomType.ToString(),
                RoomDescription = reservation.Room?.Description,
                RoomImage1 = reservation.Room?.Image1,

                // Payment details (if available)
                PaymentAmount = reservation.Payment?.Amount,
                PaymentMethod = reservation.Payment != null ? "GCash" : "Cash", // Adjust if you have more logic
                TransactionDate = reservation.Payment?.TransactionDate
            };

            return View(viewModel);
        }


        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Edit
        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var viewModel = new EditReservationViewModel
            {
                ReservationId = reservation.ReservationId,
                Status = reservation.Status,
                IsPaid = reservation.IsPaid
            };

            return View(viewModel);
        }

        // POST: Edit Reservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var reservation = await _context.Reservations.FindAsync(model.ReservationId);
            if (reservation == null)
            {
                return NotFound();
            }

            // Update only the fields allowed to be edited
            reservation.Status = model.Status;
            reservation.IsPaid = model.IsPaid;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Reservation updated successfully.";
            return RedirectToAction("Index");
        }


        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction("Index");
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reservation deleted successfully.";
            return RedirectToAction("Index");
        }

    }


}
