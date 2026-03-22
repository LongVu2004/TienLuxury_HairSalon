using TienLuxury.Models;

using Microsoft.EntityFrameworkCore;
using TienLuxury.Services;
using MongoDB.Bson;

namespace HairSalonWeb.Services
{
    public class ReservationService(DBContext dBContext) : IReservationService
    {
        private readonly DBContext _dbContext = dBContext;

        public async Task<ObjectId> AddReservation(Reservation newReservation)
        {
            await _dbContext.Reservations.AddAsync(newReservation);

            _dbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_dbContext.ChangeTracker.DebugView.LongView);

            _dbContext.SaveChanges();
            return newReservation.ID;
        }

        public async Task UpdateReservationStatus(Reservation reservationToUpdate)
        {
            Reservation reservation = await _dbContext.Reservations.FirstOrDefaultAsync(r => r.ID == reservationToUpdate.ID);
            
            if (reservation == null)
            {
                return;
            }
            reservation.ReservationStatus = reservationToUpdate.ReservationStatus;
            _dbContext.Reservations.Update(reservation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteReservation(ObjectId id)
        {
            var reservation = await _dbContext.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _dbContext.Reservations.Remove(reservation);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Reservation>> GetAllReservation()
            => await _dbContext.Reservations.OrderBy(booking => booking.ReservationDate).Take(50).ToListAsync();
            
        public async Task<Reservation?> GetBookingByID(ObjectId id)
             => await _dbContext.Reservations.FirstOrDefaultAsync(booking => booking.ID == id);
    }
}
