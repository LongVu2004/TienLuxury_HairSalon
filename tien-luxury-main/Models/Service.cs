using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace TienLuxury.Models
{
    [Collection("service")]
    public class Service
    {
        private ObjectId id;

        [Required(ErrorMessage = "Nhập tên dịch vụ")]
        [Display(Name = "Tên dịch vụ")]
        private string serviceName;

        [Display(Name = "Giá")]
        private Decimal price;

        [Display(Name = "Mô tả")]
        private string? description;

        [Display(Name = "Trạng thái dịch vụ")]
        private bool isActivated;
        private string? serviceType;
        private string imagePath = string.Empty;

        public ObjectId ID { get => id; set => id = value; }
        public string ServiceName { get => serviceName; set => serviceName = value; }
        public Decimal Price { get => price; set => price = value; }
        public string? Description { get => description; set => description = value; }
        public bool IsActivated { get => isActivated; set => isActivated = value; }
        public string ImagePath { get => imagePath; set => imagePath = value; }
        public string ServiceType { get => serviceType; set => serviceType = value; }
    }
}