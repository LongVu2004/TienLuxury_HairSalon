using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TienLuxury.Models;
using TienLuxury.Services;
using TienLuxury.ViewModels;
using MongoDB.Bson;

namespace TienLuxury.Controllers
{
    public class ReservationController(IServiceService serviceService, IReservationService reservationService, IReservationDetailService reservationDetailService) : Controller
    {
        private readonly IServiceService _seviceService = serviceService;
        IReservationService _reservationService = reservationService;
        IReservationDetailService _reservationDetailService = reservationDetailService;

        public async Task<IActionResult> Index()
        {
            ServiceByTypeListViewModel model = new()
            {
                Hair = new List<Service>(),
                SkinCare = new List<Service>(),
                Others = new List<Service>(),
            };

            IEnumerable<Service> allServices = await _seviceService.GetAllServicesActivated();

            foreach (Service s in allServices)
            {
                if (s.ServiceType == "Dịch vụ tóc")
                {
                    model.Hair.Add(s);
                }
                else
                {
                    if (s.ServiceType == "Skin care")
                    {
                        model.SkinCare.Add(s);
                    }
                    else
                    {
                        model.Others.Add(s);
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation(string FullName, string PhoneNumber, DateTime? ReservationDate, string SelectedServicesIds)
        {
            if (string.IsNullOrEmpty(SelectedServicesIds) || string.IsNullOrEmpty(PhoneNumber)
                || string.IsNullOrEmpty(FullName) || ReservationDate == null)
            {
                return Redirect("Index");
            }

            var selectedIds = System.Text.Json.JsonSerializer.Deserialize<List<string>>(SelectedServicesIds);

            IEnumerable<Service> allServices = await _seviceService.GetAllServicesActivated();

            // ReservationViewModel model = new()
            // {
            //     Services = new()
            // };

            Reservation reservation = new()
            {
                FullName = FullName,
                PhoneNumber = PhoneNumber,
                ReservationDate = (DateTime)ReservationDate,
                CreatedDate = DateTime.Now
            };

            // model.Reservation = reservation;
            reservation.ID = await _reservationService.AddReservation(reservation);
            
            foreach (string id in selectedIds)
            {
                Service service =  allServices.FirstOrDefault(s => s.ID.ToString() == id);

                // model.Services.Add(service);

                if (service != null)
                {
                    ReservationDetail detail = new()
                    {
                        ReservationID = reservation.ID,
                        ServiceID = ObjectId.Parse(id)
                    };
                    await _reservationDetailService.CreateReservationDetail(detail);
                    reservation.ReservationDetails.Add(detail);

                }
            }

            // TempData["ReservationId"] = reservation.ID.ToString();
            return RedirectToAction("Successful", "Reservation", new { id = reservation.ID});
            // return View("Successful", model);
        }

        public async Task<IActionResult> Successful(ObjectId id)
        {
            Reservation? reservation = await _reservationService.GetBookingByID(id);

            if (reservation == null)
            {
                return RedirectToAction("Index", "Home");
            }

            IEnumerable<Service> allServices = await _seviceService.GetAllServicesActivated();
            IEnumerable<ReservationDetail> reservationDetails = await _reservationDetailService.GetAllDetailsByReservationID(id);

            ReservationViewModel model = new()
            {
                Reservation = reservation,
                Services = reservationDetails
                    .Select(d => allServices.FirstOrDefault(s => s.ID == d.ServiceID))
                    .Where(s => s != null).ToList()!
            };

            return View(model);
        }
        
    }
}