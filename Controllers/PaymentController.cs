using Microsoft.AspNetCore.Mvc;
using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.ViewModels;
using HotelReservation.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public PaymentController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;

        }

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


        // POST: /Payment/Checkout
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

            // Simulate payment processing
            var payment = new Payment
            {
                ReservationId = model.ReservationId,
                Reservation = reservation,
                Amount = model.TotalAmount,
                GCashNumber = model.GCashNumber,
                GCashTransactionId = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                Status = PaymentStatus.Success,
                TransactionDate = DateTime.UtcNow,
                PaidAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            // Update reservation status
            reservation.IsPaid = true;
            reservation.Status = ReservationStatus.Confirmed; // Mark as confirmed  


            // ✅ Update room status directly (no need for reservation.Room)
            var room = await _context.Rooms.FindAsync(reservation.RoomId); // Access room directly
            if (room != null)
            {
                room.Status = RoomStatus.Booked;
                room.LastStatusUpdate = DateTime.UtcNow;
            }


            await _context.SaveChangesAsync();

            var subject = "Reservation Confirmed - Your Booking Details";
            var message = $"<h3>Thank you for your payment!</h3>" +
              $"<p>Your reservation (Reference: {reservation.BookingReference}) is now confirmed.</p>" +
              $"<p>We have sent this confirmation to your email: {reservation.GuestEmail}.</p>" +
              $"<p>You can view your reservation summary by clicking the link below:</p>" +
              $"<a href='{Url.Action("MyReservationSummary", "Booking", new { id = reservation.ReservationId }, Request.Scheme)}'>View Reservation Summary</a>";

             
            // Send the confirmation email if the guest email is provided
            if (!string.IsNullOrWhiteSpace(reservation.GuestEmail))
            {
                await _emailService.SendEmailAsync(reservation.GuestEmail, subject, message);
            }

            Console.WriteLine($"Redirecting to PaymentSuccess with ID: {payment.PaymentId}");
            return RedirectToAction("PaymentSuccess", new { paymentId = payment.PaymentId });
        }

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
    }
}
