using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TienLuxury.Models
{
    [Collection("invoice-details")]
    public class InvoiceDetail
    {
        private ObjectId invoiceId;
        private ObjectId productId;

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Required(ErrorMessage = "Nhập số lượng")]
        [Display(Name = "Số lượng")]
        private int quantity;

        public ObjectId InvoiceId { get => invoiceId; set => invoiceId = value; }
        public ObjectId ProductId { get => productId; set => productId = value; }
        public int Quantity { get => quantity; set => quantity = value; }
    }
}