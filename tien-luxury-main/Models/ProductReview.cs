using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TienLuxury.Models
{
    public class ProductReview
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public ObjectId ProductId { get; set; } // Đánh giá cho sản phẩm nào
        public ObjectId UserId { get; set; }    // Ai đánh giá
        public string UserName { get; set; }    // Tên người đánh giá (Lưu cứng để đỡ phải join bảng)
        public string Avatar { get; set; }      // Avatar người đánh giá

        public int Rating { get; set; }         // Số sao (1-5)
        public string Comment { get; set; }     // Nội dung bình luận
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ObjectId OrderId { get; set; }   // Đánh giá dựa trên đơn hàng nào (để xác thực đã mua)
        public bool? IsApproved { get; set; } = false; // false: Chờ duyệt (mặc định), true: Đã duyệt
    }
}