using MongoDB.Bson;
using Microsoft.AspNetCore.Mvc;
using TienLuxury.Areas.Admin.ViewModels;
using TienLuxury.Areas.Admin.Services;
using TienLuxury.Areas.Filter;
using TienLuxury.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace TienLuxury.Areas.Admin.Controllers
{
    //[AdminAuth]
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    [DesktopOnly]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class HomeController(IAdminAccountService adminAccountService, IMessageService messageService,
        IServiceService serviceService, IProductService productService,
        IReservationService reservationService, IInvoiceService invoiceService) : Controller
    {
        private readonly IAdminAccountService _adminAccountService = adminAccountService;
        private readonly IServiceService _serviceService = serviceService;
        private readonly IProductService _productService = productService;
        private readonly IReservationService _reservationService = reservationService;
        private readonly IInvoiceService _invoiceService = invoiceService;
        private readonly IMessageService _messageService = messageService;

        public async Task<IActionResult> Index()
        {
            // --- KHÔNG CẦN CHECK SESSION NỮA VÌ [Authorize] ĐÃ LO RỒI ---

            var reservations = await _reservationService.GetAllReservation();
            var invoices = await _invoiceService.GetAllInvoices();
            var services = await _serviceService.GetAllServices();
            var products = await _productService.GetAllProduct();
            var messages = await _messageService.GetAllMessage();

            var today = DateTime.Now.Date;

            HomeViewModel model = new()
            {
                ReservationsToDay = reservations.Count(x => x.ReservationDate.ToLocalTime().Date == today),
                OrdersToday = invoices.Count(x => x.Status == "Đang xử lý"),
                NumberOfServices = services.Count(),
                NumberOfProducts = products.Count(),
            };

            model.NearestBooking = reservations
                .Where(x => x.ReservationDate.ToLocalTime() > DateTime.Now && x.ReservationStatus != "Hoàn thành" && x.ReservationStatus != "Hủy")
                .OrderBy(x => x.ReservationDate) // Cái nào gần nhất xếp trước
                .FirstOrDefault();

            // 2. Lấy đơn hàng mới nhất (Bất kể trạng thái)
            model.LatestOrder = invoices
                .OrderByDescending(x => x.CreatedDate) // Mới nhất lên đầu
                .FirstOrDefault();

            // 3. Lấy tin nhắn mới nhất
            model.LatestMessage = messages
                .OrderByDescending(x => x.CreatedAt) // Mới nhất lên đầu
                .FirstOrDefault();

            return View(model);
        }

        [HttpPost]
        public IActionResult SignUp(AdminAccountViewModel adminAccountViewModel)
        {
            _adminAccountService.CreateAccount(adminAccountViewModel.AdminAccount);

            return RedirectToAction("Index", "Login");
        }

        public async Task<IActionResult> Logout()
        {
            // Xóa Cookie xác thực
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Xóa sạch Session cho chắc
            HttpContext.Session.Clear();

            // Quay về trang Login chung
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        [HttpGet]
        public IActionResult ChangePassword()
            => PartialView();

        // API lấy dữ liệu doanh thu cho biểu đồ
        [HttpGet]
        public async Task<IActionResult> GetRevenueData(string type)
        {
            // 1. Lấy tất cả hóa đơn đã thanh toán
            var allInvoices = await _invoiceService.GetAllInvoices();
            var paidInvoices = allInvoices.Where(x => x.Status == "Đã thanh toán");
            // Lưu ý: Tôi tạm tính cả "Đang xử lý" để bạn test cho có dữ liệu, thực tế chỉ nên tính "Đã thanh toán"

            List<string> labels = new List<string>();
            List<decimal> data = new List<decimal>();

            var now = DateTime.Now;

            // 2. Xử lý logic theo loại thời gian
            switch (type)
            {
                case "day": // Lấy 30 ngày (để có dữ liệu cho việc cuộn chuột)
                    for (int i = 29; i >= 0; i--) 
                    {
                        var date = now.AddDays(-i).Date;
                        labels.Add(date.ToString("dd/MM"));
                        var revenue = paidInvoices.Where(x => x.CreatedDate.ToLocalTime().Date == date).Sum(x => x.Total);
                        data.Add(revenue);
                    }
                    break;

                case "month": // 12 tháng trong năm nay
                    for (int i = 1; i <= 12; i++)
                    {
                        labels.Add("T" + i);
                        var revenue = paidInvoices.Where(x => x.CreatedDate.ToLocalTime().Year == now.Year && x.CreatedDate.ToLocalTime().Month == i).Sum(x => x.Total);
                        data.Add(revenue);
                    }
                    break;

                case "year": // 5 năm gần nhất
                    for (int i = 4; i >= 0; i--)
                    {
                        var year = now.Year - i;
                        labels.Add(year.ToString());
                        var revenue = paidInvoices.Where(x => x.CreatedDate.ToLocalTime().Year == year).Sum(x => x.Total);
                        data.Add(revenue);
                    }
                    break;
            }

            return Json(new { labels = labels, data = data });
        }

        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                if (_adminAccountService.ChangePassword(ObjectId.Parse(HttpContext.Session.GetString("AdminID")), newPassword, oldPassword))
                {
                    return Json(new { success = true, message = "Thay đổi mật khẩu thành công." });
                }
                return Json(new { success = false, message = "Mật khẩu cũ không đúng." });
            }
            catch
            {
                throw;
            }

        }

        public IActionResult ServicesManagement()
            => RedirectToAction("Index", "ServicesManagement");


        public IActionResult ProductsManagement()
            => RedirectToAction("Index", "ProductsManagement");


        public IActionResult OrdersManagement()
            => RedirectToAction("Index", "OrdersManagement");


        public IActionResult ReservationsManagement()
            => RedirectToAction("Index", "ReservationsManagement");


        public IActionResult EmployeesManagement()
            => RedirectToAction("Index", "EmployeesManagement");


        public IActionResult MessagesManagement()
            => RedirectToAction("Index", "MessagesManagement");

    }
}