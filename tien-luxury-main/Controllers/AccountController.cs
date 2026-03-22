using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TienLuxury.Models;
using MongoDB.Bson;
using TienLuxury.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System;

namespace TienLuxury.Controllers
{
    public class AccountController : Controller
    {
        private readonly DBContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(DBContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        // GET: Đăng ký
        public IActionResult Register()
        {
            return View();
        }

        // POST: Xử lý đăng ký
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Kiểm tra Email đã tồn tại chưa
            var existingUser = await _context.Set<AppUser>()
                                             .FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng!");
                return View(model);
            }

            // 2. Tạo user mới
            var newUser = new AppUser
            {
                Id = ObjectId.GenerateNewId(),
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                // Mã hóa mật khẩu
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Customer", // Quan trọng: Mặc định là khách hàng
                CreatedAt = DateTime.Now
            };

            // 3. Lưu vào MongoDB
            _context.Add(newUser);
            await _context.SaveChangesAsync();

            // 4. Đăng ký xong thì chuyển về trang đăng nhập
            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // GET: Đăng nhập
        public IActionResult Login()
        {
            // KIỂM TRA: Nếu có thông báo lỗi từ trang khác chuyển về (ví dụ từ trang Sản phẩm)
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            return View();
        }

        // POST: Đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Tìm user theo Email
            var user = await _context.Set<AppUser>().FirstOrDefaultAsync(u => u.Email == model.Email);

            // 2. Kiểm tra pass
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ViewBag.ErrorMessage = "Email hoặc mật khẩu không chính xác!"; // Dùng ErrorMessage cho giống Admin
                return View(model);
            }

