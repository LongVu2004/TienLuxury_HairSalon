using Microsoft.AspNetCore.Mvc.Rendering;
using TienLuxury.Models;

namespace TienLuxury.Areas.Admin.ViewModels
{
    public class ProductAddEditViewModel
    {
        public Product? Product { get; set; }
        public List<SelectListItem>? CategoryList { get; set; }
    }
}
