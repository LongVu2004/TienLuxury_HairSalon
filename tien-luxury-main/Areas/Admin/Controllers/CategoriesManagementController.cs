//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using MongoDB.Bson;
//using System.Threading.Tasks;
//using TienLuxury.Areas.Admin.ViewModels;
//using TienLuxury.Areas.Filter;
//using TienLuxury.Models;

//namespace TienLuxury.Areas.Admin.Controllers
//{
//    [Area("Admin")]
//    [DesktopOnly]
//    [Authorize(Roles = "Admin")]
//    public class CategoriesManagementController : Controller
//    {
//        private readonly DBContext _context;

//        public CategoriesManagementController(DBContext context)
//        {
//            _context = context;
//        }

//        // 1. Danh sách danh mục
//        public async Task<IActionResult> Index()
//        {
//            var categories = await _context.Set<Category>().ToListAsync();
//            return View(categories);
//        }

//        // 2. Thêm danh mục (GET)
//        [HttpGet]
//        public IActionResult AddCategory()
//        {
//            return View(new CategoryAddEditViewModel { Category = new Category() });
//        }

//        // 3. Thêm danh mục (POST)
//        [HttpPost]
//        public async Task<IActionResult> AddCategory(CategoryAddEditViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                model.Category.Id = ObjectId.GenerateNewId();
//                _context.Add(model.Category);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(model);
//        }

//        // 4. Cập nhật danh mục (GET)
//        [HttpGet]
//        public async Task<IActionResult> UpdateCategory(string id)
//        {
//            if (ObjectId.TryParse(id, out ObjectId objId))
//            {
//                var category = await _context.Set<Category>().FindAsync(objId);
//                if (category != null)
//                {
//                    return View(new CategoryAddEditViewModel { Category = category });
//                }
//            }
//            return RedirectToAction(nameof(Index));
//        }

//        // 5. Cập nhật danh mục (POST)
//        [HttpPost]
//        public async Task<IActionResult> UpdateCategory(CategoryAddEditViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Update(model.Category);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(model);
//        }

//        // 6. Xóa danh mục
//        [HttpPost]
//        public async Task<IActionResult> Delete(string id)
//        {
//            if (!ObjectId.TryParse(id, out ObjectId cateId))
//            {
//                TempData["Error"] = "ID danh mục không hợp lệ!";
//                return RedirectToAction("Index");
//            }

//            // 1. KIỂM TRA RÀNG BUỘC (QUAN TRỌNG)
//            // Kiểm tra xem có sản phẩm nào đang giữ CategoryId này không
//            var hasProduct = await _context.Set<Product>()
//                                           .AnyAsync(p => p.CategoryId == cateId);

//            if (hasProduct)
//            {
//                // Nếu có sản phẩm -> CHẶN KHÔNG CHO XÓA
//                TempData["Error"] = "Không thể xóa! Danh mục này đang chứa sản phẩm. Vui lòng xóa hoặc chuyển sản phẩm sang danh mục khác trước.";
//                return RedirectToAction("Index");
//            }

//            // 2. Nếu không có sản phẩm -> TIẾN HÀNH XÓA
//            var category = await _context.Set<Category>().FindAsync(cateId);
//            if (category != null)
//            {
//                _context.Remove(category);
//                await _context.SaveChangesAsync();
//                TempData["Success"] = "Đã xóa danh mục thành công!";
//            }
//            else
//            {
//                TempData["Error"] = "Không tìm thấy danh mục cần xóa.";
//            }

//            return RedirectToAction("Index");
//        }
//    }
//}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TienLuxury.Models;
using TienLuxury.Services; // Thêm namespace này
using MongoDB.Bson;
using System.Threading.Tasks;

namespace TienLuxury.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesManagementController : Controller
    {
        private readonly DBContext _context;
        private readonly IProductService _productService; // 1. Khai báo Service

        // 2. Inject Service vào Constructor
        public CategoriesManagementController(DBContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Set<Category>().ToListAsync();
            return View(categories);
        }

        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                await _context.AddAsync(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục thành công!";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        public async Task<IActionResult> UpdateCategory(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId cateId)) return NotFound();
            var category = await _context.Set<Category>().FindAsync(cateId);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // --- 3. LOGIC XÓA ĐÃ ĐƯỢC NÂNG CẤP ---
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId cateId))
            {
                TempData["Error"] = "ID không hợp lệ!";
                return RedirectToAction("Index");
            }

            // Dùng ProductService để lấy toàn bộ sản phẩm (đảm bảo đồng bộ với trang quản lý SP)
            var allProducts = await _productService.GetAllProduct();

            // Kiểm tra trong RAM: Có sản phẩm nào thuộc danh mục này không?
            // (Chuyển cả 2 về String để so sánh tuyệt đối chính xác)
            var hasProduct = allProducts.Any(p => p.CategoryId != null && p.CategoryId.ToString() == cateId.ToString());

            if (hasProduct)
            {
                TempData["Error"] = "Không thể xóa! Danh mục này đang chứa sản phẩm.";
                return RedirectToAction("Index");
            }

            // Nếu an toàn thì xóa
            var category = await _context.Set<Category>().FindAsync(cateId);
            if (category != null)
            {
                _context.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa danh mục!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
            }

            return RedirectToAction("Index");
        }
    }
}