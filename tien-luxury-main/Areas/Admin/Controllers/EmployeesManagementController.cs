using Microsoft.AspNetCore.Mvc;
using TienLuxury.Areas.Admin.ViewModels;
using TienLuxury.Areas.Filter;
using TienLuxury.Services;
using TienLuxury.ViewModels;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authorization;

namespace TienLuxury.Areas.Admin.Controllers
{
    //[AdminAuth]
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [DesktopOnly]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class EmployeesManagementController(IEmployeeService employeeService) : Controller
    {
        private readonly IEmployeeService _employeeService = employeeService;

        public string GetImagePath(IFormFile productImage)
        {
            try
            {
                // Tạo thư mục nếu chưa có
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/image/employee-image");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file duy nhất để tránh trùng lặp
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(productImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file vào thư mục
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    productImage.CopyTo(fileStream);
                }

                // Gán đường dẫn vào thuộc tính ImagePath
                return "/image/employee-image/" + uniqueFileName;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IActionResult> Index()
        {
            EmployeeListViewModel model = new EmployeeListViewModel()
            {
                Employees = await _employeeService.GetAllEmployees()
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult AddEmployee()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(EmployeeAddEditViewModel model, IFormFile employeeImage)
        {
            if (ModelState.IsValid)
            {
                model.Employee.ImagePath = GetImagePath(employeeImage);

                await _employeeService.AddEmployee(model.Employee);

                // return RedirectToAction("Index");
                return Json(new { success = true, RedirectUrl = Url.Action("Index", "EmployeesManagement") });
            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

            return Json(new { success = false, errors });
        }

        [HttpGet]
        public async Task<IActionResult> UpdateEmployee(ObjectId id)
        {
            EmployeeAddEditViewModel model = new()
            {
                Employee = await _employeeService.GetEmployee(id)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployee(EmployeeAddEditViewModel model, IFormFile employeeImage)
        {
            if (model.Employee.ImagePath == null)
            {
                if (ModelState.IsValid)
                {
                    model.Employee.ImagePath = GetImagePath(employeeImage);
                    await _employeeService.UpdateEmployee(model.Employee);

                    // return RedirectToAction("Index");
                    return Json(new { success = true, RedirectUrl = Url.Action("Index", "EmployeesManagement") });
                }
            }
            else
            {

                if (!string.IsNullOrEmpty(model.Employee.Name) && !string.IsNullOrEmpty(model.Employee.Position))
                {
                    if (employeeImage != null && model.Employee.ImagePath == null)
                    {
                        model.Employee.ImagePath = GetImagePath(employeeImage);
                    }

                    await _employeeService.UpdateEmployee(model.Employee);
                    return Json(new { success = true, RedirectUrl = Url.Action("Index", "EmployeesManagement") });
                }

            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

            return Json(new { success = false, errors });
        }

        [HttpGet]
        public IActionResult DeleteComfirmation()
            => View();

        public async Task<IActionResult> DeleteEmployee(ObjectId id)
        {
            await _employeeService.DeleteEmployee(id);

            return RedirectToAction("Index");
        }
    }
}