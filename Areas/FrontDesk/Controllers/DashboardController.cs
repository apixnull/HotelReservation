using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Roles = "FrontDesk")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
