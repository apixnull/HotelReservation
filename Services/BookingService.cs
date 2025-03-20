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

        public async Task<BookingViewModel?> GetBookingDetails(int id, ClaimsPrincipal user)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return null;

            var viewModel = new BookingViewModel
            {
                RoomId = room.RoomId,
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(1),
                Adults = 1,
                Children = 0,
                TotalPrice = room.Price
            };

            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var userInfo = await _context.Users.FindAsync(userId);
                if (userInfo != null)
                {
                    viewModel.GuestName = $"{userInfo.FirstName} {userInfo.LastName}";
                    viewModel.GuestEmail = userInfo.Email;
                    viewModel.GuestPhone = userInfo.Phone ?? string.Empty;
                }
            }
            return viewModel;
        }

        public async Task<Reservation?> CreateBooking(BookingViewModel model, ClaimsPrincipal user)
        {
            if (!_context.Rooms.Any(r => r.RoomId == model.RoomId && r.Status == RoomStatus.Available))
            {
                return null;
            }

            int? userId = null;
            if (user.Identity?.IsAuthenticated == true && int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int parsedUserId))
            {
                userId = parsedUserId;
            }

            var reservation = new Reservation
            {
                RoomId = model.RoomId,
                UserId = userId,
                GuestName = user.Identity?.IsAuthenticated == true ? null : model.GuestName,
                GuestEmail = user.Identity?.IsAuthenticated == true ? null : model.GuestEmail,
                GuestPhone = user.Identity?.IsAuthenticated == true ? null : model.GuestPhone,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                Adults = model.Adults,
                Children = model.Children,
                TotalPrice = model.TotalPrice,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }
    }
}
