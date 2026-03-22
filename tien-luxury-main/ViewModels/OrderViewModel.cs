namespace TienLuxury.ViewModels
{
    public class OrderViewModel
    {
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public string? AppliedVoucherCode { get; set; }
        public List<CartItemViewModel>? Items { get; set; }
    }
}