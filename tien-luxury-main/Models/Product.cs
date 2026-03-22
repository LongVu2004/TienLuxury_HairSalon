//using MongoDB.Bson;
//using MongoDB.EntityFrameworkCore;
//using System.ComponentModel.DataAnnotations;

//namespace TienLuxury.Models
//{
//    [Collection("product")]
//    public class Product
//    {
//        private ObjectId id;

//        [Required(ErrorMessage = "Nhập tên sản phẩm")]
//        [Display(Name = "Tên sản phẩm")]
//        private string? productName;

//        [Display(Name = "Giá")]
//        private Decimal price;

//        [Required(ErrorMessage = "Nhập số lượng trong kho")]
//        [Display(Name = "Số lượng trong kho")]
//        private int? quantityInStock;

//        [Display(Name = "Mô tả")]
//        private string? description;

//        private string imagePath = string.Empty;
//        private bool isSold;
//        private string? productType;
//        private ObjectId? categoryId;

//        public ObjectId ID { get => id; set => id = value; }
//        public string? ProductName { get => productName; set => productName = value; }
//        public Decimal Price { get => price; set => price = value; }
//        public int? QuantityInStock { get => quantityInStock; set => quantityInStock = value; }
//        public string? Description { get => description; set => description = value; }
//        public string ImagePath { get => imagePath; set => imagePath = value; }
//        public Boolean IsSold { get => isSold; set => isSold = value; }
//        public string ProductType { get => productType; set => productType = value; }
//        public ObjectId? CategoryId { get => categoryId; set => categoryId = value; }
//    }
//}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore; // Hoặc namespace tương ứng của bạn
using System.ComponentModel.DataAnnotations;

namespace TienLuxury.Models
{
    [Collection("product")]
    public class Product
    {
        [BsonId] // Bắt buộc để map với _id của MongoDB
        public ObjectId ID { get; set; }

        [Required(ErrorMessage = "Nhập tên sản phẩm")]
        [Display(Name = "Tên sản phẩm")]
        [BsonElement("ProductName")] // Map chính xác tên field trong DB
        public string? ProductName { get; set; }

        [Display(Name = "Giá")]
        [BsonElement("Price")]
        public Decimal Price { get; set; }

        [Required(ErrorMessage = "Nhập số lượng trong kho")]
        [Display(Name = "Số lượng trong kho")]
        [BsonElement("QuantityInStock")]
        public int? QuantityInStock { get; set; }

        [Display(Name = "Mô tả")]
        [BsonElement("Description")]
        public string? Description { get; set; }

        [BsonElement("ImagePath")]
        public string ImagePath { get; set; } = string.Empty;

        [BsonElement("IsSold")]
        public Boolean IsSold { get; set; }

        // Field cũ (Giữ lại để migrate dữ liệu, sau này có thể xóa)
        [BsonElement("ProductType")]
        public string? ProductType { get; set; }

        // Field mới (Dùng để liên kết khóa ngoại)
        [Display(Name = "Danh mục")]
        [BsonElement("CategoryId")]
        public ObjectId? CategoryId { get; set; }
    }
}