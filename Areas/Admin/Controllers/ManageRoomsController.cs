using HotelReservation.Areas.Admin.ViewModels;
using HotelReservation.Models;
using HotelReservation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")] // 🔹 Only Admins can access
    public class ManageRoomsController : Controller
    {
        private readonly ManageRoomService _roomService;

        public ManageRoomsController(ManageRoomService roomService)
        {
            _roomService = roomService;
        }

        // ✅ Room Listing (Index)
        public async Task<IActionResult> Index()
        {
            var rooms = await _roomService.GetAllRoomsAsync(); // 🔹 Fetch all rooms
            return View(rooms); //
        }


        // ✅ CREATE Room (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateRoomViewModel());
        }

        // ✅ CREATE Room (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fix the errors in the form.";
                return View(model);
            }

            bool success = await _roomService.CreateRoomAsync(model);
            if (!success)
            {
                TempData["Error"] = "Room number already exists.";
                return View(model);
            }

            TempData["Success"] = "Room created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ✅ UPDATE Room (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                TempData["Error"] = "Room not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditRoomViewModel
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                Price = room.Price,
                Status = room.Status,
                Description = room.Description,
                Amenities = room.Amenities,
                //ExistingImage1 = room.Image1,
                //ExistingImage2 = room.Image2
            };

            return View(model);
        }

        // ✅ UPDATE Room (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fix the errors in the form.";
                return View(model);
            }

            bool success = await _roomService.UpdateRoomAsync(model);
            if (!success)
            {
                TempData["Error"] = "Room update failed.";
                return View(model);
            }

            TempData["Success"] = "Room updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ✅ DELETE Room
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool success = await _roomService.DeleteRoomAsync(id);
            if (!success)
            {
                TempData["Error"] = "Room not found or could not be deleted.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Room deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
