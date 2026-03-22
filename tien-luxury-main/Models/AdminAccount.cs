using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TienLuxury.Models
{
    [Collection("admin-account")]
    public class AdminAccount
    {
        private ObjectId id;

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [Display(Name = "Tên đăng nhập")]
        private string? username;


        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [Display(Name = "Mật khẩu")]
        [StringLength(100, MinimumLength = 9, ErrorMessage = "Độ dài từ 5 - 100 ký tự.")]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Chỉ được nhập chữ và số.")]
        private string? password;

        public ObjectId ID { get => id; set => id = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
    }
}