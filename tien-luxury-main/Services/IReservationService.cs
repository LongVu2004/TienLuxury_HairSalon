using TienLuxury.Models;
using MongoDB.Bson;

namespace TienLuxury.Services
{
    public interface IReservationService
    {
        public Task<ObjectId> AddReservation(Reservation newReservation);

        public Task UpdateReservationStatus(Reservation reservationToUpdate);
        public Task DeleteReservation(ObjectId id);

        public Task<IEnumerable<Reservation>> GetAllReservation();

        public Task<Reservation?> GetBookingByID(ObjectId id);
    }
}
