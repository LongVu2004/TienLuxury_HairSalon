using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using TienLuxury.Models;
using MongoDB.Bson;

namespace TienLuxury.Services
{
    public interface IReservationDetailService
    {
        public Task CreateReservationDetail(ReservationDetail newDetail);
        public Task<IEnumerable<ReservationDetail>> GetAllDetailsByReservationID(ObjectId reservationId);
        public Task DeleteReservationDetail(ObjectId id);

    }
}