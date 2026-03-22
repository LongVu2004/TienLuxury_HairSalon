namespace TienLuxury.ViewModels
{
    public class VoucherViewModel
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; } // "FIXED" hoặc "PERCENT"
        public decimal Value { get; set; } // Giá trị giảm
        public decimal MinOrderAmount { get; set; } // Đơn tối thiểu
        public decimal? MaxDiscountAmount { get; set; } // Giảm tối đa (nếu có)
        public DateTime EndDate { get; set; } // Hạn sử dụng

        // TRỌNG TÂM: Cờ đánh dấu user đã dùng chưa
        public bool IsUsed { get; set; }
    }
}
