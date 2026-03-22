using TienLuxury.Models;
using MongoDB.Bson;

namespace TienLuxury.Services
{
    public interface IInvoiceDetailsService
    {
        public Task CreateInvoiceDetail(ObjectId invoiceId, ObjectId productId, int quantity);
        public Task CreateInvoiceDetail(InvoiceDetail detail);
        public Task DeleteInvoiceDetail(ObjectId invoiceId);
        public Task<List<InvoiceDetail>> GetDetailsByInvoiceId(ObjectId invoiceId);
    }
}