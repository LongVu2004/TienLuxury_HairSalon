using TienLuxury.Services;
using Microsoft.AspNetCore.Mvc;

namespace TienLuxury.Controllers
{
    public class ChatController : Controller
    {
        // Đổi tên biến và kiểu dữ liệu
        private readonly GeminiService _geminiService;
        private readonly ChatDataService _chatDataService;

        // Inject GeminiService vào
        public ChatController(GeminiService geminiService, ChatDataService chatDataService)
        {
            _geminiService = geminiService;
            _chatDataService = chatDataService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message)) return BadRequest();

            try
            {
                // 1. Lấy toàn bộ dữ liệu SP/Dịch vụ hiện có (đã format thành text)
                string shopData = await _chatDataService.GetSystemPromptWithDataAsync();

                // 2. Gửi cho AI trả lời
                var reply = await _geminiService.GetChatResponseAsync(request.Message, shopData);

                return Ok(new { reply });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { reply = "Hệ thống đang bận, vui lòng gọi hotline!" });
            }
        }
    }
    public class ChatRequest { public string Message { get; set; } }
}