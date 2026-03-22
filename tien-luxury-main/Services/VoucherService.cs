//using Microsoft.EntityFrameworkCore;
//using MongoDB.Bson;
//using MongoDB.Driver; // Cần thư viện này để dùng Builders
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using TienLuxury.Models;
//using TienLuxury.Data;
//using TienLuxury.ViewModels;

//namespace TienLuxury.Services
//{
//    public class VoucherService : IVoucherService
//    {
//        private readonly DBContext _context;
//        private readonly IMongoCollection<Voucher> _voucherCollection;

//        // Inject thêm IMongoDatabase để lấy Collection gốc xử lý Atomic Update
//        public VoucherService(DBContext context, IMongoDatabase database)
//        {
//            _context = context;
//            // Lấy collection "voucher" để thao tác trực tiếp
//            _voucherCollection = database.GetCollection<Voucher>("voucher");
//        }

//        // --- CÁC HÀM CRUD CƠ BẢN (Dùng EF Core cho tiện) ---
//        public async Task<List<Voucher>> GetAllVouchers()
//        {
//            return await _context.Vouchers.OrderByDescending(v => v.Id).ToListAsync();
//        }

//        public async Task<Voucher> GetVoucherById(string id)
//        {
//            if (ObjectId.TryParse(id, out ObjectId objId))
//            {
//                return await _context.Vouchers.FindAsync(objId);
//            }
//            return null;
//        }

//        public async Task CreateVoucher(Voucher voucher)
//        {
//            _context.Vouchers.Add(voucher);
//            await _context.SaveChangesAsync();
//        }

//        public async Task UpdateVoucher(Voucher voucher)
//        {
//            _context.Vouchers.Update(voucher);
//            await _context.SaveChangesAsync();
//        }

//        public async Task<Voucher> GetVoucherByCodeAsync(string code)
//        {
//            return await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == code);
//        }

//        public async Task DeleteVoucher(string id)
//        {
//            if (ObjectId.TryParse(id, out ObjectId objId))
//            {
//                var voucher = await _context.Vouchers.FindAsync(objId);
//                if (voucher != null)
//                {
//                    _context.Vouchers.Remove(voucher);
//                    await _context.SaveChangesAsync();
//                }
//            }
//        }

//        public async Task<List<VoucherViewModel>> GetVouchersForUserAsync(string userId)
//        {
//            var now = DateTime.Now;

//            var vouchers = await _context.Set<Voucher>()
//                .Where(v => v.IsActive == true
//                            && v.StartDate <= now
//                            && v.EndDate >= now
//                            && v.UsedCount < v.Quantity) 
//                .OrderBy(v => v.EndDate)
//                .ToListAsync();

//            var result = vouchers.Select(v => new VoucherViewModel
//            {
//                Code = v.Code,
//                Description = v.Description,
//                DiscountType = v.DiscountType,
//                Value = v.Value,
//                MinOrderAmount = v.MinOrderAmount,
//                MaxDiscountAmount = v.MaxDiscountAmount,
//                EndDate = v.EndDate,
//                IsUsed = v.UsedByUserIds != null && v.UsedByUserIds.Contains(userId)
//            }).ToList();

//            return result;
//        }

//        // --- LOGIC ATOMIC UPDATE (XỬ LÝ RACE CONDITION) ---
//        public async Task<decimal> ApplyVoucherAsync(string code, string userId, decimal orderTotal)
//        {
//            // 1. Lấy thông tin voucher (Chỉ để xem điều kiện cơ bản)
//            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == code);

//            // Kiểm tra cơ bản
//            if (voucher == null) throw new Exception("Voucher không tồn tại.");
//            if (!voucher.IsActive) throw new Exception("Voucher đã bị khóa.");
//            if (DateTime.Now < voucher.StartDate) throw new Exception("Voucher chưa đến ngày sử dụng.");
//            if (DateTime.Now > voucher.EndDate) throw new Exception("Voucher đã hết hạn.");
//            if (orderTotal < voucher.MinOrderAmount) throw new Exception($"Đơn hàng phải từ {voucher.MinOrderAmount:N0}đ mới được dùng.");

