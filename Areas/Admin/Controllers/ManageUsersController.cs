using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using HotelReservation.Areas.Admin.ViewModels;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")] // 🔹 Only Admins can access
    public class ManageUsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManageUsersController(ApplicationDbContext context)
        {
            _context = context;
        }
            
        /******************************************************************************************/ 
        // Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Where(u => u.Role == UserRole.Housekeeping || u.Role == UserRole.FrontDesk)
                .ToListAsync();

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

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                TempData["Error"] = "This email is already in use.";
                return View(model);
            }

            if (model.Role != UserRole.Housekeeping && model.Role != UserRole.FrontDesk)
            {
                TempData["Error"] = "Invalid role selection.";
                return View(model);
            }

            string? profilePicturePath = null;

            if (model.ProfilePicture != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                // ✅ Ensure the uploads directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // ✅ Generate unique filename with original extension
                string fileExtension = Path.GetExtension(model.ProfilePicture.FileName);
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // ✅ Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                // ✅ Store relative path in the database for easy retrieval
                profilePicturePath = $"/uploads/{uniqueFileName}";
            }

            var newUser = new User
            {
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                ProfilePicture = profilePicturePath, // ✅ Save relative path
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User account created successfully.";
            return RedirectToAction(nameof(Index));
        }


        /******************************************************************************************/
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users
                .Where(u => u.Role == UserRole.Housekeeping || u.Role == UserRole.FrontDesk) // ✅ Restrict editable roles
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                TempData["Error"] = "User not found or access denied.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ Convert User to EditAccountViewModel
            var viewModel = new EditAccountViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                CurrentProfilePicture = user.ProfilePicture // ✅ Ensure this is correctly mapped
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

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fix the form below.";
                model.CurrentProfilePicture = user.ProfilePicture; // ✅ Ensure image remains in UI
                return View(model);
            }

            if (model.Role != UserRole.Housekeeping && model.Role != UserRole.FrontDesk)
            {
                TempData["Error"] = "Invalid role selection.";
                model.CurrentProfilePicture = user.ProfilePicture; // ✅ Keep image in UI
                return View(model);
            }

            // 🔹 Update allowed fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.Phone;
            user.Role = model.Role;

            // ✅ Handle Profile Picture Upload
            if (model.ProfilePicture != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                // ✅ Ensure the uploads directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // ✅ Generate unique filename with original extension
                string fileExtension = Path.GetExtension(model.ProfilePicture.FileName);
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                // ✅ Delete old profile picture if it exists
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // ✅ Save the new file
                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                // ✅ Store relative path in database
                user.ProfilePicture = $"/uploads/{uniqueFileName}";
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction("Index");
        }

        /******************************************************************************************/
        // Delete



    }
}
