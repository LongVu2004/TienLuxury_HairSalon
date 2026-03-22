using MongoDB.Bson;
using System;

namespace TienLuxury.Areas.Admin.ViewModels
{
    public class ReviewListViewModel
    {
        public string Id { get; set; } // ID bài đánh giá
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsApproved { get; set; }
    }
}