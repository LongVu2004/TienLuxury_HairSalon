using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TienLuxury.Models;
using MongoDB.Bson;

namespace TienLuxury.Services
{
    public class InvoiceDetailsService(DBContext dBContext) : IInvoiceDetailsService
    {
        private readonly DBContext _dbContext = dBContext;

        public async Task CreateInvoiceDetail(ObjectId invoiceId, ObjectId productId, int quantity)
        {
            var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(i => i.ID == invoiceId);
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.ID == productId);

            if (invoice == null || product == null)
            {
                throw new System.Exception("Invoice or Product not found");
            }

            if (product.QuantityInStock < quantity)
            {
                throw new System.Exception("Not enough product in stock");
            }

            var detail = new InvoiceDetail()
            {
                InvoiceId = invoiceId,
                ProductId = productId,
                Quantity = quantity,
            };

            _dbContext.InvoiceDetails.Add(detail);
            await _dbContext.SaveChangesAsync();
        }

        public async Task CreateInvoiceDetail(InvoiceDetail detail)
        {
            _dbContext.InvoiceDetails.Add(detail);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteInvoiceDetail(ObjectId invoiceId)
        {
            var detail = await _dbContext.InvoiceDetails.FirstOrDefaultAsync(d => d.InvoiceId == invoiceId);
            if (detail != null)
            {
                _dbContext.InvoiceDetails.Remove(detail);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<InvoiceDetail>> GetDetailsByInvoiceId(ObjectId invoiceId)
            => await _dbContext.InvoiceDetails.Where(d => d.InvoiceId == invoiceId).ToListAsync();
    }
}