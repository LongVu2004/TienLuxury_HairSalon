using Microsoft.AspNetCore.Mvc;
using TienLuxury.Services;
using TienLuxury.ViewModels;
using TienLuxury.Models;
using MongoDB.Bson;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TienLuxury.Helpers;

namespace TienLuxury.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly DBContext _context;

        public ProductController(IProductService productService, DBContext context)
        {
            _productService = productService;
            _context = context;
        }

        // GET: Chi tiết sản phẩm
        public async Task<IActionResult> ProductDetail(string id)
        {
            // 1. Kiểm tra ID hợp lệ
            if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out ObjectId productId))
                return RedirectToAction("Index", "Home");

            // 2. Lấy thông tin sản phẩm
            var product = await _productService.GetProductById(productId);
            if (product == null) return NotFound();

            // 3. Lấy danh sách đánh giá của sản phẩm này (Sắp xếp mới nhất)
            var reviews = await _context.Set<ProductReview>()
                                        .Where(r => r.ProductId == productId && r.IsApproved == true)
                                        .OrderByDescending(r => r.CreatedAt)
                                        .ToListAsync();

            // 4. Lấy sản phẩm liên quan
            // FIX LỖI CS1061: Vì Service không có hàm GetProductsByType, ta lấy hết về rồi lọc
            var allProducts = await _productService.GetAllProduct();
            var relatedProducts = allProducts
                                  .Where(p => p.ProductType == product.ProductType && p.ID != productId)
                                  .Take(4)
                                  .ToList();

            // 5. Khởi tạo ViewModel
            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Products = relatedProducts, // Danh sách sản phẩm liên quan
                CartItemViewModel = new CartItemViewModel { ProductID = id, Quantity = 1 }, // Default cho giỏ hàng

                // Dữ liệu đánh giá
                Reviews = reviews,
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0,
                CanReview = false // Mặc định là ẩn form
            };

            if (User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirst("UserId")?.Value;
                if (ObjectId.TryParse(userIdString, out ObjectId userId))
                {
                    // Lấy thông tin User hiện tại để lấy Số điện thoại (Key để map với đơn hàng)
                    var user = await _context.Set<AppUser>().FindAsync(userId);

                    if (user != null && !string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        var paidInvoiceIds = await _context.Set<Invoice>()
                            .Where(i => i.PhoneNumber == user.PhoneNumber &&
                                       (i.Status == "Đã thanh toán" || i.Status == "Hoàn thành"))
                            .Select(i => i.ID)
                            .ToListAsync();

                        if (paidInvoiceIds.Any())
                        {
                            var hasBought = await _context.Set<InvoiceDetail>()
                                .AnyAsync(d => paidInvoiceIds.Contains(d.InvoiceId) && d.ProductId == productId);

                            var hasReviewed = reviews.Any(r => r.UserId == userId);

                            if (hasBought && !hasReviewed)
                            {
                                viewModel.CanReview = true;
                            }
                        }
                    }
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(ProductDetailViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng!";
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? [];
            var CartItem = cart.FirstOrDefault(c => c.ProductID == model.CartItemViewModel.ProductID);

            if (CartItem == null)
            {
                cart.Add(
                    new CartItemViewModel()
                    {
                        ProductID = model.CartItemViewModel.ProductID,
                        ProductName = model.CartItemViewModel.ProductName,
                        ProductPrice = model.CartItemViewModel.ProductPrice,
                        ImagePath = model.CartItemViewModel.ImagePath,
                        Quantity = model.CartItemViewModel.Quantity,
                    }
                );
            }
            else
            {
                CartItem.Quantity++;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult BuyNow(ProductDetailViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để mua ngay!";
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? [];
            var CartItem = cart.FirstOrDefault(c => c.ProductID == model.CartItemViewModel.ProductID);

            if (CartItem == null)
            {
                cart.Add(
                    new CartItemViewModel()
                    {
                        ProductID = model.CartItemViewModel.ProductID,
                        ProductName = model.CartItemViewModel.ProductName,
                        ProductPrice = model.CartItemViewModel.ProductPrice,
                        ImagePath = model.CartItemViewModel.ImagePath,
                        Quantity = model.CartItemViewModel.Quantity,
                    }
                );
            }
            else
            {
                CartItem.Quantity += model.CartItemViewModel.Quantity;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index", "ShoppingCart");
        }

        // POST: Gửi đánh giá
        [HttpPost]
        public async Task<IActionResult> SubmitReview(string productId, int rating, string comment)
        {
            // 1. Check đăng nhập
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            var userIdString = User.FindFirst("UserId")?.Value;
            var userAvatar = User.FindFirst("Avatar")?.Value;

            var userName = User.Identity.Name ?? User.FindFirst(ClaimTypes.Email)?.Value ?? "Khách hàng";

            if (ObjectId.TryParse(productId, out ObjectId pId) && ObjectId.TryParse(userIdString, out ObjectId uId))
            {
                var user = await _context.Set<AppUser>().FindAsync(uId);
                if (user != null) userName = user.FullName;

                var review = new ProductReview
                {
                    Id = ObjectId.GenerateNewId(),
                    ProductId = pId,
                    UserId = uId,
                    UserName = userName,
                    Avatar = string.IsNullOrEmpty(userAvatar) ? "/image/user-image/default-avatar.png" : userAvatar,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now,
                    IsApproved = false // Mặc định là chờ duyệt
                };

                // 3. Lưu vào DB
                _context.Add(review);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đánh giá của bạn đã được đăng!";
            }

            // Quay lại trang chi tiết sản phẩm
            return RedirectToAction("ProductDetail", new { id = productId });
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string searchTerm)
        {
            var allProducts = await _productService.GetAllProduct();

            var suggestions = allProducts
                .Where(p => p.ProductName.ToLower().Contains(searchTerm.ToLower()))
                .Select(p => new
                {
                    ID = p.ID.ToString(),
                    p.ProductName
                })
                .Take(5)
                .ToList();

            return Json(suggestions);
        }

        //public async Task<IActionResult> Index(string searchTerm)
        //{
        //    IEnumerable<Product> allProducts = await _productService.GetAllProduct();

        //    if (!string.IsNullOrEmpty(searchTerm))
        //    {
        //        string term = searchTerm.ToLower().Trim();
        //        allProducts = allProducts.Where(p => p.ProductName.ToLower().Contains(term));
        //    }

        //    ProductByTypeListViewModel model = new()
        //    {
        //        Hair = new List<Product>(),
        //        SkinCare = new List<Product>(),
        //        Others = new List<Product>(),
        //        QuantityInCart = (HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart")?.Count ?? 0),
        //        SearchTerm = searchTerm
        //    };

        //    foreach (var p in allProducts)
        //    {
        //        if (p.ProductType == "Mỹ phẩm tóc")
        //        {
        //            model.Hair.Add(p);
        //        }
        //        else
        //        {
        //            if (p.ProductType == "Skin care")
        //            {
        //                model.SkinCare.Add(p);
        //            }
        //            else
        //            {
        //                model.Others.Add(p);
        //            }
        //        }

        //    }

        //    return View(model);
        //}

        public async Task<IActionResult> Index(string searchTerm)
        {
            // 1. Lấy tất cả danh mục từ DB
            var categories = await _context.Categories.ToListAsync();

            // 2. Lấy tất cả sản phẩm (Có lọc theo tìm kiếm nếu cần)
            var productsQuery = _context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                productsQuery = productsQuery.Where(p => p.ProductName.Contains(searchTerm));
            }
            var allProducts = await productsQuery.ToListAsync();

            // 3. Khởi tạo ViewModel
            var viewModel = new ProductIndexViewModel
            {
                SearchTerm = searchTerm,
                // Code lấy số lượng giỏ hàng của bạn (giữ nguyên logic cũ)
                QuantityInCart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart")?.Sum(x => x.Quantity) ?? 0
            };

            // 4. GOM NHÓM SẢN PHẨM (Phần quan trọng nhất)
            foreach (var cat in categories)
            {
                string catIdString = cat.Id.ToString();
                // Lấy các sản phẩm có CategoryID trùng với ID của danh mục hiện tại
                // Lưu ý: Đảm bảo kiểu dữ liệu so sánh (cùng là ObjectId hoặc cùng là String)
                var productsInGroup = allProducts
                    .Where(p => p.CategoryId != null && p.CategoryId.ToString() == catIdString) // Giả sử trong Product có field CategoryId
                    .ToList();

                // Chỉ hiển thị nhóm này nếu có ít nhất 1 sản phẩm
                if (productsInGroup.Any())
                {
                    viewModel.CategoryGroups.Add(new CategoryGroupViewModel
                    {
                        CategoryId = catIdString,
                        CategoryName = cat.CategoryName, // Tên danh mục từ DB
                        Products = productsInGroup
                    });
                }
            }

            var validCategoryIds = categories.Select(c => c.Id.ToString()).ToList();

            var uncategorizedProducts = allProducts
                .Where(p => p.CategoryId == null || !validCategoryIds.Contains(p.CategoryId.ToString()))
                .ToList();

            if (uncategorizedProducts.Any())
            {
                viewModel.CategoryGroups.Add(new CategoryGroupViewModel
                {
                    CategoryId = "others",
                    CategoryName = "Sản phẩm khác",
                    Products = uncategorizedProducts
                });
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term))
                return Json(new List<object>());

            var products = await _productService.GetAllProduct();

            // Tìm kiếm và lấy 5 kết quả
            var matches = products
                .Where(p => p.ProductName.ToLower().Contains(term.ToLower().Trim()))
                .Select(p => new {
                    id = p.ID.ToString(),
                    name = p.ProductName,
                    image = p.ImagePath,
                    price = p.Price.ToString("N0") + " đ"
                })
                .Take(5)
                .ToList();

            return Json(matches);
        }
    }
}