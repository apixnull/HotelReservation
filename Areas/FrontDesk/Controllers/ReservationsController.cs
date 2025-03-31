using HotelReservation.Areas.FrontDesk.ViewModels;
using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.Services;
using HotelReservation.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace YourNamespace.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Policy = "FrontDesk,Admin")]
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

            return View(reservations);
        }

        public IActionResult SearchRoom()
        {
            return View(new SearchRoomViewModel());
        }




        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Search Rooms
        [HttpPost]
        public async Task<IActionResult> SearchRoom(SearchRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("SearchRoom", model);
            }

            // Fetch available rooms
            var availableRooms = await _context.Rooms
                .Where(r => r.MaxOccupancy >= model.MaxOccupancy &&
                            r.RoomType == model.SelectedRoomType &&
                            r.Status == RoomStatus.Available &&
                            !_context.Reservations.Any(res =>
                                res.RoomId == r.RoomId &&
                                res.Status != ReservationStatus.CheckedOut && // Only consider active reservations
                                ((model.CheckInDate >= res.CheckInDate && model.CheckInDate < res.CheckOutDate) ||
                                 (model.CheckOutDate > res.CheckInDate && model.CheckOutDate <= res.CheckOutDate))))
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
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            // Ensure minimum stay of 1 night
            int nights = (int)(checkOut - checkIn).TotalDays;
            if (nights < 1) nights = 1;

            var viewModel = new ReservationViewModel
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType.ToString(),
                RoomDescription = room.Description,
                MaxOccupancy = room.MaxOccupancy,
                RoomImage1 = room.Image1,
                RoomImage2 = room.Image2,
                TotalPrice = room.Price * nights,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                BookingReference = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = "Invalid reservation details: " + string.Join(", ", errors);
                return View(model);
            }

            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null)
            {
                TempData["Error"] = "Selected room is not available.";
                return View(model);
            }

            int nights = (int)(model.CheckOutDate - model.CheckInDate).TotalDays;
            if (nights < 1)
            {
                TempData["Error"] = "Check-Out Date must be after Check-In Date.";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.BookingReference))
            {
                model.BookingReference = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            }

            var reservation = new Reservation
            {
                RoomId = model.RoomId,
                UserId = model.UserId,
                GuestName = model.GuestName,
                GuestEmail = model.GuestEmail,
                GuestPhone = model.GuestPhone,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                Adults = model.Adults,
                Children = model.Children,
                TotalPrice = room.Price * nights,
                BookingReference = model.BookingReference,
                IsPaid = false,
                SpecialRequest = model.SpecialRequest,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            model.ReservationId = reservation.ReservationId;

            TempData["Success"] = "Reservation created successfully! Proceeding to CheckoutProcess.";
            return RedirectToAction("Index", "Payment", new { area = "FrontDesk", reservationId = model.ReservationId });
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
