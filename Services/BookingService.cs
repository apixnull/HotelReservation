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
       

    }
}
