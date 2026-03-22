//using Microsoft.AspNetCore.Mvc;
//using TienLuxury.Helpers;
//using TienLuxury.Models;
//using TienLuxury.Services;
//using TienLuxury.ViewModels;
//using MongoDB.Bson;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;

//namespace TienLuxury.Controllers
//{
//    public class ShoppingCartController : Controller
//    {
//        private readonly IInvoiceService _invoiceService;
//        private readonly IInvoiceDetailsService _invoiceDetailService;
//        private readonly IProductService _productService;
//        private readonly IVoucherService _voucherService;
//        private readonly DBContext _context;

//        public ShoppingCartController(
//            IInvoiceService invoiceService,
//            IInvoiceDetailsService invoiceDetailsService,
//            IProductService productService,
//            IVoucherService voucherService,
//            DBContext context)
//        {
//            _invoiceService = invoiceService;
//            _invoiceDetailService = invoiceDetailsService;
//            _productService = productService;
//            _voucherService = voucherService;
//            _context = context;
//        }

//        // GET: Xem giỏ hàng
//        public async Task<IActionResult> Index()
//        {
//            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? [];

//            OrderViewModel model = new()
//            {
//                Items = cart
//            };

//            if (User.Identity.IsAuthenticated)
//            {
//                AppUser user = null;

//                var userIdString = User.FindFirst("UserId")?.Value;
//                if (!string.IsNullOrEmpty(userIdString) && ObjectId.TryParse(userIdString, out ObjectId objectId))
//                {
//                    user = await _context.Set<AppUser>().FindAsync(objectId);
//                }

//                if (user == null)
//                {
//                    var email = User.FindFirst(ClaimTypes.Email)?.Value;
//                    if (!string.IsNullOrEmpty(email))
//                    {
//                        user = await _context.Set<AppUser>().FirstOrDefaultAsync(u => u.Email == email);
//                    }
//                }

//                if (user != null)
//                {
//                    Console.WriteLine($"--> Đã tìm thấy user: {user.FullName} - {user.PhoneNumber}");

//                    model.CustomerName = user.FullName;
//                    model.PhoneNumber = user.PhoneNumber;
//                    model.Email = user.Email;
//                }
//            }

//            return View(model);
//        }

//        public IActionResult Decrease(string productId)
//        {
//            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart");
//            if (cart != null)
//            {
//                var item = cart.FirstOrDefault(i => i.ProductID == productId.ToString());
//                if (item != null)
//                {
//                    item.Quantity--;
//                    if (item.Quantity <= 0) cart.Remove(item);
//                    HttpContext.Session.SetObjectAsJson("Cart", cart);
//                }
//            }
//            return RedirectToAction("Index");
//        }

//        public IActionResult Increase(ObjectId productId)
//        {
//            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart");
//            if (cart != null)
//            {
//                var item = cart.FirstOrDefault(i => i.ProductID == productId.ToString());
//                if (item != null)
//                {
//                    item.Quantity++;
//                    HttpContext.Session.SetObjectAsJson("Cart", cart);
//                }
//            }
//            return RedirectToAction("Index");
//        }

//        public IActionResult Remove(ObjectId productId)
//        {
//            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart");
//            if (cart != null)
//            {
//                var item = cart.FirstOrDefault(i => i.ProductID == productId.ToString());
//                if (item != null)
//                {
//                    cart.Remove(item);
//                    HttpContext.Session.SetObjectAsJson("Cart", cart);
//                }
//            }
//            return RedirectToAction("Index");
//        }

//        [HttpPost]
//        public async Task<IActionResult> CreateInvoice(OrderViewModel model)
//        {
//            if (!ModelState.IsValid) return RedirectToAction("Index");

//            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart");
//            if (cart == null || cart.Count == 0) return RedirectToAction("Index");

//            string pMethod = model.PaymentMethod?.ToLower() ?? "cod";

//            Invoice newInvoice = new()
//            {
//                CustomerName = model.CustomerName,
//                PhoneNumber = model.PhoneNumber,
//                Email = model.Email,
//                Address = model.Address,
//                CreatedDate = DateTime.Now,
//                PaymentMethod = model.PaymentMethod,
//                Status = pMethod == "bank" ? "Chờ thanh toán" : "Đang xử lý"
//            };

