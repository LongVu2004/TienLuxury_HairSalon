using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;

namespace TienLuxury.Models
{
    [Collection("reservation-detail")]
    public class ReservationDetail
    {
        private ObjectId reservationID;
        private ObjectId serviceID;

        public ObjectId ReservationID { get => reservationID; set => reservationID = value; }
        public ObjectId ServiceID { get => serviceID; set => serviceID = value; }
    }
}