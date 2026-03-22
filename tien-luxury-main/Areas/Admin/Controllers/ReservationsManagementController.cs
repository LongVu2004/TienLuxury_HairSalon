using Microsoft.AspNetCore.Mvc;
using TienLuxury.Services;
using TienLuxury.Areas.Admin.ViewModels;
using TienLuxury.Models;
using MongoDB.Bson;
using TienLuxury.Areas.Filter;
using Microsoft.AspNetCore.Authorization;

namespace TienLuxury.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [DesktopOnly]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ReservationsManagementController(IReservationService reservationService, IServiceService serviceService, IReservationDetailService reservationDetailService) : Controller
    {
        private readonly IReservationService _reservationService = reservationService;
        private readonly IServiceService _serviceService = serviceService;
        private readonly IReservationDetailService _reservationDetailService = reservationDetailService;

        public async Task<IActionResult> Index()
        {
            ReservationListViewModel model = new()
            {
                Reservations = await _reservationService.GetAllReservation(),
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateReservationStatus(ObjectId id)
        {
            Reservation? reservation = await _reservationService.GetBookingByID(id);
            reservation.CreatedDate = new DateTime(
                reservation.CreatedDate.Year,
                reservation.CreatedDate.Month,
                reservation.CreatedDate.Day,
                reservation.CreatedDate.Hour,
                reservation.CreatedDate.Minute,
                0  // Đặt giây = 0
            );


            if (reservation != null)
            {
                List<ReservationDetail> details = (await _reservationDetailService.GetAllDetailsByReservationID(id)).ToList();
                List<Service> servicesBooked = new();

                IEnumerable<Service> allServices = await _serviceService.GetAllServices();

                foreach (ReservationDetail d in details)
                {
                    servicesBooked.Add(allServices.FirstOrDefault(s => s.ID == d.ServiceID));
                }

                ReservationAddEditViewModel model = new()
                {
                    Reservation = await _reservationService.GetBookingByID(id),
                    Services = servicesBooked
                };

                return View(model);
            }
            return Json(new { success = false, message = "Không tìm thấy lịch đặt hẹn nào" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReservationStatus(ReservationAddEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _reservationService.UpdateReservationStatus(model.Reservation);

                return Redirect("Index");
            }
            return Json(new { success = false, message = "Cập nhật thất bại" });
        }

        [HttpGet]
        public IActionResult DeleteComfirmation()
            => View();

        [HttpPost]
        public async Task<IActionResult> DeleteReservation(ObjectId id)
        {
            Reservation? reservation = await _reservationService.GetBookingByID(id);
            if (reservation != null)
            {
                var reservationDetails = await _reservationDetailService.GetAllDetailsByReservationID(id);
                if (reservationDetails != null && reservationDetails.Count() > 0)
                {
                    foreach (var detail in reservationDetails)
                    {
                        await _reservationDetailService.DeleteReservationDetail(detail.ReservationID);
                    }
                }
                
                await _reservationService.DeleteReservation(reservation.ID);

                return RedirectToAction("Index");
            }
            else
            {
                return Json(new { success = false, message = "Không tìm thấy lịch đặt hẹn nào" });
            }
        }
    }
}
