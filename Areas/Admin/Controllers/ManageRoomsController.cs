using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")] // 🔹 Only Admins can access
    public class ManageRoomsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
