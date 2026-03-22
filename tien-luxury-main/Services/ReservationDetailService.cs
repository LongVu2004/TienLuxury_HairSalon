using Microsoft.EntityFrameworkCore;
using TienLuxury.Models;
using MongoDB.Bson;

namespace TienLuxury.Services
{
    public class ReservationDetailService(DBContext dbContext) : IReservationDetailService
    {
        private readonly DBContext _dbContext = dbContext;

        public async Task CreateReservationDetail(ReservationDetail newDetail)
        {
            await _dbContext.ReservationDetails.AddAsync(newDetail);

            _dbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_dbContext.ChangeTracker.DebugView.LongView);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteReservationDetail(ObjectId id)
        {
            var reservationDetail = await _dbContext.ReservationDetails.FirstOrDefaultAsync(i => i.ReservationID == id);
            if (reservationDetail != null)
            {
                _dbContext.ReservationDetails.Remove(reservationDetail);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ReservationDetail>> GetAllDetailsByReservationID(ObjectId reservationId)
            => _dbContext.ReservationDetails.Where(r => r.ReservationID == reservationId);
    }
}