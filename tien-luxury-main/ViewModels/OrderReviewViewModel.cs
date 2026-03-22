namespace TienLuxury.ViewModels
{
    public class OrderReviewViewModel
    {
        public string InvoiceId { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ProductReviewItem> Items { get; set; } = new List<ProductReviewItem>();
    }

    public class ProductReviewItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }

        // Dữ liệu để binding form
        public int Rating { get; set; } = 5; // Mặc định 5 sao
        public string Comment { get; set; }
        public bool IsReviewed { get; set; } = false; // Đã đánh giá chưa
        public bool IsApproved { get; set; } 
    }
}
