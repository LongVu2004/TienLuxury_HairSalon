using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TienLuxury.Models
{
    [Collection("category")] // Tên bảng trong MongoDB
    public class Category
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [Display(Name = "Tên danh mục")]
        [BsonElement("CategoryName")]
        public string CategoryName { get; set; }

        [Display(Name = "Mô tả danh mục")]
        [BsonElement("Description")]
        public string? Description { get; set; }
    }
}