//            ObjectId invoiceId = await _invoiceService.CreateInvoice(newInvoice);
//            newInvoice.ID = invoiceId;

//            // 1. Tính Tổng tiền gốc (Chưa giảm)
//            decimal originalTotal = 0;
//            foreach (var c in cart)
//            {
//                InvoiceDetail detail = new()
//                {
//                    InvoiceId = invoiceId,
//                    ProductId = ObjectId.Parse(c.ProductID),
//                    Quantity = c.Quantity
//                };
//                originalTotal += c.ProductPrice * c.Quantity;

//                await _invoiceDetailService.CreateInvoiceDetail(detail);
//                await _productService.MinusQuantityInStock(ObjectId.Parse(c.ProductID), c.Quantity);
//            }

//            // 2. --- LOGIC VOUCHER (MỚI THÊM VÀO ĐÂY) ---
//            decimal discountAmount = 0;
//            decimal finalTotal = originalTotal;

//            // Kiểm tra xem khách có nhập mã không
//            if (!string.IsNullOrEmpty(model.AppliedVoucherCode))
//            {
//                try
//                {
//                    // Lấy UserId để check (bắt buộc phải đăng nhập mới dùng đc voucher)
//                    var userIdString = User.FindFirst("UserId")?.Value;
//                    if (!string.IsNullOrEmpty(userIdString))
//                    {
//                        // GỌI ATOMIC UPDATE: Vừa check, vừa trừ số lượng, vừa lấy tiền giảm
//                        // Hàm này an toàn tuyệt đối, không lo 2 người dùng cùng lúc
//                        discountAmount = await _voucherService.ApplyVoucherAsync(model.AppliedVoucherCode, userIdString, originalTotal);

//                        // Nếu hàm trên chạy qua được (không báo lỗi Exception) nghĩa là thành công
//                        newInvoice.VoucherCode = model.AppliedVoucherCode;
//                        newInvoice.DiscountAmount = discountAmount;

//                        finalTotal = originalTotal - discountAmount;
//                        if (finalTotal < 0) finalTotal = 0; // Không để âm tiền
//                    }
//                }
//                catch (Exception ex)
//                {
//                    // Nếu lỗi (Hết vé, chưa đủ điều kiện...) -> Bỏ qua voucher, tính giá gốc
//                    // Có thể log lỗi hoặc thông báo (nhưng ở đây đang POST nên ta cứ lờ đi để đơn hàng vẫn thành công)
//                    Console.WriteLine("Lỗi áp dụng voucher: " + ex.Message);
//                }
//            }
//            // ----------------------------------------------

//            // 3. Cập nhật Tổng tiền cuối cùng vào Database
//            await _invoiceService.UpdateTotal(newInvoice, finalTotal);
//            HttpContext.Session.Remove("Cart");

//            // Nếu chọn chuyển khoản ngân hàng
//            if (pMethod == "bank") // QR VietQR
//            {
//                return RedirectToAction("PaymentQR", new { invoiceId = invoiceId.ToString(), amount = finalTotal });
//            }
//            else if (pMethod == "momo") // QR Momo
//            {
//                return RedirectToAction("PaymentMomo", new { invoiceId = invoiceId.ToString(), amount = finalTotal });
//            }

//            // Nếu là COD
//            SuccessfulViewModel successfulViewModel = new()
//            {
//                Invoice = newInvoice,
//                Items = cart
//            };
//            return View("Successful", successfulViewModel);
//        }

//        // Hiển thị QR
//        public IActionResult PaymentQR(string invoiceId, decimal amount)
//        {
//            string bankId = "MB";
//            string accountNo = "0000956031794";
//            string template = "bRACcqN";
//            // Lấy 8 ký tự cuối của ID để nội dung chuyển khoản ngắn gọn
//            string shortId = invoiceId.Length > 8 ? invoiceId.Substring(invoiceId.Length - 8) : invoiceId;
//            string content = $"{shortId}";

//            string qrUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png?amount={amount}&addInfo={content}";

//            ViewBag.QrUrl = qrUrl;
//            ViewBag.Amount = amount;
//            ViewBag.Content = content;
//            ViewBag.InvoiceId = invoiceId;

