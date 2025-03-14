using HotelReservation.Areas.Admin.ViewModels;
using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Services
{
    public class ManageUserService
    {
        private readonly ApplicationDbContext _context;

        public ManageUserService(ApplicationDbContext context)
        {
            _context = context;
        }

        /******************************************************************************************/
        // Get Users (Housekeeping & FrontDesk)
        public async Task<List<User>> GetUsersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Housekeeping || u.Role == UserRole.FrontDesk)
                .ToListAsync();
        }

        /******************************************************************************************/
        // Create User
        public async Task<bool> CreateUserAsync(CreateAccountViewModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return false; // Email already exists
            }

            if (model.Role != UserRole.Housekeeping && model.Role != UserRole.FrontDesk)
            {
                return false; // Invalid role
            }

            string? profilePicturePath = null;
            if (model.ProfilePicture != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string fileExtension = Path.GetExtension(model.ProfilePicture.FileName);
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }
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
                ProfilePicture = profilePicturePath,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return true;
        }

        /******************************************************************************************/
        // Get User by ID
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Housekeeping || u.Role == UserRole.FrontDesk)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        /******************************************************************************************/
        // Update User
        public async Task<bool> UpdateUserAsync(EditAccountViewModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return false; // User not found 
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;     
            user.Phone = model.Phone;
            user.Role = model.Role;     
            
            if(model.ProfilePicture != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string fileExtension = Path.GetExtension(model.ProfilePicture.FileName);
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                user.ProfilePicture = $"/uploads/{uniqueFileName}";
            }

            _context.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
        /******************************************************************************************/
        // Delete User
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);  
            if(user == null)
            {
                return false; // User not found
            }

            if(!string.IsNullOrEmpty(user.ProfilePicture))
            {
                string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
