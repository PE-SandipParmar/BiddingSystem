using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BiddingSystem.Data;
using BiddingSystem.Models;

namespace RentManagement.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        // GET: Dashboard/Index
        [HttpGet]
        public async Task<IActionResult> Index(int? year)
        {
            return View();
        }


      

    }
}