//            // Kiểm tra xem user này đã dùng chưa (Kiểm tra ở Client side trước để đỡ tốn query update)
//            if (voucher.UsedByUserIds.Contains(userId)) throw new Exception("Bạn đã sử dụng voucher này rồi.");

//            // Kiểm tra xem đã hết lượt chưa (Kiểm tra sơ bộ)
//            if (voucher.UsedCount >= voucher.Quantity) throw new Exception("Voucher đã hết lượt sử dụng.");

//            // 2. TÍNH TOÁN SỐ TIỀN ĐƯỢC GIẢM
//            decimal discountAmount = 0;
//            if (voucher.DiscountType == "FIXED")
//            {
//                discountAmount = voucher.Value;
//            }
//            else // PERCENT
//            {
//                discountAmount = orderTotal * (voucher.Value / 100);
//                if (voucher.MaxDiscountAmount.HasValue && discountAmount > voucher.MaxDiscountAmount.Value)
//                {
//                    discountAmount = voucher.MaxDiscountAmount.Value;
//                }
//            }

//            // 3. THỰC HIỆN ATOMIC UPDATE (Quan trọng nhất)
//            // Logic: "Tìm voucher đúng Mã đó, MÀ số lượng dùng < tổng số, MÀ user chưa dùng"
//            // Sau đó: "Tăng số lượng dùng lên 1 VÀ thêm user vào danh sách"
//            // Tất cả diễn ra trong 1 tích tắc.

//            var filter = Builders<Voucher>.Filter.And(
//                Builders<Voucher>.Filter.Eq(x => x.Id, voucher.Id),
//                Builders<Voucher>.Filter.Lt(x => x.UsedCount, voucher.Quantity), // Chặn người thứ 11
//                Builders<Voucher>.Filter.Not(Builders<Voucher>.Filter.AnyEq(x => x.UsedByUserIds, userId))// Chặn spam double click
//            );

//            var update = Builders<Voucher>.Update
//                .Inc(x => x.UsedCount, 1)        // Tăng UsedCount + 1
//                .Push(x => x.UsedByUserIds, userId); // Thêm userId vào mảng

//            // Chạy lệnh Update
//            var result = await _voucherCollection.UpdateOneAsync(filter, update);

//            // Nếu update thành công (ModifiedCount > 0) -> Nghĩa là giành được vé
//            if (result.ModifiedCount > 0)
//            {
//                return discountAmount; // Trả về số tiền giảm
//            }
//            else
//            {
//                // Nếu thất bại -> Có thể do vừa hết vé xong, hoặc user vừa dùng xong ở tab khác
//                // Check lại để báo lỗi chính xác
//                var checkAgain = await _context.Vouchers.FindAsync(voucher.Id);
//                if (checkAgain.UsedCount >= checkAgain.Quantity)
//                    throw new Exception("Rất tiếc, voucher vừa hết lượt sử dụng.");
//                if (checkAgain.UsedByUserIds.Contains(userId))
//                    throw new Exception("Bạn đã sử dụng voucher này rồi.");

//                throw new Exception("Áp dụng voucher thất bại. Vui lòng thử lại.");
//            }
//        }
//    }
//}


//using Microsoft.EntityFrameworkCore;
//using MongoDB.Bson;
//using MongoDB.Driver;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using TienLuxury.Models;
//using TienLuxury.Data;
//using TienLuxury.ViewModels;

//namespace TienLuxury.Services
//{
//    public class VoucherService : IVoucherService
//    {
//        private readonly DBContext _context;
//        private readonly IMongoCollection<Voucher> _voucherCollection;

//        public VoucherService(DBContext context, IMongoDatabase database)
//        {
//            _context = context;
//            _voucherCollection = database.GetCollection<Voucher>("voucher");
//        }

//        // --- CRUD ---
//        public async Task<List<Voucher>> GetAllVouchers()
//        {
//            return await _context.Vouchers.OrderByDescending(v => v.Id).ToListAsync();
//        }

