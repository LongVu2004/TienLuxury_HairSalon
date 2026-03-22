using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TienLuxury.Areas.Filter
{
    public class DesktopOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString().ToLower();

            if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone") || userAgent.Contains("ipad"))
            {
                context.Result = new RedirectToActionResult("Index", "DeviceNotSupported", null);
            }

            base.OnActionExecuting(context);
        }
    }
}

