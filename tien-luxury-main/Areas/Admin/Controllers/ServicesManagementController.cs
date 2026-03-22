using Microsoft.AspNetCore.Mvc;
using TienLuxury.Areas.Admin.ViewModels;
using TienLuxury.Areas.Filter;
using TienLuxury.Models;
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
    public class ServicesManagementController(IServiceService _serviceService) : Controller
    {
        public string GetImagePath(IFormFile serviceImage)
        {
            try
            {
                // Tạo thư mục nếu chưa có
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/image/service-image");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file duy nhất để tránh trùng lặp
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(serviceImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file vào thư mục
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    serviceImage.CopyTo(fileStream);
                }

                // Gán đường dẫn vào thuộc tính ImagePath
                return "/image/service-image/" + uniqueFileName;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IActionResult> Index()
        {
            ServicesListViewModel listViewModel = new ServicesListViewModel()
            {
                Services = await _serviceService.GetAllServices()
            };
            return View(listViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AddService()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> AddService(ServiceAddUpdateViewModel model, IFormFile serviceImage)
        {
            if (ModelState.IsValid)
            {

                model.Service.ImagePath = GetImagePath(serviceImage);

                await _serviceService.AddService(model.Service);

                // return RedirectToAction("Index");
                return Json(new { success = true, RedirectUrl = Url.Action("Index", "ServicesManagement") });

            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

            return Json(new { success = false, errors });
        }


        [HttpGet]
        public async Task<IActionResult> UpdateService(ObjectId? id)
        {
            var model = new ServiceAddUpdateViewModel()
            {
                Service = new Service()
            };

            if (id.HasValue)
            {
                var service = _serviceService.GetServiceByID(id);

                if (service != null)
                {
                    model.Service.ID = service.ID;
                    model.Service.ServiceName = service.ServiceName;
                    model.Service.Price = service.Price;
                    model.Service.Description = service.Description;
                    model.Service.IsActivated = service.IsActivated;
                    model.Service.ImagePath = service.ImagePath;
                    model.Service.ServiceType = service.ServiceType;
                }
            }
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateService(ServiceAddUpdateViewModel model, IFormFile serviceImage)
        {
            if (model.Service.ImagePath == null)
            {

                if (ModelState.IsValid)
                {
                    model.Service.ImagePath = GetImagePath(serviceImage);
                    await _serviceService.UpdateService(model.Service);

                    // return RedirectToAction("Index");
                    return Json(new { success = true, RedirectUrl = Url.Action("Index", "ServicesManagement") });
                }
            }
            else
            {
                if (serviceImage == null && model.Service.ImagePath != null && !string.IsNullOrEmpty(model.Service.ServiceName))
                {
                    await _serviceService.UpdateService(model.Service);
                    return Json(new { success = true, RedirectUrl = Url.Action("Index", "ServicesManagement") });
                }

                if (!string.IsNullOrEmpty(model.Service.ServiceName))
                {
                    model.Service.ImagePath = GetImagePath(serviceImage);
                    await _serviceService.UpdateService(model.Service);

                    return Json(new { success = true, RedirectUrl = Url.Action("Index", "ServicesManagement") });
                }
                else
                {
                    return Json(new { success = false, Message = "Tên dịch vụ không được để trống" });
                }

            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

            return Json(new { success = false, errors });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteService(ObjectId id)
        {
            ServiceDeleteViewModel model = new()
            {
                Id = id,
                Name = await _serviceService.GetServiceNameById(id)
            };
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteService(ObjectId? id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _serviceService.RemoveService(_serviceService.GetServiceByID(id));
                    return Json(new { success = true, message = "Xóa dịch vụ thành công" });
                }
                catch
                {
                    return Json(new { success = false, message = "Đã xảy ra lỗi" });
                }

            }

            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return Json(new { success = false, errors });
        }
    }
}