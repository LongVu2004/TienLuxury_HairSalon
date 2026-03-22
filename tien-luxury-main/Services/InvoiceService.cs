using Microsoft.EntityFrameworkCore;
using TienLuxury.Models;
using MongoDB.Bson;

namespace TienLuxury.Services
{
    public class InvoiceService(DBContext dbContext) : IInvoiceService
    {
        private readonly DBContext _dbContext = dbContext;

        public async Task<List<Invoice>> GetAll()
            => await _dbContext.Invoices.OrderBy(i => i.CreatedDate).ToListAsync();
        
        public async Task<ObjectId> CreateInvoice(Invoice newInvoice)
        {
            await _dbContext.Invoices.AddAsync(newInvoice);
            await _dbContext.SaveChangesAsync();

            _dbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_dbContext.ChangeTracker.DebugView.LongView);
    
            return newInvoice.ID;

        }

        public async Task UpdateStatusInvoice(Invoice invoiceToUpdate)
        {
            Invoice? invoiceUpdated = await _dbContext.Invoices.FirstOrDefaultAsync(i => i.ID == invoiceToUpdate.ID) ?? throw new InvalidOperationException("Invoice not found");

            invoiceUpdated.Status = invoiceToUpdate.Status;

            _dbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_dbContext.ChangeTracker.DebugView.LongView);

            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateTotal(Invoice invoice, decimal Total)
        {
            Invoice? invoiceUpdated = await _dbContext.Invoices.FirstOrDefaultAsync(i => i.ID == invoice.ID) ?? throw new InvalidOperationException("Invoice not found");

            invoiceUpdated.Total = Total;
            _dbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_dbContext.ChangeTracker.DebugView.LongView);

            await _dbContext.SaveChangesAsync();
        }
        public async Task<IEnumerable<Invoice>> GetAllInvoices()
            => await _dbContext.Invoices.OrderBy(i => i.CreatedDate).ToListAsync();

        public async Task<Invoice> GetInvoiceById(ObjectId id)
            => await _dbContext.Invoices.FirstOrDefaultAsync(i => i.ID == id) ?? throw new InvalidOperationException("Invoice not found");

        public async Task<IEnumerable<Invoice>> GetInvoicesByPhoneNumber(string phoneNumber)
            => await _dbContext.Invoices.Where(i => i.PhoneNumber == phoneNumber).ToListAsync();

        public async Task DeleteInvoice(ObjectId id)
        {
            var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(i => i.ID == id) ?? throw new InvalidOperationException("Invoice not found");

            if (invoice != null)
            {
                _dbContext.Invoices.Remove(invoice);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("Invoice not found");
            }
        }
    }
}