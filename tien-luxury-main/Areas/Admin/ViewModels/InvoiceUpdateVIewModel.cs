using TienLuxury.Models;
using TienLuxury.ViewModels;

namespace TienLuxury.Areas.Admin.ViewModels
{
    public class InvoiceUpdateVIewModel
    {
        public Invoice Invoice { get; set; }
        public List<CartItemViewModel>? Items { get; set; }
    }
}