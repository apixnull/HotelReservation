using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace HotelReservation.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Search for available rooms
        public async Task<IActionResult> Search(DateTime CheckInDate, DateTime CheckOutDate, int Adults, int Children, string? RoomType)
        {
            // 🔹 Query available rooms
            var query = _context.Rooms
                .Where(r => r.Status == RoomStatus.Available);

            if (!string.IsNullOrEmpty(RoomType) && Enum.TryParse(RoomType, out RoomType parsedType))
            {
                query = query.Where(r => r.RoomType == parsedType);
            }

            var availableRooms = await query.ToListAsync();

            // ✅ Redirect with Fragment to Scroll
            return View( "Search" ,availableRooms);
        }

        public async Task<IActionResult> GetRoomDetails(int id)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
            {
                return Content("<p class='text-danger'>Room not found.</p>");
            }

            return PartialView("_RoomDetailsPartial", room);
        }


    }
}
