using TienLuxury.Models;

namespace TienLuxury.ViewModels
{
    public class ProductIndexViewModel
    {
        public string SearchTerm { get; set; }
        public int QuantityInCart { get; set; }

        // Danh sách chứa các nhóm: Mỗi nhóm gồm Tên danh mục + List sản phẩm
        public List<CategoryGroupViewModel> CategoryGroups { get; set; } = new List<CategoryGroupViewModel>();
    }

    public class CategoryGroupViewModel
    {
        public string CategoryId { get; set; }   // Để làm neo link (Anchor)
        public string CategoryName { get; set; } // Tên hiển thị (VD: SKIN CARE)
        public List<Product> Products { get; set; } // Danh sách sản phẩm của nhóm này
    }
}