//            return View();
//        }

//        // Hàm hiển thị QR Momo
//        public IActionResult PaymentMomo(string invoiceId, decimal amount)
//        {
//            // Thông tin nhận tiền Momo của BẠN
//            string momoPhoneNumber = "0966856765"; // Số điện thoại đăng ký Momo
//            string momoName = "PHAM LONG VU";      // Tên tài khoản Momo
//            string email = "pvu50711@gmail.com";   // Email

//            // Tạo nội dung chuyển khoản ngắn gọn (8 ký tự cuối mã đơn)
//            string shortId = invoiceId.Length > 8 ? invoiceId.Substring(invoiceId.Length - 8) : invoiceId;
//            string content = $"{shortId}";

//            // Chuỗi lệnh đặc biệt để App Momo nhận diện
//            string momoString = $"2|99|{momoPhoneNumber}|{momoName}|{email}|0|0|{amount}|{content}|transfer_myqr";

//            // Tạo link ảnh QR Code
//            string qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=400x400&data={System.Net.WebUtility.UrlEncode(momoString)}";

//            ViewBag.QrUrl = qrUrl;
//            ViewBag.Amount = amount;
//            ViewBag.Content = content;
//            ViewBag.InvoiceId = invoiceId;
//            ViewBag.MomoName = momoName;
//            ViewBag.MomoPhone = momoPhoneNumber;

//            return View();
//        }

//        // POST: Hủy đơn hàng & Khôi phục giỏ hàng
//        public async Task<IActionResult> CancelOrder(string invoiceId)
//        {
//            if (!ObjectId.TryParse(invoiceId, out ObjectId objectId))
//                return RedirectToAction("Index");

//            try
//            {
//                var invoice = await _invoiceService.GetInvoiceById(objectId);

//                if (invoice != null && (invoice.Status == "Chờ thanh toán" || invoice.Status == "Đang xử lý"))
//                {
//                    var details = await _context.Set<InvoiceDetail>()
//                                                .Where(d => d.InvoiceId == objectId)
//                                                .ToListAsync();

//                    var restoredCart = new List<CartItemViewModel>();

//                    foreach (var item in details)
//                    {
//                        var product = await _context.Set<Product>().FindAsync(item.ProductId);
//                        if (product != null)
//                        {
//                            product.QuantityInStock += item.Quantity;
//                            _context.Update(product);

//                            restoredCart.Add(new CartItemViewModel
//                            {
//                                ProductID = product.ID.ToString(),
//                                ProductName = product.ProductName,
//                                ProductPrice = (int)product.Price,
//                                Quantity = item.Quantity,
//                                ImagePath = product.ImagePath
//                            });
//                        }
//                        _context.Remove(item);
//                    }
//                    HttpContext.Session.SetObjectAsJson("Cart", restoredCart);
//                    _context.Remove(invoice);
//                    await _context.SaveChangesAsync();
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Lỗi hủy đơn: " + ex.Message);
//            }

//            return RedirectToAction("Index", "ShoppingCart");
//        }

//        public async Task<IActionResult> Successful(string invoiceId)
//        {
//            if (!string.IsNullOrEmpty(invoiceId) && ObjectId.TryParse(invoiceId, out ObjectId objectId))
//            {
//                var invoice = await _invoiceService.GetInvoiceById(objectId);

//                if (invoice != null)
//                {
//                    var details = await _context.Set<InvoiceDetail>()
//                                                .Where(d => d.InvoiceId == objectId)
//                                                .ToListAsync();

//                    var items = new List<CartItemViewModel>();

//                    foreach (var d in details)
//                    {
//                        var product = await _productService.GetProductById(d.ProductId);
//                        if (product != null)
//                        {
//                            items.Add(new CartItemViewModel
//                            {
//                                ProductName = product.ProductName,
//                                ProductPrice = (int)product.Price,
//                                Quantity = d.Quantity,
//                                ImagePath = product.ImagePath
//                            });
//                        }
//                    }

//                    var model = new SuccessfulViewModel
//                    {
//                        Invoice = invoice,
//                        Items = items
//                    };

