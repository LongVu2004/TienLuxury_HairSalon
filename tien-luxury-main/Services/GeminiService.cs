using Mscc.GenerativeAI; // Chỉ cần duy nhất namespace này cho v2.9.3

namespace TienLuxury.Services
{
    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly string _modelId;

        public GeminiService(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"];
            _modelId = configuration["Gemini:Model"] ?? "gemini-2.5-flash";
        }

        public async Task<string> GetChatResponseAsync(string userMessage, string dataContext)
        {
            var googleAI = new GoogleAI(_apiKey);

            // Ghép dữ liệu thật vào Prompt
            string finalSystemPrompt = $"""
                Bạn là AI Trợ lý của 'TienLuxury Hair Salon'. 
                Nhiệm vụ: Tư vấn dịch vụ và bán sản phẩm dựa trên dữ liệu thật dưới đây.
                Hotline hỗ trợ: 0366.050.144

                {dataContext} 
                
                QUY TẮC TRẢ LỜI:
                1. CHỈ tư vấn những dịch vụ/sản phẩm có trong danh sách trên. Nếu khách hỏi món không có, hãy báo hết hàng hoặc chưa kinh doanh.
                2. Báo giá chính xác từng đồng (ví dụ 100,000 VNĐ).
                3. Ngắn gọn, vui vẻ, dùng icon.
                4. Giờ hoạt động: Thứ 2 đến Chủ nhật, 8 giờ sáng đến 22 giờ tối 
                """;

            // === SỬA LỖI Ở ĐÂY ===
            var model = googleAI.GenerativeModel(
            model: Model.Gemini25Flash,
                systemInstruction: new Content
                {
                    // Thay List<Part> thành List<IPart>
                    Parts = new List<IPart>
                    {
                        new TextData { Text = finalSystemPrompt }
                    }
                }
            );

            try
            {
                var response = await model.GenerateContent(userMessage);
                return response.Text;
            }
            catch (Exception ex)
            {
                return $"Lỗi AI: {ex.Message}";
            }
        }
    }
}