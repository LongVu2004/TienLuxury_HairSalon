using Microsoft.EntityFrameworkCore;
using System.Text;
using TienLuxury.Models;

namespace TienLuxury.Services
{
    public class ChatDataService
    {
        private readonly DBContext _context;

        public ChatDataService(DBContext context)
        {
            _context = context;
        }

        public async Task<string> GetSystemPromptWithDataAsync()
        {
            StringBuilder sb = new StringBuilder();

            // 1. LẤY DỊCH VỤ (Chỉ lấy cái nào IsActivated = true)
            var services = await _context.Set<Service>()
                                         .Where(s => s.IsActivated)
                                         .ToListAsync();

            sb.AppendLine("=== DANH SÁCH DỊCH VỤ TẠI SALON ===");
            if (services.Any())
            {
                foreach (var s in services)
                {
                    sb.AppendLine($"- {s.ServiceName}: {s.Price:N0} VNĐ. ({s.Description ?? "Dịch vụ tiêu chuẩn"})");
                }
            }
            else
            {
                sb.AppendLine("(Hiện chưa có dịch vụ nào)");
            }
            sb.AppendLine(); 

            // 2. LẤY SẢN PHẨM (Chỉ lấy cái còn hàng: QuantityInStock > 0)
            var products = await _context.Set<Product>()
                                         .Where(p => p.QuantityInStock > 0)
                                         .ToListAsync();

            sb.AppendLine("=== SẢN PHẨM SÁP/MỸ PHẨM ĐANG BÁN ===");
            if (products.Any())
            {
                foreach (var p in products)
                {
                    sb.AppendLine($"- {p.ProductName}: {p.Price:N0} VNĐ. Loại: {p.ProductType}. (Còn {p.QuantityInStock} cái)");
                }
            }
            else
            {
                sb.AppendLine("(Hiện hết hàng sản phẩm)");
            }

            return sb.ToString();
        }
    }
}
