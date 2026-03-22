using Microsoft.AspNetCore.Mvc;
using TienLuxury.Models;
using TienLuxury.ViewModels;
using TienLuxury.Services;
using MongoDB.Bson;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TienLuxury.Controllers
{
    public class ContactController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly DBContext _context;

        public ContactController(IMessageService messageService, DBContext context)
        {
            _messageService = messageService;
            _context = context;
        }

        // GET: Hiển thị trang liên hệ (Ai cũng xem được)
        public async Task<IActionResult> Index()
        {
            var model = new MessageViewModel
            {
                Message = new Message()
            };

            if (User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userIdString) && ObjectId.TryParse(userIdString, out ObjectId objectId))
                {
                    var user = await _context.Set<AppUser>().FindAsync(objectId);
                    if (user != null)
                    {
                        model.Message.CustomerName = user.FullName;
                        model.Message.Email = user.Email;
                        model.Message.PhoneNumber = user.PhoneNumber;
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Successful()
        {
            return View();
        }

        // POST: Gửi tin nhắn
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage(MessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Message.CreatedAt = DateTime.Now;

                await _messageService.CreateMessage(model.Message);

                TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi đánh giá!";
                return RedirectToAction("Successful");
            }

            return View("Index", model);
        }
    }
}