//                    return View(model);
//                }
//            }

//            return View();
//        }

//        // Hàm xử lý nút "Tôi đã thanh toán"
//        public async Task<IActionResult> ConfirmPayment(string invoiceId)
//        {
//            if (!string.IsNullOrEmpty(invoiceId) && ObjectId.TryParse(invoiceId, out ObjectId objectId))
//            {
//                // 1. Tìm đơn hàng
//                var invoice = await _invoiceService.GetInvoiceById(objectId);

//                if (invoice != null)
//                {
//                    invoice.Status = "Đã thanh toán";

//                    // (Gọi hàm UpdateStatusInvoice có sẵn trong Service của bạn)
//                    await _invoiceService.UpdateStatusInvoice(invoice);
//                }
//            }

//            return RedirectToAction("Successful", new { invoiceId = invoiceId });
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using TienLuxury.Helpers;
using TienLuxury.Models;
using TienLuxury.Services;
using TienLuxury.ViewModels;
using MongoDB.Bson;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TienLuxury.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceDetailsService _invoiceDetailService;
        private readonly IProductService _productService;
        private readonly IVoucherService _voucherService;
        private readonly DBContext _context;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(
            IInvoiceService invoiceService,
            IInvoiceDetailsService invoiceDetailsService,
            IProductService productService,
            IVoucherService voucherService,
            DBContext context,
            ILogger<ShoppingCartController> logger)
        {
            _invoiceService = invoiceService;
            _invoiceDetailService = invoiceDetailsService;
            _productService = productService;
            _voucherService = voucherService;
            _context = context;
            _logger = logger;
        }

        // GET: Xem giỏ hàng
        public async Task<IActionResult> Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            OrderViewModel model = new()
            {
                Items = cart
            };

            if (User.Identity.IsAuthenticated)
            {
                AppUser user = null;

                var userIdString = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userIdString) && ObjectId.TryParse(userIdString, out ObjectId objectId))
                {
                    user = await _context.Set<AppUser>().FindAsync(objectId);
                }

                if (user == null)
                {
                    var email = User.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(email))
                    {
                        user = await _context.Set<AppUser>().FirstOrDefaultAsync(u => u.Email == email);
                    }
                }

                if (user != null)
                {
                    model.CustomerName = user.FullName;
                    model.PhoneNumber = user.PhoneNumber;
                    model.Email = user.Email;
                }
            }

            // Nếu có lỗi voucher từ lần trước (TempData), đưa vào ViewBag để show
            if (TempData.ContainsKey("VoucherError"))
            {
                ViewBag.VoucherError = TempData["VoucherError"];
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice(OrderViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index");

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart");
            if (cart == null || cart.Count == 0) return RedirectToAction("Index");

            string pMethod = model.PaymentMethod?.ToLower() ?? "cod";

            // 1. Tính Tổng tiền gốc (Chưa giảm) - chỉ tính, chưa tạo invoice
            decimal originalTotal = 0;
            foreach (var c in cart)
            {
                originalTotal += c.ProductPrice * c.Quantity;
            }

            // 2. Xử lý voucher TRƯỚC KHI tạo invoice
            decimal discountAmount = 0;
            decimal finalTotal = originalTotal;
            string appliedCode = model.AppliedVoucherCode?.Trim();

            if (!string.IsNullOrEmpty(appliedCode))
            {
                // Kiểm tra user login
                var userIdString = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdString))
                {
                    ViewBag.ErrorMessage = "Vui lòng đăng nhập để sử dụng voucher.";
                    model.Items = cart;
                    return View("Index", model);
                }

                try
                {
                    discountAmount = await _voucherService.ApplyVoucherAsync(appliedCode, userIdString, originalTotal);
                    finalTotal = originalTotal - discountAmount;
                    if (finalTotal < 0) finalTotal = 0;
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = ex.Message;

                    model.Items = cart;

                    return View("Index", model);
                }
            }

            // 3. Tạo Invoice & InvoiceDetails (sau khi voucher đã được reserve nếu có)
            Invoice newInvoice = new()
            {
                CustomerName = model.CustomerName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Address = model.Address,
                CreatedDate = DateTime.UtcNow,
                PaymentMethod = model.PaymentMethod,
                Status = pMethod == "bank" ? "Chờ thanh toán" : "Đang xử lý",
                VoucherCode = appliedCode,
                DiscountAmount = discountAmount
            };

            ObjectId invoiceId = ObjectId.Empty;
            try
            {
                invoiceId = await _invoiceService.CreateInvoice(newInvoice);
                newInvoice.ID = invoiceId;

                // Lưu chi tiết đơn & trừ tồn kho
                foreach (var c in cart)
                {
                    InvoiceDetail detail = new()
                    {
                        InvoiceId = invoiceId,
                        ProductId = ObjectId.Parse(c.ProductID),
                        Quantity = c.Quantity
                    };

                    await _invoiceDetailService.CreateInvoiceDetail(detail);
                    await _productService.MinusQuantityInStock(ObjectId.Parse(c.ProductID), c.Quantity);
                }

                // Cập nhật tổng
                await _invoiceService.UpdateTotal(newInvoice, finalTotal);

                // Xóa giỏ hàng (chỉ khi mọi thứ thành công)
                HttpContext.Session.Remove("Cart");

                // Chuyển hướng theo phương thức thanh toán
                if (pMethod == "bank")
                {
                    return RedirectToAction("PaymentQR", new { invoiceId = invoiceId.ToString(), amount = finalTotal });
                }
                else if (pMethod == "momo")
                {
                    return RedirectToAction("PaymentMomo", new { invoiceId = invoiceId.ToString(), amount = finalTotal });
                }

                var successfulViewModel = new SuccessfulViewModel
                {
                    Invoice = newInvoice,
                    Items = cart
                };
                return View("Successful", successfulViewModel);
            }
            catch (Exception ex)
            {
                // Nếu đã reserve voucher trước đó, rollback để trả lại lượt
                if (!string.IsNullOrEmpty(appliedCode))
                {
                    try
                    {
                        var userIdString = User.FindFirst("UserId")?.Value;
                        if (!string.IsNullOrEmpty(userIdString))
                        {
                            await _voucherService.RollbackVoucherReserveAsync(appliedCode, userIdString);
                        }
                    }
                    catch (Exception rbEx)
                    {
                        // Log rollback thất bại
                        _logger.LogError(rbEx, "Rollback voucher failed for code {Code} user {User}", appliedCode, User.Identity.Name);
                    }
                }

                ViewBag.ErrorMessage = "Có lỗi hệ thống khi tạo đơn hàng. Vui lòng thử lại.";
                model.Items = cart; // Nạp lại cart
                return View("Index", model);
            }
        }

        // ... các action khác giữ nguyên (PaymentQR, PaymentMomo, CancelOrder, Successful, ConfirmPayment)
        public async Task<IActionResult> Successful(string invoiceId)
        {
            if (!string.IsNullOrEmpty(invoiceId) && ObjectId.TryParse(invoiceId, out ObjectId objectId))
            {
                var invoice = await _invoiceService.GetInvoiceById(objectId);

                if (invoice != null)
                {
                    var details = await _context.Set<InvoiceDetail>()
                                                .Where(d => d.InvoiceId == objectId)
                                                .ToListAsync();

                    var items = new List<CartItemViewModel>();

                    foreach (var d in details)
                    {
                        var product = await _productService.GetProductById(d.ProductId);
                        if (product != null)
                        {
                            items.Add(new CartItemViewModel
                            {
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
            }

            return View();
        }

        // Hàm xử lý nút "Tôi đã thanh toán"
        public async Task<IActionResult> ConfirmPayment(string invoiceId)
        {
            if (!string.IsNullOrEmpty(invoiceId) && ObjectId.TryParse(invoiceId, out ObjectId objectId))
            {
                // 1. Tìm đơn hàng
                var invoice = await _invoiceService.GetInvoiceById(objectId);

                if (invoice != null)
                {
                    invoice.Status = "Đã thanh toán";

                    // (Gọi hàm UpdateStatusInvoice có sẵn trong Service của bạn)
                    await _invoiceService.UpdateStatusInvoice(invoice);
                }
            }

            return RedirectToAction("Successful", new { invoiceId = invoiceId });
        }
        // POST: Hủy đơn hàng & Khôi phục giỏ hàng
        public async Task<IActionResult> CancelOrder(string invoiceId)
        {
            if (!ObjectId.TryParse(invoiceId, out ObjectId objectId))
                return RedirectToAction("Index");

            try
            {
                var invoice = await _invoiceService.GetInvoiceById(objectId);

                if (invoice != null && (invoice.Status == "Chờ thanh toán" || invoice.Status == "Đang xử lý"))
                {
                    var details = await _context.Set<InvoiceDetail>()
                                                .Where(d => d.InvoiceId == objectId)
                                                .ToListAsync();

                    var restoredCart = new List<CartItemViewModel>();

                    foreach (var item in details)
                    {
                        var product = await _context.Set<Product>().FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.QuantityInStock += item.Quantity;
                            _context.Update(product);

                            restoredCart.Add(new CartItemViewModel
                            {
                                ProductID = product.ID.ToString(),
                                ProductName = product.ProductName,
                                ProductPrice = (int)product.Price,
                                Quantity = item.Quantity,
                                ImagePath = product.ImagePath
                            });
                        }
                        _context.Remove(item);
                    }
                    HttpContext.Session.SetObjectAsJson("Cart", restoredCart);
                    _context.Remove(invoice);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi hủy đơn: " + ex.Message);
            }

            return RedirectToAction("Index", "ShoppingCart");
        }
        // Hiển thị QR
        public IActionResult PaymentQR(string invoiceId, decimal amount)
        {
            string bankId = "MB";
            string accountNo = "0000956031794";
            string template = "bRACcqN";
            // Lấy 8 ký tự cuối của ID để nội dung chuyển khoản ngắn gọn
            string shortId = invoiceId.Length > 8 ? invoiceId.Substring(invoiceId.Length - 8) : invoiceId;
            string content = $"{shortId}";

            string qrUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png?amount={amount}&addInfo={content}";

            ViewBag.QrUrl = qrUrl;
            ViewBag.Amount = amount;
            ViewBag.Content = content;
            ViewBag.InvoiceId = invoiceId;

            return View();
        }

        // Hàm hiển thị QR Momo
        public IActionResult PaymentMomo(string invoiceId, decimal amount)
        {
            // Thông tin nhận tiền Momo của BẠN
            string momoPhoneNumber = "0966856765"; // Số điện thoại đăng ký Momo
            string momoName = "PHAM LONG VU";      // Tên tài khoản Momo
            string email = "pvu50711@gmail.com";   // Email

            // Tạo nội dung chuyển khoản ngắn gọn (8 ký tự cuối mã đơn)
            string shortId = invoiceId.Length > 8 ? invoiceId.Substring(invoiceId.Length - 8) : invoiceId;
            string content = $"{shortId}";

            // Chuỗi lệnh đặc biệt để App Momo nhận diện
            string momoString = $"2|99|{momoPhoneNumber}|{momoName}|{email}|0|0|{amount}|{content}|transfer_myqr";

            // Tạo link ảnh QR Code
            string qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=400x400&data={System.Net.WebUtility.UrlEncode(momoString)}";

            ViewBag.QrUrl = qrUrl;
            ViewBag.Amount = amount;
            ViewBag.Content = content;
            ViewBag.InvoiceId = invoiceId;
            ViewBag.MomoName = momoName;
            ViewBag.MomoPhone = momoPhoneNumber;

            return View();
        }
        public IActionResult Decrease(string productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart");
            if (cart != null)
            {
                var item = cart.FirstOrDefault(i => i.ProductID == productId.ToString());
                if (item != null)
                {
                    item.Quantity--;
                    if (item.Quantity <= 0) cart.Remove(item);
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Increase(ObjectId productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart");
            if (cart != null)
            {
                var item = cart.FirstOrDefault(i => i.ProductID == productId.ToString());
                if (item != null)
                {
                    item.Quantity++;
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Remove(ObjectId productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart");
            if (cart != null)
            {
                var item = cart.FirstOrDefault(i => i.ProductID == productId.ToString());
                if (item != null)
                {
                    cart.Remove(item);
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                }
            }
            return RedirectToAction("Index");
        }
    }
}
