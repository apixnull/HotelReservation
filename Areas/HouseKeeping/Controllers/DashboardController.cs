using HotelReservation.Areas.Housekeeping.ViewModels;
using HotelReservation.Data;
using HotelReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservation.Areas.Housekeeping.Controllers
{
    [Area("Housekeeping")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
