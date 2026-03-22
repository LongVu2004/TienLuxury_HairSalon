using Microsoft.AspNetCore.Mvc;
using TienLuxury.Areas.Admin.ViewModels;
using TienLuxury.Areas.Filter;
using TienLuxury.Models;
using TienLuxury.Services;
using TienLuxury.ViewModels;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authorization;

namespace TienLuxury.Areas.Admin.Controllers
{
    //[AdminAuth]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [DesktopOnly]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class OrdersManagementController(IInvoiceService invoiceService, IInvoiceDetailsService invoiceDetailsService, IProductService productService) : Controller
    {
        private readonly IInvoiceService _invoiceService = invoiceService;
        private readonly IInvoiceDetailsService _invoiceDetailService = invoiceDetailsService;
        private readonly IProductService _productService = productService;
        public async Task<IActionResult> Index()
        {
            InvoiceListViewModel model = new()
            {
                Invoices = await _invoiceService.GetAll()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateInvoiceStatus(ObjectId invoiceID)
        {
            Invoice invoice = await _invoiceService.GetInvoiceById(invoiceID);
            if (invoice == null)
            {
                return NotFound("Không tìm thấy hóa đơn nào.");
            }

            var invoiceDetailsTask = _invoiceDetailService.GetDetailsByInvoiceId(invoiceID);
            var allProductsTask = _productService.GetAllProduct();

            await Task.WhenAll(invoiceDetailsTask, allProductsTask);

            invoice.InvoiceDetails = invoiceDetailsTask.Result;
            var allProducts = allProductsTask.Result;

            var productDetailsMap = allProducts.ToDictionary(p => p.ID, p => p);

            var cartItems = invoice.InvoiceDetails
                .Where(d => productDetailsMap.ContainsKey(d.ProductId))
                .Select(d =>
                {
                    var product = productDetailsMap[d.ProductId];
                    return new CartItemViewModel
                    {
                        ProductID = product.ID.ToString(),
                        ProductName = product.ProductName,
                        ProductPrice = (int)product.Price,
                        ImagePath = product.ImagePath,
                        Quantity = d.Quantity
                    };
                }).ToList();

            var model = new InvoiceUpdateVIewModel
            {
                Invoice = invoice,
                Items = cartItems
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInvoiceStatus(InvoiceUpdateVIewModel model)
        {
            var invoice = await _invoiceService.GetInvoiceById(model.Invoice.ID);
            if (invoice == null)
            {
                return NotFound("Không tìm thấy hóa đơn nào.");
            }

            invoice.Status = model.Invoice.Status;
            await _invoiceService.UpdateStatusInvoice(invoice);

            TempData["SuccessMessage"] = "Cập nhật trạng thái hóa đơn thành công.";
            return RedirectToAction("Index");

        }

        [HttpGet]
        public IActionResult DeleteConfirmation()
            => View();

        [HttpPost]
        public async Task<IActionResult> DeleteInvoice(ObjectId id)
        {
            var invoice = await _invoiceService.GetInvoiceById(id);
            if (invoice == null)
            {
                return NotFound("Không tìm thấy hóa đơn nào ");
            }

            var invoiceDetails = await _invoiceDetailService.GetDetailsByInvoiceId(id);
            foreach (var detail in invoiceDetails)
            {
                if (invoice.Status != "Đã xử lý")
                {
                    await _productService.MinusQuantityInStock(detail.ProductId, -detail.Quantity);
                }

                await _invoiceDetailService.DeleteInvoiceDetail(invoice.ID);
            }

            await _invoiceService.DeleteInvoice(invoice.ID);

            TempData["SuccessMessage"] = "Xóa hóa đơn thành công.";
            return RedirectToAction("Index");
        }

    }
}
