//using System.Threading.Tasks;
//using TienLuxury.Models;
//using TienLuxury.ViewModels;

//namespace TienLuxury.Services
//{
//    public interface IVoucherService
//    {
//        // Các hàm quản lý cơ bản (Cho Admin)
//        Task<List<Voucher>> GetAllVouchers();
//        Task CreateVoucher(Voucher voucher);
//        Task UpdateVoucher(Voucher voucher);
//        Task DeleteVoucher(string id);
//        Task<Voucher> GetVoucherById(string id);
//        Task<Voucher> GetVoucherByCodeAsync(string code);
//        Task<List<VoucherViewModel>> GetVouchersForUserAsync(string userId);

//        Task<decimal> ApplyVoucherAsync(string voucherCode, string userId, decimal orderTotal);
//    }
//}
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using TienLuxury.Models;
//using TienLuxury.ViewModels;

//namespace TienLuxury.Services
//{
//    public interface IVoucherService
//    {
//        // Các hàm quản lý cơ bản (Cho Admin)
//        Task<List<Voucher>> GetAllVouchers();
//        Task CreateVoucher(Voucher voucher);
//        Task UpdateVoucher(Voucher voucher);
//        Task DeleteVoucher(string id);
//        Task<Voucher> GetVoucherById(string id);
//        Task<Voucher> GetVoucherByCodeAsync(string code);
//        Task<List<VoucherViewModel>> GetVouchersForUserAsync(string userId);

//        // Áp voucher (atomic). Nếu thất bại => ném Exception với message rõ ràng.
//        Task<decimal> ApplyVoucherAsync(string voucherCode, string userId, decimal orderTotal);

//        // Rollback khi cần: nếu voucher đã được reserve (UsedCount tăng, userId push),
//        // thì gọi hàm này để giảm UsedCount và remove userId.
//        Task<bool> RollbackVoucherReserveAsync(string voucherCode, string userId);
//    }
//}

using System.Threading.Tasks;
using System.Collections.Generic;
using TienLuxury.Models;
using TienLuxury.ViewModels;

namespace TienLuxury.Services
{
    public interface IVoucherService
    {
        // --- CRUD (Admin) ---
        Task<List<Voucher>> GetAllVouchers();
        Task<Voucher> GetVoucherById(string id);
        Task CreateVoucher(Voucher voucher);
        Task UpdateVoucher(Voucher voucher);
        Task DeleteVoucher(string id);

        // --- Lấy voucher theo mã ---
        Task<Voucher> GetVoucherByCodeAsync(string code);

        // --- Lấy danh sách voucher có thể dùng (User) ---
        Task<List<VoucherViewModel>> GetVouchersForUserAsync(string userId);

        // --- Áp dụng voucher (Atomic MongoDB) ---
        Task<decimal> ApplyVoucherAsync(string voucherCode, string userId, decimal orderTotal);

        // --- Rollback voucher khi đặt hàng thất bại ---
        Task<bool> RollbackVoucherReserveAsync(string voucherCode, string userId);
    }
}

