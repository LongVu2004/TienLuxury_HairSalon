using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using TienLuxury.Services;
using TienLuxury.Models;
using Microsoft.EntityFrameworkCore;
using TienLuxury.ViewModels;

namespace TienLuxury.Controllers
{
    [Authorize] // Phải đăng nhập mới được dùng mã
    public class VoucherController : Controller
    {
        private readonly IVoucherService _voucherService;
        private readonly DBContext _context;

        public VoucherController(IVoucherService voucherService, DBContext context)
        {
            _voucherService = voucherService;
            _context = context;
        }

        // GET: Lấy danh sách Voucher khả dụng để hiển thị lên Popup
        [HttpGet]
        public async Task<IActionResult> GetAvailableVouchers()
        {
            // Lấy UserId từ Token/Session
            var userId = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            // Gọi Service để lấy danh sách đã xử lý flag IsUsed
            var vouchers = await _voucherService.GetVouchersForUserAsync(userId);

            return Json(new { success = true, data = vouchers });
        }

        // API này dùng để kiểm tra mã và trả về số tiền giảm (Chưa trừ lượt dùng)
        [HttpPost]
        public async Task<IActionResult> CheckVoucher([FromBody] CheckVoucherRequest request)
        {
            try
            {
                // Lấy UserId hiện tại
                var userId = User.FindFirst("UserId")?.Value; // Hoặc ClaimTypes.NameIdentifier tùy cấu hình của bạn
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                // Gọi Service để tính toán (Lưu ý: Hàm này trong Service phải tách logic check ra khỏi logic update nếu muốn chỉ check)
                // Tuy nhiên, để đơn giản, ta sẽ query voucher lên và tự check nhẹ ở đây để báo giá trước

                var voucher = await _voucherService.GetVoucherByCodeAsync(request.Code); // Bạn cần thêm hàm này vào Service

                if (voucher == null || !voucher.IsValid())
                    return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn!" });

                if (request.OrderTotal < voucher.MinOrderAmount)
                    return Json(new { success = false, message = $"Đơn hàng phải từ {voucher.MinOrderAmount:N0}đ mới được dùng mã này!" });

                if (voucher.UsedByUserIds.Contains(userId))
                    return Json(new { success = false, message = "Bạn đã sử dụng mã này rồi!" });

                // Tính toán số tiền giảm
                decimal discount = 0;
                if (voucher.DiscountType == "FIXED")
                {
                    discount = voucher.Value;
                }
                else
                {
                    discount = request.OrderTotal * (voucher.Value / 100);
                    if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
                    {
                        discount = voucher.MaxDiscountAmount.Value;
                    }
                }

                // Đảm bảo không giảm quá số tiền đơn hàng (âm tiền)
                if (discount > request.OrderTotal) discount = request.OrderTotal;

                return Json(new
                {
                    success = true,
                    discountAmount = discount,
                    finalTotal = request.OrderTotal - discount,
                    message = "Áp dụng mã thành công!"
                });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }


    // Class DTO để nhận dữ liệu từ Client
    public class CheckVoucherRequest
    {
        public string Code { get; set; }
        public decimal OrderTotal { get; set; }
    }
}