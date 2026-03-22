using TienLuxury.Models;

namespace TienLuxury.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product? Product { get; set; }
        public List<Product>? Products { get; set; }

        public CartItemViewModel? CartItemViewModel { get; set; }
        public List<ProductReview> Reviews { get; set; } = new List<ProductReview>();
        public bool CanReview { get; set; } = false;
        public double AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
    }
}