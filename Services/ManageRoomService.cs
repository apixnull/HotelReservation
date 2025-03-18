using HotelReservation.Data;
using HotelReservation.Models;
using HotelReservation.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Services
{
    public class ManageRoomService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ManageRoomService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ✅ CREATE ROOM
        public async Task<bool> CreateRoomAsync(CreateRoomViewModel model)
        {
            // Check if room number already exists
            if (await _context.Rooms.AnyAsync(r => r.RoomNumber == model.RoomNumber))
                return false;

            var newRoom = new Room
            {
                RoomNumber = model.RoomNumber,
                RoomType = model.RoomType,
                Price = model.Price,
                Status = model.Status,
                Description = model.Description,
                Amenities = model.Amenities,
                MaxOccupancy = model.MaxOccupancy // ✅ Added Max Occupancy
            };

            _context.Rooms.Add(newRoom);
            await _context.SaveChangesAsync();

            // Define the folder path: wwwroot/uploads/room_pictures/{RoomNumber}
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "room_pictures", newRoom.RoomNumber);
            Directory.CreateDirectory(folderPath);

            if (model.Image1 != null)
                newRoom.Image1 = await SaveImageAsync(model.Image1, folderPath);

            if (model.Image2 != null)
                newRoom.Image2 = await SaveImageAsync(model.Image2, folderPath);

            _context.Update(newRoom);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Room?> GetRoomByIdAsync(int id)
        {
            return await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == id);
        }

        // ✅ Fetch all rooms from the database
        public async Task<List<Room>> GetAllRoomsAsync()
        {
            return await _context.Rooms
                .AsNoTracking() // 🔹 Improves performance for read-only queries
                .ToListAsync();
        }

        // ✅ UPDATE ROOM
        public async Task<bool> UpdateRoomAsync(EditRoomViewModel model)
        {
            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null) return false;

            room.RoomType = model.RoomType;
            room.Price = model.Price;
            room.Status = model.Status;
            room.Description = model.Description;
            room.Amenities = model.Amenities;
            room.MaxOccupancy = model.MaxOccupancy; // ✅ Added Max Occupancy

            // Define folder path based on room number
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "room_pictures", room.RoomNumber);
            Directory.CreateDirectory(folderPath);

            // ✅ Replace Image1 if updated
            if (model.Image1 != null)
            {
                if (!string.IsNullOrEmpty(room.Image1))
                    DeleteImage(room.Image1);

                room.Image1 = await SaveImageAsync(model.Image1, folderPath);
            }

            // ✅ Replace Image2 if updated
            if (model.Image2 != null)
            {
                if (!string.IsNullOrEmpty(room.Image2))
                    DeleteImage(room.Image2);

                room.Image2 = await SaveImageAsync(model.Image2, folderPath);
            }

            _context.Update(room);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ DELETE ROOM
        public async Task<bool> DeleteRoomAsync(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return false;

            // Define the folder path for the room images
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "room_pictures", room.RoomNumber);
            if (Directory.Exists(folderPath))
                Directory.Delete(folderPath, true); // Recursive delete of folder and its files

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Save Image
        private async Task<string> SaveImageAsync(IFormFile file, string folderPath)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            string filePath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Extract the room folder name from the folderPath and build the relative URL
            string roomFolder = Path.GetFileName(folderPath);
            return $"/uploads/room_pictures/{roomFolder}/{uniqueFileName}";
        }

        // ✅ Delete Image
        private void DeleteImage(string imagePath)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
