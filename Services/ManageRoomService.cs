using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Services
{
    public class ManageRoomService
    {
        private readonly ApplicationDbContext _context;

        public ManageRoomService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Create a new room
    }
}
