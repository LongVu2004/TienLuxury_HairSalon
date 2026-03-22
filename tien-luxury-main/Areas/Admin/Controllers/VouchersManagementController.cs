using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TienLuxury.Areas.Admin.ViewModels;
using TienLuxury.Models;
using TienLuxury.Services;
using MongoDB.Bson;
using TienLuxury.Areas.Filter;

namespace TienLuxury.Areas.Admin.Controllers
{
    [Area("Admin")]
    [DesktopOnly]
    [Authorize(Roles = "Admin")]
    public class VouchersManagementController : Controller
    {
        private readonly IVoucherService _voucherService;

        public VouchersManagementController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        // 1. Danh sách Voucher
        public async Task<IActionResult> Index()
        {
            var vouchers = await _voucherService.GetAllVouchers();
            return View(vouchers);
        }

        // 2. Thêm mới (GET)
        [HttpGet]
        public IActionResult AddVoucher()
        {
            return View(new VoucherAddEditViewModel { Voucher = new Voucher() });
        }

        // 3. Thêm mới (POST)
        [HttpPost]
        public async Task<IActionResult> AddVoucher(VoucherAddEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Auto generate ID
                model.Voucher.Id = ObjectId.GenerateNewId();
                model.Voucher.Code = model.Voucher.Code.ToUpper().Trim(); // Viết hoa mã cho chuẩn

                await _voucherService.CreateVoucher(model.Voucher);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 4. Cập nhật (GET)
        [HttpGet]
        public async Task<IActionResult> UpdateVoucher(string id)
        {
            var voucher = await _voucherService.GetVoucherById(id);
            if (voucher == null) return RedirectToAction(nameof(Index));

            return View(new VoucherAddEditViewModel { Voucher = voucher });
        }

        // 5. Cập nhật (POST)
        [HttpPost]
        public async Task<IActionResult> UpdateVoucher(VoucherAddEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Voucher.Code = model.Voucher.Code.ToUpper().Trim();
                await _voucherService.UpdateVoucher(model.Voucher);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 6. Xóa (POST)
        [HttpPost]
        public async Task<IActionResult> DeleteVoucher(string id)
        {
            await _voucherService.DeleteVoucher(id);
            return RedirectToAction(nameof(Index));
        }
    }
}