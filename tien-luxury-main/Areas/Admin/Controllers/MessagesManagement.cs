using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TienLuxury.Areas.Admin.ViewModels;
using TienLuxury.Areas.Filter;
using TienLuxury.Services;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authorization;

namespace TienLuxury.Areas.Admin.Controllers
{
    //[AdminAuth]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [DesktopOnly]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class MessagesManagementController(IMessageService messagesService) : Controller
    {
        private readonly IMessageService _messagesService = messagesService;

        public async Task<IActionResult> Index()
        {
            MessageListViewModel model = new MessageListViewModel()
            {
                Messages = await _messagesService.GetAllMessage()
            };
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(ObjectId id)
        {
            var message = await _messagesService.GetMessageById(id);
            if (message == null)
            {
                return Json(new { success = false });
            }

            await _messagesService.DeleteMessage(message);

            var redirectUrl = Url.Action("Index", "MessagesManagement");
            return Json(new { success = true, redirectUrl });
        }

    }
}