using TienLuxury.Models;
using Microsoft.AspNetCore.Http;

namespace TienLuxury.ViewModels
{
    public class UserProfileViewModel
    {
        // Thông tin cá nhân
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? CurrentAvatar { get; set; } // Để hiển thị ảnh cũ
        public IFormFile? AvatarUpload { get; set; } // Để nhận file upload mới

        // Dữ liệu lịch sử 
        public List<Invoice> OrderHistory { get; set; } = new List<Invoice>();
        public List<Reservation> BookingHistory { get; set; } = new List<Reservation>();
    }
}