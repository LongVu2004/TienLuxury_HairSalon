using TienLuxury.Models;

namespace TienLuxury.Areas.Admin.ViewModels
{
    public class HomeViewModel
    {
        public int ReservationsToDay { get; set; } = 0;
        public int OrdersToday { get; set; } = 0;
        public int NumberOfServices { get; set; } = 0;
        public int NumberOfProducts { get; set; } = 0;

        public Reservation? NearestBooking { get; set; }
        public Invoice? LatestOrder { get; set; }
        public Message? LatestMessage { get; set; }

    }
}