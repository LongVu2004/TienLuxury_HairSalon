using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TienLuxury.Models
{
    [Collection("user")]
    public class AppUser
    {
        public ObjectId Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        public string PasswordHash { get; set; } // Lưu mật khẩu đã mã hóa

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }
        public string? Avatar { get; set; }

        public string Role { get; set; } = "Customer"; // Mặc định là Khách hàng (Admin/Customer)

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
