// BookingService.cs
using System.Security.Claims;
using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Services
{
    public class BookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Room>> SearchAvailableRooms(int maxOccupancy, string? roomType)
        {
            var query = _context.Rooms.Where(r => r.Status == RoomStatus.Available);

            if (!string.IsNullOrEmpty(roomType) && Enum.TryParse(roomType, out RoomType parsedType))
            {
                query = query.Where(r => r.RoomType == parsedType);
            }

            query = query.Where(r => r.MaxOccupancy >= maxOccupancy);
            return await query.ToListAsync();
        }

        public async Task<Room?> GetRoomDetails(int id)
        {
            return await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == id);
        }
        // ✅ Create a new reservation
        public async Task<Reservation?> CreateReservation(CheckoutViewModel model, ClaimsPrincipal user)
        {
            // Ensure check-in date is in the future and check-out is after check-in
            if (model.CheckInDate < DateTime.Today || model.CheckOutDate <= model.CheckInDate)
            {
                return null; // 🚨 Invalid dates
            }

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == model.RoomId);
            if (room == null)
            {
                return null; // 🚨 Room not found
            }

            // Check if the room is already booked during the selected dates
            bool isRoomAvailable = !await _context.Reservations.AnyAsync(r =>
                r.RoomId == model.RoomId &&
                r.Status == ReservationStatus.Confirmed &&
                (
                    // Case 1: New booking starts within an existing booking
                    (model.CheckInDate >= r.CheckInDate && model.CheckInDate < r.CheckOutDate) ||
                    // Case 2: New booking ends within an existing booking
                    (model.CheckOutDate > r.CheckInDate && model.CheckOutDate <= r.CheckOutDate) ||
                    // Case 3: New booking fully contains an existing booking
                    (model.CheckInDate <= r.CheckInDate && model.CheckOutDate >= r.CheckOutDate)
                )
            );

            if (!isRoomAvailable)
            {
                return null; // 🚨 Room is already booked
            }

            // Get the logged-in user's ID
            int? userId = null;
            if (user.Identity?.IsAuthenticated == true)
            {
                string? userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            var reservation = new Reservation
            {
                RoomId = model.RoomId,
                UserId = userId,
                GuestName = model.GuestName,
                GuestEmail = model.GuestEmail,
                GuestPhone = model.GuestPhone,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                Adults = model.Adults,
                Children = model.Children,
                TotalPrice = model.TotalPrice,
                Status = ReservationStatus.Pending,
                BookingReference = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                SpecialRequest = model.SpecialRequest,
                CreatedAt = DateTime.UtcNow // ✅ Store the creation date
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }


    }
}