            // 3. Tạo thẻ bài (Claims) định danh người dùng
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role), // Quan trọng để phân quyền
                new Claim("UserId", user.Id.ToString()),
                new Claim("Avatar", user.Avatar ?? "/image/user-image/default-avatar.png")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // 4. Ghi cookie vào trình duyệt (Đăng nhập thành công)
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            // 5. Chuyển hướng theo quyền
            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" }); // Nếu bạn có Area Admin
            }
            return RedirectToAction("Index", "Home");
        }

        // Đăng xuất
        public async Task<IActionResult> Logout()
        {
            // 1. Xóa Cookie đăng nhập
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // 2. Xóa Session
            HttpContext.Session.Clear();
            // 3. SỬA DÒNG NÀY: Chuyển hướng về Trang chủ (Home) thay vì Login
            return RedirectToAction("Index", "Home");
        }

        // GET: Hồ sơ cá nhân & Lịch sử
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !ObjectId.TryParse(userIdString, out ObjectId objectId))
                return RedirectToAction("Login");

            var user = await _context.Set<AppUser>().FindAsync(objectId);
            if (user == null) return RedirectToAction("Login");

            var phone = user.PhoneNumber;

            // Lấy hóa đơn
            var orders = string.IsNullOrEmpty(phone)
                ? new List<Invoice>()
                : await _context.Set<Invoice>()
                    .Where(i => i.PhoneNumber == phone)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

            // Lấy lịch hẹn
            var bookings = string.IsNullOrEmpty(phone)
                ? new List<Reservation>()
                : await _context.Set<Reservation>()
                    .Where(r => r.PhoneNumber == phone)
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();

            var model = new UserProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                // Xử lý ảnh: Nếu null thì lấy ảnh mặc định
                CurrentAvatar = string.IsNullOrEmpty(user.Avatar) ? "/image/user-image/default-avatar.png" : user.Avatar,
                OrderHistory = orders,
                BookingHistory = bookings
            };

            return View(model);
        }

        // POST: Cập nhật thông tin cá nhân
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !ObjectId.TryParse(userIdString, out ObjectId objectId))
                return RedirectToAction("Login");

            var user = await _context.Set<AppUser>().FindAsync(objectId);
            if (user != null)
            {
                // Cập nhật thông tin cơ bản
                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;

                if (model.AvatarUpload != null && model.AvatarUpload.Length > 0)
                {
                    try
                    {
                        // Xác định đường dẫn thư mục: wwwroot/images/user-image
                        // Sử dụng Path.Combine để nối chuỗi đường dẫn chuẩn theo hệ điều hành
                        string webRootPath = _webHostEnvironment.WebRootPath;
                        string folderName = "image/user-image";
                        string uploadPath = Path.Combine(webRootPath, "image", "user-image");

                        // Kiểm tra xem thư mục có tồn tại không, nếu không thì TẠO MỚI
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }
                        string extension = Path.GetExtension(model.AvatarUpload.FileName); 
                        string uniqueFileName = Guid.NewGuid().ToString() + extension; 
                        string filePath = Path.Combine(uploadPath, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.AvatarUpload.CopyToAsync(fileStream);
                        }
                        user.Avatar = "/" + folderName + "/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        // Nếu lỗi, in ra Console và không làm sập web
                        Console.WriteLine(" -------- LỖI UPLOAD ẢNH: " + ex.Message);
                        TempData["ErrorMessage"] = "Có lỗi khi lưu ảnh, vui lòng thử lại!";
                    }
                }

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            }

            return RedirectToAction("Profile");
        }

        // GET: Xem chi tiết đơn hàng từ lịch sử
        [Authorize]
        public async Task<IActionResult> OrderDetail(string invoiceId)
        {
            if (string.IsNullOrEmpty(invoiceId) || !ObjectId.TryParse(invoiceId, out ObjectId objectId))
            {
                return RedirectToAction("Profile");
            }
            // 1. Lấy thông tin đơn hàng
            var invoice = await _context.Set<Invoice>().FindAsync(objectId);
            if (invoice == null) return NotFound();

            // 2. Lấy chi tiết sản phẩm (Join bảng InvoiceDetail với Product)
            var details = await _context.Set<InvoiceDetail>()
                                        .Where(d => d.InvoiceId == objectId)
                                        .ToListAsync();

            var items = new List<CartItemViewModel>();

            foreach (var d in details)
            {
                var product = await _context.Set<Product>().FindAsync(d.ProductId);
                
                if (product != null)
                {
                    items.Add(new CartItemViewModel
                    {
                        ProductID = product.ID.ToString(),
                        ProductName = product.ProductName,
                        ProductPrice = (int)product.Price,
                        Quantity = d.Quantity,
                        ImagePath = product.ImagePath
                    });
                }
            }
            var model = new SuccessfulViewModel
            {
                Invoice = invoice,
                Items = items
            };

            return View(model);
        }

        // GET: Đổi mật khẩu
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !ObjectId.TryParse(userIdString, out ObjectId objectId))
                return RedirectToAction("Login");

            var user = await _context.Set<AppUser>().FindAsync(objectId);
            if (user == null) return RedirectToAction("Login");

            // Truyền cờ báo hiệu user này đã có mật khẩu hay chưa
            ViewBag.HasPassword = !string.IsNullOrEmpty(user.PasswordHash);

            return View();
        }

        // POST: Đổi mật khẩu
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            // Kiểm tra các trường khác (Mật khẩu mới, Xác nhận...)
            if (!ModelState.IsValid)
            {
                // Mẹo: Thêm dòng này để giữ lại trạng thái UI (Ẩn hiện ô mật khẩu cũ)
                var uId = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(uId) && ObjectId.TryParse(uId, out ObjectId oId))
                {
                    var u = await _context.Set<AppUser>().FindAsync(oId);
                    ViewBag.HasPassword = u != null && !string.IsNullOrEmpty(u.PasswordHash);
                }
                return View(model);
            }

            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !ObjectId.TryParse(userIdString, out ObjectId objectId))
                return RedirectToAction("Login");

            var user = await _context.Set<AppUser>().FindAsync(objectId);
            if (user == null) return RedirectToAction("Login");

            // --- LOGIC XỬ LÝ MẬT KHẨU CŨ ---

            // 1. Nếu tài khoản VỐN ĐÃ CÓ mật khẩu (User thường)
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                // Bắt buộc phải nhập mật khẩu cũ
                if (string.IsNullOrEmpty(model.OldPassword))
                {
                    ModelState.AddModelError("OldPassword", "Vui lòng nhập mật khẩu cũ để xác thực");
                    ViewBag.HasPassword = true;
                    return View(model);
                }

                // Kiểm tra mật khẩu cũ có đúng không
                if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
                {
                    ModelState.AddModelError("OldPassword", "Mật khẩu cũ không chính xác!");
                    ViewBag.HasPassword = true;
                    return View(model);
                }
            }
            // 2. Nếu tài khoản CHƯA CÓ mật khẩu (Google/Facebook)
            else
            {
                // Bỏ qua bước kiểm tra OldPassword -> Cho phép chạy tiếp xuống dưới
            }

            // --- LOGIC LƯU MẬT KHẨU MỚI ---

            // Mã hóa mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật mật khẩu thành công! Vui lòng đăng nhập lại.";

            // Đăng xuất để bắt đăng nhập lại bằng mật khẩu mới
            return RedirectToAction("Logout");
        }

        // Đăng nhập bằng tài khoản Google
        public IActionResult LoginByGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            // "Google" là tên mặc định của scheme
            return Challenge(properties, "Google");
        }

        public async Task<IActionResult> GoogleResponse()
        {
            // Lấy thông tin từ Cookie tạm mà Google vừa ghi vào
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            // Lấy thông tin người dùng từ Google
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var avatarUrl = claims?.FirstOrDefault(c => c.Type == "picture" || c.Type == "urn:google:picture" || c.Type == "image")?.Value;

            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            // --- LOGIC TỰ ĐỘNG ĐĂNG KÝ / ĐĂNG NHẬP ---

            // Kiểm tra xem email này đã có trong DB chưa
            var user = await _context.Set<AppUser>().FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // CHƯA CÓ -> TỰ ĐỘNG TẠO TÀI KHOẢN MỚI
                user = new AppUser
                {
                    Id = ObjectId.GenerateNewId(),
                    Email = email,
                    FullName = name,
                    Role = "Customer",
                    CreatedAt = DateTime.Now,
                    PhoneNumber = "", // Google không trả về SĐT, user sẽ cập nhật sau
                    PasswordHash = "", // Không cần mật khẩu
                    Avatar = !string.IsNullOrEmpty(avatarUrl) ? avatarUrl : "/image/user-image/default-avatar.png"
                };
                _context.Add(user);
                await _context.SaveChangesAsync();
            }

            // ĐẾN ĐÂY LÀ ĐÃ CÓ USER (Cũ hoặc Mới) -> THỰC HIỆN ĐĂNG NHẬP VÀO HỆ THỐNG

            // Tạo Claims cho hệ thống của mình (giống hệt hàm Login thường)
            var myClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString()),
                new Claim("Avatar", user.Avatar ?? "/image/user-image/default-avatar.png")
            };

            var claimsIdentity = new ClaimsIdentity(myClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Ghi đè lại Cookie cũ bằng Cookie chính chủ của hệ thống
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return RedirectToAction("Index", "Home");
        }

        // Đăng nhập bằng tài khoản Facebook
        public IActionResult LoginByFacebook()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("FacebookResponse")
            };
            return Challenge(properties, "Facebook");
        }

        public async Task<IActionResult> FacebookResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) return RedirectToAction("Login");

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;

            // 1. Lấy thông tin cơ bản
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            // 2. Lấy Facebook ID chuẩn (Thử nhiều kiểu claim để chắc chắn lấy được)
            var facebookId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                             ?? claims?.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            // 3. Tạo link Avatar Facebook
            string avatarUrl = null;
            if (!string.IsNullOrEmpty(facebookId))
            {
                // Sử dụng Graph API để lấy ảnh lớn
                avatarUrl = $"https://graph.facebook.com/{facebookId}/picture?type=large";
            }

            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Không lấy được Email từ Facebook.";
                return RedirectToAction("Login");
            }

            // 4. Xử lý Database
            var user = await _context.Set<AppUser>().FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // Tạo mới
                user = new AppUser
                {
                    Id = ObjectId.GenerateNewId(),
                    Email = email,
                    FullName = name ?? "Facebook User",
                    Role = "Customer",
                    CreatedAt = DateTime.Now,
                    PhoneNumber = "",
                    PasswordHash = "",
                    // Ưu tiên lấy ảnh Facebook, nếu không có thì lấy ảnh mặc định
                    Avatar = !string.IsNullOrEmpty(avatarUrl) ? avatarUrl : "/image/user-image/default-avatar.png"
                };
                _context.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                // --- CẬP NHẬT (QUAN TRỌNG) ---
                // Nếu có link Facebook -> Cập nhật luôn vào DB (Bất kể ảnh cũ là gì)
                // Điều này giúp đồng bộ: Bạn đổi avatar bên FB -> Đăng nhập lại web -> Web tự đổi theo
                if (!string.IsNullOrEmpty(avatarUrl) && user.Avatar != avatarUrl)
                {
                    user.Avatar = avatarUrl;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
            }

            // 5. Tạo Cookie Mới (QUAN TRỌNG: Phải dùng user.Avatar vừa cập nhật)
            var myClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString()),
                
                // Lấy Avatar mới nhất từ biến user (đã được update ở trên)
                new Claim("Avatar", user.Avatar ?? "/image/user-image/default-avatar.png")
            };

            var claimsIdentity = new ClaimsIdentity(myClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Ghi đè Cookie cũ
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Xóa cũ
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal); // Ghi mới

            return RedirectToAction("Index", "Home");
        }

        // GET: Hiển thị trang đánh giá đơn hàng
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ReviewOrder(string invoiceId)
        {
            if (string.IsNullOrEmpty(invoiceId) || !ObjectId.TryParse(invoiceId, out ObjectId oId))
                return RedirectToAction("Profile");

            var invoice = await _context.Set<Invoice>().FindAsync(oId);
            if (invoice == null || (invoice.Status != "Đã thanh toán" && invoice.Status != "Hoàn thành"))
            {
                TempData["ErrorMessage"] = "Đơn hàng này chưa thể đánh giá!";
                return RedirectToAction("Profile");
            }

            var details = await _context.Set<InvoiceDetail>().Where(d => d.InvoiceId == oId).ToListAsync();

            var model = new OrderReviewViewModel
            {
                InvoiceId = invoiceId, // Lưu lại ID đơn hàng để dùng
                CreatedDate = invoice.CreatedDate,
                Items = new List<ProductReviewItem>()
            };

            foreach (var item in details)
            {
                var product = await _context.Set<Product>().FindAsync(item.ProductId);
                if (product != null)
                {
                    // --- LOGIC MỚI: Check xem đơn hàng này (oId) đã có đánh giá cho sp này chưa ---
                    var existingReview = await _context.Set<ProductReview>()
                        .FirstOrDefaultAsync(r => r.OrderId == oId && r.ProductId == item.ProductId);

                    model.Items.Add(new ProductReviewItem
                    {
                        ProductId = product.ID.ToString(),
                        ProductName = product.ProductName,
                        ProductImage = product.ImagePath,
                        // Nếu đã đánh giá ở đơn này rồi -> Hiện lại nội dung cũ
                        Rating = existingReview?.Rating ?? 5,
                        Comment = existingReview?.Comment ?? "",
                        IsReviewed = existingReview != null, // Biến này để View khóa ô nhập lại nếu muốn
                        IsApproved = existingReview != null ? existingReview.IsApproved ?? false : false
                    });
                }
            }

            return View(model);
        }

        // POST: Lưu đánh giá hàng loạt
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitOrderReviews(OrderReviewViewModel model)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            var userAvatar = User.FindFirst("Avatar")?.Value;
            var userName = User.Identity.Name;

            if (ObjectId.TryParse(userIdString, out ObjectId uId) && ObjectId.TryParse(model.InvoiceId, out ObjectId orderId))
            {
                var user = await _context.Set<AppUser>().FindAsync(uId);
                if (user != null) userName = user.FullName;

                foreach (var item in model.Items)
                {
                    if (ObjectId.TryParse(item.ProductId, out ObjectId pId))
                    {
                        // Kiểm tra đánh giá dựa trên OrderId + ProductId
                        var existingReview = await _context.Set<ProductReview>()
                            .FirstOrDefaultAsync(r => r.OrderId == orderId && r.ProductId == pId);

                        if (existingReview == null)
                        {
                            // CHƯA CÓ -> TẠO MỚI (Cho phép tạo nhiều đánh giá cho 1 sản phẩm, miễn là khác đơn hàng)
                            if (!string.IsNullOrEmpty(item.Comment)) // Chỉ lưu nếu có nội dung (tùy chọn)
                            {
                                var review = new ProductReview
                                {
                                    Id = ObjectId.GenerateNewId(),
                                    ProductId = pId,
                                    UserId = uId,
                                    UserName = userName,
                                    Avatar = userAvatar ?? "/image/user-image/default-avatar.png",
                                    Rating = item.Rating,
                                    Comment = item.Comment,
                                    CreatedAt = DateTime.Now,
                                    OrderId = orderId,
                                    IsApproved = false // Mặc định chờ duyệt
                                };
                                _context.Add(review);
                            }
                        }
                        //else
                        //{
                        //    if (existingReview.IsApproved.GetValueOrDefault() == false)
                        //    {
                        //        continue;
                        //    }
                        //    // ĐÃ CÓ (Trong cùng đơn hàng này) -> CẬP NHẬT LẠI
                        //    existingReview.Rating = item.Rating;
                        //    existingReview.Comment = item.Comment;
                        //    existingReview.CreatedAt = DateTime.Now;
                        //    existingReview.IsApproved = false;
                        //    _context.Update(existingReview);
                        //}
                    }
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đánh giá đơn hàng thành công! Vui lòng chờ quản trị viên phê duyệt.";
            }

            return RedirectToAction("ReviewOrder", new { invoiceId = model.InvoiceId });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
