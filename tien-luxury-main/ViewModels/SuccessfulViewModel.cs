using TienLuxury.Models;

namespace TienLuxury.ViewModels
{
    public class SuccessfulViewModel
    {
        public Invoice Invoice { get; set; }
        public List<CartItemViewModel> Items { get; set; }
    }
}