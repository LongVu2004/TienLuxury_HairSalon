using TienLuxury.Models;

namespace TienLuxury.ViewModels
{
    public class ReservationViewModel
    {
        public Reservation Reservation { get; set; }
        public List<Service> Services { get; set; }
    }
}