using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using HotelReservation.Areas.Admin.ViewModels;
using HotelReservation.Services;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")] // 🔹 Only Admins can access
    public class ManageUsersController : Controller
    {
        private readonly ManageUserService _manageUserService;
        public ManageUsersController(ManageUserService manageUserService)
        {
            _manageUserService = manageUserService;
        }

        /******************************************************************************************/
        // Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _manageUserService.GetUsersAsync();
            return View(users);
        }



        /******************************************************************************************/
        // Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fix the errors in the form.";
                return View(model);
            }

            if (!await _manageUserService.CreateUserAsync(model))
            {
                TempData["Error"] = "Email is already in use or role is invalid.";
                return View(model);
            }

            if (model.Role != UserRole.Housekeeping && model.Role != UserRole.FrontDesk)
            {
                TempData["Error"] = "Invalid role selection.";
                return View(model);
            }

            TempData["Success"] = "User account created successfully.";
            return RedirectToAction(nameof(Index));
        }


        /******************************************************************************************/
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _manageUserService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found or access denied.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new EditAccountViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                CurrentProfilePicture = user.ProfilePicture
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditAccountViewModel model)
        {
            if (id != model.UserId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fix the form below.";
                return View(model);
            }

            if (!await _manageUserService.UpdateUserAsync(model))
            {
                TempData["Error"] = "User update failed.";
                return View(model);
            }

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        /******************************************************************************************/
        // Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if(!await _manageUserService.DeleteUserAsync(id))
            {
                TempData["Error"] = "User Deletion Failed or User not found";
                return RedirectToAction(nameof(Index)); 
            }

            TempData["Success"] = "User deleted successfully.";     

            return RedirectToAction(nameof(Index));
        }

    }
}
