using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TienLuxury.Models;
using TienLuxury.Services;
using TienLuxury.ViewModels;

namespace TienLuxury.Controllers
{
    public class HomeController(IEmployeeService employeeService, IServiceService serviceService) : Controller
    {

        private readonly IEmployeeService _employeeService = employeeService;
        private readonly IServiceService _serviceService = serviceService;

        // private readonly ILogger<HomeController> _logger;
        // public HomeController(ILogger<HomeController> logger)
        // {
        //     _logger = logger;
        // }

        public async Task<IActionResult> Index()
        {
            HomeViewModel model = new HomeViewModel
            {
                Employees = await _employeeService.GetAllEmployees(),
                // Services = _serviceService.GetAllServicesActivated().Where(s => s.ServiceName == "Đắp mặt nạ" || s.ServiceName == "Uốn định hình" || s.ServiceName == "Làm móng")
            };
            return View(model);
        }

        public async Task<IActionResult> Reservation(string PhoneNumber)
        {
            HttpContext.Session.SetString("PhoneNumber", PhoneNumber);
            return Json(new { success = true, RedirectUrl = Url.Action("Index", "reservation") });
        }

    }
}
