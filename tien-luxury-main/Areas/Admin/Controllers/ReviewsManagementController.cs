using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TienLuxury.Areas.Admin.ViewModels;
using TienLuxury.Services; // Namespace chứa IReviewService

namespace TienLuxury.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReviewsManagementController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IProductService _productService;

        // Inject các Service vào
        public ReviewsManagementController(IReviewService reviewService, IProductService productService)
        {
            _reviewService = reviewService;
            _productService = productService;
        }

        // GET: Admin/ReviewsManagement
        public async Task<IActionResult> Index()
        {
            // 1. Gọi Service lấy Review
            var reviews = await _reviewService.GetAllReviewsAsync();

            // 2. Map sang ViewModel
            var viewModels = new List<ReviewListViewModel>();

            // Lấy toàn bộ sản phẩm để map tên (Cách này đơn giản, nếu nhiều SP quá thì tối ưu sau)
            var allProducts = await _productService.GetAllProduct(); // Giả sử bên ProductService bạn có hàm lấy list

            foreach (var r in reviews)
            {
                // Tìm tên sản phẩm từ service
                var product = allProducts.FirstOrDefault(p => p.ID == r.ProductId);

                viewModels.Add(new ReviewListViewModel
                {
                    Id = r.Id.ToString(),
                    ProductName = product != null ? product.ProductName : "Sản phẩm đã xóa",
                    ProductImage = product != null ? product.ImagePath : "",
                    UserName = r.UserName,
                    Avatar = r.Avatar,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    IsApproved = r.IsApproved ?? false
                });
            }

            return View(viewModels);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(string id)
        {
            await _reviewService.ApproveReviewAsync(id);
            TempData["SuccessMessage"] = "Đã duyệt đánh giá thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCensored(string id)
        {
            await _reviewService.ApproveReviewWithCensorshipAsync(id);
            TempData["SuccessMessage"] = "Đã che từ nhạy cảm và duyệt bài thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _reviewService.DeleteReviewAsync(id);
            TempData["SuccessMessage"] = "Đã xóa đánh giá!";
            return RedirectToAction(nameof(Index));
        }
    }
}