using System.Drawing.Printing;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TienLuxury.Areas.Admin.ViewModels;
using TienLuxury.Areas.Filter;
using TienLuxury.Models;
using TienLuxury.Services;
using TienLuxury.ViewModels;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authorization;

namespace TienLuxury.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [DesktopOnly]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ProductsManagementController : Controller
    {
        private readonly IProductService _productService;
        private readonly DBContext _context; // Inject thêm DBContext để lấy Category

        public ProductsManagementController(IProductService productService, DBContext context)
        {
            _productService = productService;
            _context = context;
        }



        // Sửa Constructor để nhận thêm Context
        public async Task<IActionResult> Index()
        {
            // 1. Lấy dữ liệu
            var categories = await _context.Set<Category>().ToListAsync();
            var allProducts = await _productService.GetAllProduct();

            // Sử dụng ViewModel mới (ProductIndexViewModel) thay vì cái cũ
            var model = new ProductIndexViewModel
            {
                CategoryGroups = new List<CategoryGroupViewModel>()
            };

            var categorizedProductIds = new List<string>();

            // 2. Vòng lặp gom nhóm
            foreach (var cat in categories)
            {
                string catIdString = cat.Id.ToString();
                string catName = cat.CategoryName.Trim().ToLower();

                // LOGIC TÌM KIẾM KÉP: Khớp ID (mới) HOẶC Khớp Tên (cũ)
                var productsInGroup = allProducts
                    .Where(p =>
                        (p.CategoryId != null && p.CategoryId.ToString() == catIdString)
                        ||
                        (p.ProductType != null && p.ProductType.Trim().ToLower() == catName)
                    )
                    .ToList();

                if (productsInGroup.Any())
                {
                    model.CategoryGroups.Add(new CategoryGroupViewModel
                    {
                        CategoryId = catIdString,
                        CategoryName = cat.CategoryName,
                        Products = productsInGroup
                    });

                    categorizedProductIds.AddRange(productsInGroup.Select(p => p.ID.ToString()));
                }
            }

            // 3. Xử lý sản phẩm còn dư (Mục Khác)
            var uncategorizedProducts = allProducts
                .Where(p => !categorizedProductIds.Contains(p.ID.ToString()))
                .ToList();

            if (uncategorizedProducts.Any())
            {
                model.CategoryGroups.Add(new CategoryGroupViewModel
                {
                    CategoryId = "others",
                    CategoryName = "Chưa phân loại / Khác",
                    Products = uncategorizedProducts
                });
            }

            return View(model);
        }

        // --- HÀM HỖ TRỢ LẤY DANH MỤC ---
        private async Task<List<SelectListItem>> GetCategorySelectList()
        {
            var categories = await _context.Set<Category>().ToListAsync();
            return categories.Select(c => new SelectListItem
            {
                Text = c.CategoryName,
                Value = c.Id.ToString()
            }).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            // Load danh mục lên View
            ProductAddEditViewModel model = new ProductAddEditViewModel
            {
                Product = new Product(),
                CategoryList = await GetCategorySelectList()
            };
            return View(model);
        }

        public string GetImagePath(IFormFile productImage)
        {
            // 1. Kiểm tra Null (Tránh Crash)
            if (productImage == null) return null;

            try
            {
                // 2. Kiểm tra đuôi file (Chỉ cho phép ảnh)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(productImage.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new Exception("Chỉ chấp nhận định dạng ảnh (.jpg, .png, .gif, .webp)");
                }

                // 3. Kiểm tra dung lượng (Ví dụ: giới hạn 5MB)
                if (productImage.Length > 5 * 1024 * 1024)
                {
                    throw new Exception("File ảnh quá lớn (Tối đa 5MB)");
                }

                // 4. Tạo đường dẫn chuẩn
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "image", "product-image");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    productImage.CopyTo(fileStream);
                }

                return "/image/product-image/" + uniqueFileName;
            }
            catch
            {
                throw; // Ném lỗi ra để Controller xử lý
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductAddEditViewModel model, IFormFile productImage)
        {
            // Nếu không chọn ảnh thì báo lỗi (Tùy logic của bạn có bắt buộc ảnh không)
            if (productImage == null)
            {
                return Json(new { success = false, errors = new { Image = new[] { "Vui lòng chọn hình ảnh sản phẩm." } } });
            }

            if (model.Product.CategoryId != null)
            {
                var category = await _context.Set<Category>().FindAsync(model.Product.CategoryId);
                if (category != null)
                {
                    model.Product.ProductType = category.CategoryName; // Tự động điền ProductType
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    model.Product.ImagePath = GetImagePath(productImage);
                    await _productService.CreateProduct(model.Product);
                    return Json(new { success = true, RedirectUrl = Url.Action("Index", "ProductsManagement") });
                }
                catch (Exception ex)
                {
                    // Bắt lỗi định dạng ảnh sai từ hàm GetImagePath
                    return Json(new { success = false, errors = new { Image = new[] { ex.Message } } });
                }
            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

            return Json(new { success = false, errors });

        }

        [HttpGet]
        public async Task<IActionResult> UpdateProduct(ObjectId id)
        {
            var product = await _productService.GetProductById(id);

            ProductAddEditViewModel model = new()
            {
                Product = product,

                // --- BỔ SUNG DÒNG NÀY ĐỂ HIỂN THỊ DROPDOWN ---
                CategoryList = await GetCategorySelectList()
            };

            if (model.Product.CategoryId == null && !string.IsNullOrEmpty(model.Product.ProductType))
            {
                // Tìm xem trong danh sách Category có cái nào tên trùng với ProductType cũ không
                var match = model.CategoryList.FirstOrDefault(c => c.Text == model.Product.ProductType);
                if (match != null)
                {
                    // Nếu trùng thì gán ID vào để Dropdown tự chọn
                    model.Product.CategoryId = MongoDB.Bson.ObjectId.Parse(match.Value);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(ProductAddEditViewModel model, IFormFile productImage)
        {
            if (model.Product.CategoryId != null)
            {
                var category = await _context.Set<Category>().FindAsync(model.Product.CategoryId);
                if (category != null)
                {
                    model.Product.ProductType = category.CategoryName;
                }
            }

            if (model.Product.ImagePath == null)
            {

                if (ModelState.IsValid)
                {
                    model.Product.ImagePath = GetImagePath(productImage);
                    await _productService.UpdateProduct(model.Product);

                    // return RedirectToAction("Index");
                    return Json(new { success = true, RedirectUrl = Url.Action("Index", "ProductsManagement") });
                }
            }
            else
            {
                if (productImage == null && model.Product.ImagePath != null)
                {
                    await _productService.UpdateProduct(model.Product);
                    return Json(new { success = true, RedirectUrl = Url.Action("Index", "ProductsManagement") });
                }

                if (!string.IsNullOrEmpty(model.Product.ProductName))
                {
                    model.Product.ImagePath = GetImagePath(productImage);
                    await _productService.UpdateProduct(model.Product);

                    return Json(new { success = true, RedirectUrl = Url.Action("Index", "ProductsManagement") });
                }
                else
                {
                    return Json(new { success = false, Message = "Tên sản phẩm không được để trống" });
                }

            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

            return Json(new { success = false, errors });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(ObjectId id)
        {
            await _productService.DeleteProduct(await _productService.GetProductById(id));
            return RedirectToAction("Index");
        }

        // Thêm vào ProductsManagementController.cs

        [HttpGet]
        public async Task<IActionResult> MigrateData()
        {
            // 1. Lấy tất cả danh mục và sản phẩm
            var categories = await _context.Set<Category>().ToListAsync();
            var products = await _productService.GetAllProduct();

            int countUpdated = 0;

            foreach (var p in products)
            {
                // Chỉ xử lý những sản phẩm chưa có CategoryId nhưng có ProductType
                if (p.CategoryId == null && !string.IsNullOrEmpty(p.ProductType))
                {
                    // Tìm danh mục có tên trùng với ProductType (So sánh không phân biệt hoa thường)
                    var matchCat = categories.FirstOrDefault(c =>
                        c.CategoryName.Trim().ToLower() == p.ProductType.Trim().ToLower());

                    if (matchCat != null)
                    {
                        // Cập nhật ID danh mục vào sản phẩm
                        p.CategoryId = matchCat.Id;

                        // Gọi service update (hoặc context update tùy setup của bạn)
                        await _productService.UpdateProduct(p);
                        countUpdated++;
                    }
                }
            }

            return Content($"Đã cập nhật CategoryId thành công cho {countUpdated} sản phẩm!");
        }
    }
}
