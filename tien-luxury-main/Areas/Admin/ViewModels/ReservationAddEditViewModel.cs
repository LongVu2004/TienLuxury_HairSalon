using TienLuxury.Models;

namespace TienLuxury.Areas.Admin.ViewModels
{
    public class ReservationAddEditViewModel
    {
        public Reservation Reservation { get; set; }
        public List<Service>? Services { get; set; }
    }
}