//        public async Task<Voucher> GetVoucherById(string id)
//        {
//            if (ObjectId.TryParse(id, out ObjectId obj))
//            {
//                return await _context.Vouchers.FindAsync(obj);
//            }
//            return null;
//        }

//        public async Task CreateVoucher(Voucher voucher)
//        {
//            _context.Vouchers.Add(voucher);
//            await _context.SaveChangesAsync();
//        }

//        public async Task UpdateVoucher(Voucher voucher)
//        {
//            _context.Vouchers.Update(voucher);
//            await _context.SaveChangesAsync();
//        }

//        public async Task DeleteVoucher(string id)
//        {
//            if (ObjectId.TryParse(id, out ObjectId obj))
//            {
//                var existing = await _context.Vouchers.FindAsync(obj);
//                if (existing != null)
//                {
//                    _context.Vouchers.Remove(existing);
//                    await _context.SaveChangesAsync();
//                }
//            }
//        }

//        public async Task<Voucher> GetVoucherByCodeAsync(string code)
//        {
//            return await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == code);
//        }

//        public async Task<List<VoucherViewModel>> GetVouchersForUserAsync(string userId)
//        {
//            var now = DateTime.Now;

//            var vouchers = await _context.Vouchers
//                .Where(v => v.IsActive
//                            && v.StartDate <= now
//                            && v.EndDate >= now
//                            && v.UsedCount < v.Quantity)
//                .OrderBy(v => v.EndDate)
//                .ToListAsync();

//            return vouchers.Select(v => new VoucherViewModel
//            {
//                Code = v.Code,
//                Description = v.Description,
//                DiscountType = v.DiscountType,
//                Value = v.Value,
//                MinOrderAmount = v.MinOrderAmount,
//                MaxDiscountAmount = v.MaxDiscountAmount,
//                EndDate = v.EndDate,
//                IsUsed = v.UsedByUserIds != null && v.UsedByUserIds.Contains(userId)
//            }).ToList();
//        }

//        // --- Áp dụng voucher (atomic) ---
//        public async Task<decimal> ApplyVoucherAsync(string code, string userId, decimal orderTotal)
//        {
//            // Lấy voucher để check điều kiện cơ bản
//            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == code);

//            if (voucher == null)
//                throw new Exception("Voucher không tồn tại.");

//            if (!voucher.IsActive)
//                throw new Exception("Voucher đang bị khóa.");

//            if (DateTime.Now < voucher.StartDate)
//                throw new Exception("Voucher chưa đến ngày sử dụng.");

//            if (DateTime.Now > voucher.EndDate)
//                throw new Exception("Voucher đã hết hạn.");

//            if (orderTotal < voucher.MinOrderAmount)
//                throw new Exception($"Đơn hàng cần tối thiểu {voucher.MinOrderAmount:N0}đ để sử dụng voucher.");

//            if (voucher.UsedByUserIds != null && voucher.UsedByUserIds.Contains(userId))
//                throw new Exception("Bạn đã sử dụng voucher này.");

//            if (voucher.UsedCount >= voucher.Quantity)
//                throw new Exception("Voucher đã hết lượt.");

//            // Tính discount
//            decimal discount = 0;
//            if (voucher.DiscountType == "FIXED")
//            {
//                discount = voucher.Value;
//            }
//            else
//            {
//                discount = orderTotal * (voucher.Value / 100m);
//                if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
//                    discount = voucher.MaxDiscountAmount.Value;
//            }

//            // Atomic update trên MongoDB:
//            // filter bao gồm Id, UsedCount < Quantity, userId chưa có trong UsedByUserIds, voucher active/time check
//            var filter = Builders<Voucher>.Filter.And(
//                Builders<Voucher>.Filter.Eq(v => v.Id, voucher.Id),
//                Builders<Voucher>.Filter.Lt(v => v.UsedCount, voucher.Quantity),
//                Builders<Voucher>.Filter.Eq(v => v.IsActive, true),
//                Builders<Voucher>.Filter.Lte(v => v.StartDate, DateTime.Now),
//                Builders<Voucher>.Filter.Gte(v => v.EndDate, DateTime.Now),
//                Builders<Voucher>.Filter.Not(Builders<Voucher>.Filter.AnyEq(v => v.UsedByUserIds, userId))
//            );

