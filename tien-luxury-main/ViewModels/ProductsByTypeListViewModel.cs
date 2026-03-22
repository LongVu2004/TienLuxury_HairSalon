using TienLuxury.Models;

namespace TienLuxury.ViewModels
{
    public class ProductByTypeListViewModel
    {
        public List<Product> Hair { get; set; }
        public List<Product> SkinCare { get; set; }
        public List<Product> Others { get; set; }

        public int QuantityInCart { get; set; } = 0;
        public string? SearchTerm { get; set; }
    }

}