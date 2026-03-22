using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TienLuxury.Models;
using TienLuxury.Services;
using TienLuxury.ViewModels;

namespace TienLuxury.Controllers
{
    public class ServiceController(IServiceService serviceService) : Controller
    {   

        private IServiceService _serviceService = serviceService;

        public async Task<IActionResult> Index()
        {
            IEnumerable<Service> allServices = await _serviceService.GetAllServicesActivated();

            List<Service> hair = new();
            List<Service> skinCare = new();
            List<Service> others = new();            

            foreach(Service s in allServices)
            {
                if (s.ServiceType == "Dịch vụ tóc")
                {
                    hair.Add(s);
                }
                else
                {
                    if (s.ServiceType == "Skin care")
                    {
                        skinCare.Add(s);
                    }
                    else
                    {
                        others.Add(s);
                    }   
                }
            }

            ServiceByTypeListViewModel model = new()
            {
                Hair = hair,
                SkinCare = skinCare,
                Others = others
            };

            return View(model);
        }
    }
}
