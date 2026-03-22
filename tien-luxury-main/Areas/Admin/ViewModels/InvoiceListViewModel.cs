using TienLuxury.Models;

namespace TienLuxury.Areas.Admin.ViewModels
{
    public class InvoiceListViewModel
    {
        public IEnumerable<Invoice> Invoices { get; set; }
    }
}