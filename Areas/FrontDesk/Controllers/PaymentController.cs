using HotelReservation.Areas.FrontDesk.ViewModels;
using HotelReservation.Data;
using HotelReservation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Policy = "FrontDesk,Admin")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public PaymentController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Index
        [HttpGet]
        public async Task<IActionResult> Index(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null) return NotFound();

            var viewModel = new PaymentViewModel
            {
                ReservationId = reservation.ReservationId,
                BookingReference = reservation.BookingReference,
                GuestName = reservation.GuestName!,
                TotalAmount = reservation.TotalPrice
            };

            return View(viewModel);
        }




        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Select Payment Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectPaymentMethod(PaymentViewModel model)
        {
            if (string.IsNullOrEmpty(model.PaymentMethod))
            {
                TempData["Error"] = "Please select a payment method.";
                return RedirectToAction("Index", new { reservationId = model.ReservationId });
            }

            return RedirectToAction("Checkout", new { reservationId = model.ReservationId, paymentMethod = model.PaymentMethod });
        }





        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  CHeckout
        [HttpGet]
        public async Task<IActionResult> Checkout(int reservationId, string paymentMethod)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null) return NotFound();

            var viewModel = new PaymentViewModel
            {
                ReservationId = reservation.ReservationId,
                BookingReference = reservation.BookingReference,
                GuestName = reservation.GuestName!,
                TotalAmount = reservation.TotalPrice,
                PaymentMethod = paymentMethod
            };

            return View(viewModel);
        }



        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Process Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
        {
            var reservation = await _context.Reservations.FindAsync(model.ReservationId);
            if (reservation == null) return NotFound();

            if (model.PaymentMethod == "GCash" && string.IsNullOrEmpty(model.GCashNumber))
            {
                TempData["Error"] = "GCash number is required.";
                return RedirectToAction("Checkout", new { reservationId = model.ReservationId, paymentMethod = model.PaymentMethod });
            }

            // ✅ Update reservation status
            reservation.IsPaid = true;
            await _context.SaveChangesAsync();

            // ✅ Send Email Confirmation
            if (!string.IsNullOrEmpty(reservation.GuestEmail))
            {
                string subject = "Reservation Confirmed - Your Booking Details";
                string? reservationSummaryUrl = Url.Action("Summary", "Reservation", new { id = reservation.ReservationId }, Request.Scheme);

                string message = $@"
                    <h3>Thank you for your payment!</h3>
                    <p>Your reservation (<strong>Reference: {reservation.BookingReference}</strong>) is now confirmed.</p>
                    <p>You can view your reservation summary by clicking the link below:</p>
                    <a href='{reservationSummaryUrl}'>View Reservation Summary</a>
                ";

                await _emailService.SendEmailAsync(reservation.GuestEmail, subject, message);
            }

            TempData["Success"] = "Payment successful! Confirmation email sent.";
            return RedirectToAction("Success", new { reservationId = model.ReservationId });
        }


        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Success Page
        [HttpGet]
        public async Task<IActionResult> Success(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null) return NotFound();

            var viewModel = new PaymentViewModel
            {
                ReservationId = reservation.ReservationId,
                BookingReference = reservation.BookingReference,
                GuestName = reservation.GuestName!,
                TotalAmount = reservation.TotalPrice
            };

            return View(viewModel);
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        //  Generate a Summary of the Reservation
        [HttpGet]
        public async Task<IActionResult> Summary(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return NotFound();

            var viewModel = new PaymentViewModel
            {
                ReservationId = reservation.ReservationId,
                BookingReference = reservation.BookingReference,
                GuestName = reservation.GuestName!,
                TotalAmount = reservation.TotalPrice,
                PaymentMethod = reservation.IsPaid ? "Paid" : "Pending",
                CheckInDate = reservation.CheckInDate, // got an errr here
                CheckOutDate = reservation.CheckOutDate, // got an error here as well
                RoomNumber = reservation.Room?.RoomNumber, // got an error here as well 
                RoomImage1 = reservation.Room?.Image1 // got an error here as we ll
            };

            return View(viewModel);
        }
    }
}