//            var update = Builders<Voucher>.Update
//                .Inc(v => v.UsedCount, 1)
//                .Push(v => v.UsedByUserIds, userId);

//            var result = await _voucherCollection.UpdateOneAsync(filter, update);

//            if (result.ModifiedCount > 0)
//            {
//                return discount;
//            }

//            // Nếu update không thành công, kiểm tra lại nguyên nhân để trả message chính xác
//            var latest = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == voucher.Id);
//            if (latest == null)
//                throw new Exception("Voucher không tồn tại (sau khi kiểm tra lại).");
//            if (latest.UsedByUserIds != null && latest.UsedByUserIds.Contains(userId))
//                throw new Exception("Bạn đã sử dụng voucher này.");
//            if (latest.UsedCount >= latest.Quantity)
//                throw new Exception("Rất tiếc, voucher vừa hết lượt sử dụng.");
//            if (!latest.IsActive)
//                throw new Exception("Voucher hiện không hoạt động.");
//            if (DateTime.Now < latest.StartDate || DateTime.Now > latest.EndDate)
//                throw new Exception("Voucher không còn hợp lệ về thời gian.");

//            throw new Exception("Áp dụng voucher thất bại. Vui lòng thử lại.");
//        }

//        // --- Rollback reserve (undo) ---
//        public async Task<bool> RollbackVoucherReserveAsync(string voucherCode, string userId)
//        {
//            if (string.IsNullOrEmpty(voucherCode) || string.IsNullOrEmpty(userId))
//                return false;

//            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == voucherCode);
//            if (voucher == null) return false;

//            var filter = Builders<Voucher>.Filter.Eq(v => v.Id, voucher.Id);
//            var update = Builders<Voucher>.Update
//                .Inc(v => v.UsedCount, -1)
//                .Pull(v => v.UsedByUserIds, userId);

//            var res = await _voucherCollection.UpdateOneAsync(filter, update);
//            return res.ModifiedCount > 0;
//        }
//    }
//}


using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TienLuxury.Models;
using TienLuxury.ViewModels;

