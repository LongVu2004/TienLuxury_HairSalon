using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using TienLuxury.Models;

namespace TienLuxury.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    //[AdminAuth] // Sử dụng Attribute bảo mật bạn đã gửi
    public class AccountsManagementController : Controller
    {
        private readonly DBContext _context;

        public AccountsManagementController(DBContext context)
        {
            _context = context;
        }

        // GET: Admin/AccountsManagement
        public async Task<IActionResult> Index(string searchString)
        {
            // Lấy danh sách user
            var usersQuery = _context.AppUsers.AsQueryable();

            // Xử lý tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                // Tìm theo Email hoặc Tên hoặc SĐT
                usersQuery = usersQuery.Where(u =>
                    u.Email.Contains(searchString) ||
                    u.FullName.Contains(searchString) ||
                    u.PhoneNumber.Contains(searchString));
            }

            // Sắp xếp: Mới nhất lên đầu
            var users = await usersQuery.OrderByDescending(u => u.CreatedAt).ToListAsync();

            // Giữ lại từ khóa tìm kiếm ở View
            ViewBag.CurrentFilter = searchString;

            return View(users);
        }

        // GET: Admin/AccountsManagement/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            // Parse string sang ObjectId vì MongoDB dùng ObjectId
            if (!ObjectId.TryParse(id, out ObjectId objectId)) return NotFound();

            var user = await _context.AppUsers.FirstOrDefaultAsync(m => m.Id == objectId);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Admin/AccountsManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId)) return NotFound();

            var user = await _context.AppUsers.FindAsync(objectId);

            if (user != null)
            {
                // Kiểm tra: Không cho xóa tài khoản đang là Admin (để tránh xóa nhầm chính mình)
                if (user.Role == "Admin")
                {
                    TempData["ErrorMessage"] = "Không thể xóa tài khoản Quản trị viên!";
                    return RedirectToAction(nameof(Index));
                }

                _context.AppUsers.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa tài khoản thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
        // POST: Admin/AccountsManagement/UpdateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string id, string newRole)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId)) return NotFound();

            var user = await _context.AppUsers.FindAsync(objectId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản!";
                return RedirectToAction(nameof(Index));
            }

            user.Role = newRole;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật quyền cho '{user.FullName}' thành: {newRole}";

            return RedirectToAction(nameof(Index));
        }
    }
}