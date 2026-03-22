using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;

namespace TienLuxury.Models
{

    [Collection("invoice")]
    public class Invoice
    {
        private ObjectId id;

        [Required(ErrorMessage = "Nhập tên khách hàng")]
        [Display(Name = "Tên khách hàng")]
        private string? customerName;

        [Required(ErrorMessage = "Số đện thoại")]
        [Display(Name = "Số điện thoại")]
        private string phoneNumber;

        [Display(Name = "Email")]
        private string? email = "";

        [Required(ErrorMessage = "Địa chỉ")]
        [Display(Name = "Địa chỉ")]
        private string? address;

        [Display(Name = "Ngày tạo")]
        private DateTime createdDate;

        [Display(Name = "Tổng tiền")]
        private Decimal total;

        [Display(Name = "Phương thức thanh toán")]
        private string paymentMethod = "COD";

        [Display(Name = "Trạng thái")]
        private string status = "Đang xử lý";

        public string? VoucherCode { get; set; } // Lưu mã đã dùng
        public decimal? DiscountAmount { get; set; } = 0; // Lưu số tiền đã giảm

        private List<InvoiceDetail> invoiceDetails;

        public ObjectId ID { get => id; set => id = value; }
        public string? CustomerName { get => customerName; set => customerName = value; }
        public string? Address { get => address; set => address = value; }
        public string PhoneNumber { get => phoneNumber; set => phoneNumber = value; }
        public string? Email { get => email; set => email = value; }
        public DateTime CreatedDate { get => createdDate; set => createdDate = value; }
        public decimal Total { get => total; set => total = value; }
        public string PaymentMethod { get => paymentMethod; set => paymentMethod = value; }
        public string Status { get => status; set => status = value; }
        public List<InvoiceDetail> InvoiceDetails { get => invoiceDetails; set => invoiceDetails = value; }
    }
}