namespace TienLuxury.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IMongoCollection<Voucher> _voucherCollection;

        public VoucherService(IMongoDatabase database)
        {
            _voucherCollection = database.GetCollection<Voucher>("voucher");
        }

        // ============================
        // CRUD
        // ============================
        public async Task<List<Voucher>> GetAllVouchers()
        {
            return await _voucherCollection
                .Find(Builders<Voucher>.Filter.Empty)
                .SortByDescending(v => v.Id)
                .ToListAsync();
        }

        public async Task<Voucher> GetVoucherById(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objId))
                return null;

            return await _voucherCollection
                .Find(v => v.Id == objId)
                .FirstOrDefaultAsync();
        }

        public async Task CreateVoucher(Voucher voucher)
        {
            await _voucherCollection.InsertOneAsync(voucher);
        }

        public async Task UpdateVoucher(Voucher voucher)
        {
            await _voucherCollection.ReplaceOneAsync(
                v => v.Id == voucher.Id,
                voucher
            );
        }

        public async Task DeleteVoucher(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objId))
                return;

            await _voucherCollection.DeleteOneAsync(v => v.Id == objId);
        }

        // ============================
        // Lấy voucher bằng code (đã fix lỗi trùng code!)
        // ============================
        public async Task<Voucher> GetVoucherByCodeAsync(string code)
        {
            //return await _voucherCollection
            //    .Find(v => v.Code == code)
            //    .FirstOrDefaultAsync();
            return await _voucherCollection
                .Find(v => v.Code == code && v.IsActive == true) // Chỉ lấy cái đang Active cho chắc
                .SortByDescending(v => v.Id) // QUAN TRỌNG: Ưu tiên lấy cái mới nhất
                .FirstOrDefaultAsync();
        }

        // ============================
        // Lấy voucher hợp lệ cho user dùng
        // ============================
        public async Task<List<VoucherViewModel>> GetVouchersForUserAsync(string userId)
        {
            var nowVN = DateTime.Now;
            var nowUtc = nowVN.ToUniversalTime();

            var filter = Builders<Voucher>.Filter.And(
                Builders<Voucher>.Filter.Eq(v => v.IsActive, true),
                Builders<Voucher>.Filter.Lte(v => v.StartDate, nowUtc),
                Builders<Voucher>.Filter.Gte(v => v.EndDate, nowUtc),
                Builders<Voucher>.Filter.Where(v => v.UsedCount < v.Quantity)
            );

            var vouchers = await _voucherCollection.Find(filter).ToListAsync();

            return vouchers.Select(v => new VoucherViewModel
            {
                Code = v.Code,
                Description = v.Description,
                DiscountType = v.DiscountType,
                Value = v.Value,
                MinOrderAmount = v.MinOrderAmount,
                MaxDiscountAmount = v.MaxDiscountAmount,
                EndDate = v.EndDate,
                IsUsed = v.UsedByUserIds != null && v.UsedByUserIds.Contains(userId)
            }).ToList();
        }

        // ============================
        // Áp dụng voucher (Atomic MongoDB)
        // ============================
        public async Task<decimal> ApplyVoucherAsync(string code, string userId, decimal orderTotal)
        {
            var nowVN = DateTime.Now;
            var nowUtc = nowVN.ToUniversalTime();

            var voucher = await GetVoucherByCodeAsync(code);
            if (voucher == null)
                throw new Exception("Voucher không tồn tại.");

            // Check logic theo giờ VN
            if (!voucher.IsActive)
                throw new Exception("Voucher đang bị khóa.");
            if (nowVN < voucher.StartDate)
                throw new Exception("Voucher chưa bắt đầu.");
            if (nowVN > voucher.EndDate)
                throw new Exception("Voucher đã hết hạn.");

            if (orderTotal < voucher.MinOrderAmount)
                throw new Exception($"Đơn hàng tối thiểu {voucher.MinOrderAmount:N0}đ.");

            if (voucher.UsedByUserIds != null && voucher.UsedByUserIds.Contains(userId))
                throw new Exception("Bạn đã sử dụng voucher này.");

            // Tính discount
            decimal discount;
            if (voucher.DiscountType == "FIXED")
            {
                discount = voucher.Value;
            }
            else
            {
                discount = orderTotal * (voucher.Value / 100m);
                if (voucher.MaxDiscountAmount.HasValue)
                    discount = Math.Min(discount, voucher.MaxDiscountAmount.Value);
            }

            // ------ ATOMIC UPDATE (DÙNG UTC CHO MONGODB CẦN) ------
            var filter = Builders<Voucher>.Filter.And(
                Builders<Voucher>.Filter.Eq(v => v.Id, voucher.Id),
                Builders<Voucher>.Filter.Eq(v => v.IsActive, true),
                Builders<Voucher>.Filter.Lte(v => v.StartDate, nowUtc),
                Builders<Voucher>.Filter.Gte(v => v.EndDate, nowUtc),
                Builders<Voucher>.Filter.Lt("UsedCount", voucher.Quantity),
                Builders<Voucher>.Filter.Not(
                    Builders<Voucher>.Filter.AnyEq(v => v.UsedByUserIds, userId)
                )
            );

            var update = Builders<Voucher>.Update
                .Inc(v => v.UsedCount, 1)
                .Push(v => v.UsedByUserIds, userId);

            var result = await _voucherCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
                throw new Exception("Mã giảm giá đã hết lượt sử dụng.");

            return discount;
        }

        // ============================
        // Rollback (xóa sử dụng voucher)
        // ============================
        public async Task<bool> RollbackVoucherReserveAsync(string code, string userId)
        {
            var filter = Builders<Voucher>.Filter.Eq(v => v.Code, code);

            var update = Builders<Voucher>.Update
                .Inc(v => v.UsedCount, -1)
                .Pull(v => v.UsedByUserIds, userId);

            var result = await _voucherCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }
    }
}
