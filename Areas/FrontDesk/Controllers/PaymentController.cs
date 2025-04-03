using HotelReservation.Areas.FrontDesk.ViewModels;
using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.Services;
using HotelReservation.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Policy = "FrontDeskOnly")]
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
        // GET: /Payment/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (TempData["ReservationId"] == null) 
            {
                return RedirectToAction("Search", "Booking");
            }

            int reservationId = Convert.ToInt32(TempData["ReservationId"]);
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null)
            {
                return NotFound();
            }

            var checkoutViewModel = new CheckoutViewModel
            {
                ReservationId = reservation.ReservationId,
                TotalAmount = reservation.TotalPrice
            };

            TempData.Keep("ReservationId");
            return View(checkoutViewModel);
        }



        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
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

            // 🔍 Check if a payment already exists for this reservation
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.ReservationId == model.ReservationId);

            if (existingPayment != null)
            {
                TempData["Error"] = "A payment has already been made for this reservation.";
                return View(model);
            }

            // ✅ Proceed with payment creation
            var payment = new Payment
            {
                ReservationId = model.ReservationId,
                Reservation = reservation,
                Amount = model.TotalAmount,
                PaymentMethod = model.PaymentMethod,
                Status = PaymentStatus.Success,
                TransactionDate = DateTime.UtcNow,
                PaidAt = DateTime.UtcNow
            };

            if (model.PaymentMethod == "GCash")
            {
                payment.GCashNumber = model.GCashNumber;
                payment.GCashTransactionId = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            }

            _context.Payments.Add(payment);

            // ✅ Mark reservation as paid
            reservation.IsPaid = true;
            reservation.Status = ReservationStatus.Confirmed;

            // ✅ Update room status (if applicable)
            var room = await _context.Rooms.FindAsync(reservation.RoomId);
            if (room != null)
            {
                room.Status = RoomStatus.Booked;
                room.LastStatusUpdate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // ✅ Send confirmation email
            if (!string.IsNullOrWhiteSpace(reservation.GuestEmail))
            {
                var subject = "Reservation Confirmed - Your Booking Details";
                var message = $"<h3>Thank you for your payment!</h3>" +
                              $"<p>Your reservation (Reference: {reservation.BookingReference}) is now confirmed.</p>" +
                              $"<p>We have sent this confirmation to your email: {reservation.GuestEmail}.</p>" +
                              $"<p><a href='{Url.Action("MyReservationSummary", "Booking", new { id = reservation.ReservationId }, Request.Scheme)}'>View Reservation Summary</a></p>";

                await _emailService.SendEmailAsync(reservation.GuestEmail, subject, message);
            }

            return RedirectToAction("PaymentSuccess", "Payment", new { area = "FrontDesk", paymentId = payment.PaymentId });
        }



        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        // Payment success redirection
        public async Task<IActionResult> PaymentSuccess(int paymentId)
        {
            // Retrieve the Payment record including its Reservation (and optionally Room details)
            var payment = await _context.Payments
                .Include(p => p.Reservation)
                .ThenInclude(r => r.Room)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                return NotFound();
            }

            // Return a view that shows the payment success details.
            return View(payment);
        }

        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
        /***********************************************************************************************************************/
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
    }
}
