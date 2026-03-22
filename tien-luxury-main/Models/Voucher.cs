using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TienLuxury.Models
{
    [Collection("voucher")]
    public class Voucher
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [Required(ErrorMessage = "Mã voucher không được để trống")]
        [StringLength(20, ErrorMessage = "Mã tối đa 20 ký tự")]
        [Display(Name = "Mã Voucher")]
        public string Code { get; set; } // Ví dụ: TET2025 (Viết hoa, không dấu)

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Loại giảm giá")]
        public string DiscountType { get; set; } // Giá trị: "PERCENT" (Phần trăm) hoặc "FIXED" (Tiền mặt)

        [Required]
        [Display(Name = "Giá trị giảm")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải lớn hơn 0")]
        public decimal Value { get; set; } // VD: 10 (là 10%) hoặc 50000 (là 50k)

        [Display(Name = "Giảm tối đa")]
        public decimal? MaxDiscountAmount { get; set; } // Chỉ dùng cho loại PERCENT (VD: Giảm 50% nhưng tối đa 50k)

        [Display(Name = "Đơn tối thiểu")]
        public decimal MinOrderAmount { get; set; } = 0; // Đơn hàng > số này mới được dùng

        [Required]
        [Display(Name = "Tổng số lượng")]
        public int Quantity { get; set; } // Tổng số vé phát ra

        [Display(Name = "Đã sử dụng")]
        public int UsedCount { get; set; } = 0; // Số vé đã bị xài

        [Display(Name = "Ngày bắt đầu")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày kết thúc")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Lưu danh sách User ID những người đã dùng để chặn spam (1 người dùng 2 lần)
        public List<string> UsedByUserIds { get; set; } = new List<string>();
        public bool IsValid()
        {
            return IsActive &&
                   DateTime.Now >= StartDate &&
                   DateTime.Now <= EndDate &&
                   UsedCount < Quantity;
        }
    }
}