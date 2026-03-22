using Microsoft.EntityFrameworkCore;
using TienLuxury.Models;
using TienLuxury.Services;
using MongoDB.Bson;

namespace HairSalonWeb.Services
{
    public class ServiceService(DBContext hairSalonDBContext) : IServiceService
    {
        private readonly DBContext _dbContext = hairSalonDBContext;

        public async Task AddService(Service newService)
        {
            await _dbContext.Services.AddAsync(newService);

            _dbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_dbContext.ChangeTracker.DebugView.LongView);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Service>> GetAllServices()
            => await _dbContext.Services.AsNoTracking().ToListAsync();

        public async Task<IEnumerable<Service>> GetAllServicesActivated()
            => await _dbContext.Services.Where(s => s.IsActivated == true).ToListAsync();

        public  Service? GetServiceByID(ObjectId? Id)
            =>  _dbContext.Services.FirstOrDefault(s => s.ID == Id);

        public async Task<string?> GetServiceNameById(ObjectId? Id)
            => GetServiceByID(Id).ServiceName;

        public async Task RemoveService(Service serviceToRemove)
        {
            Service removedService = GetServiceByID(serviceToRemove.ID);

            if (removedService != null)
            {
                _dbContext.Services.Remove(removedService);

                _dbContext.ChangeTracker.DetectChanges();
                Console.WriteLine(_dbContext.ChangeTracker.DebugView.LongView);

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Service cannot be found.");
            }
        }

        public async Task UpdateService(Service serviceToUpdate)
        {
            Service updatedService = GetServiceByID(serviceToUpdate.ID);

            if (updatedService != null)
            {
                updatedService.ServiceName = serviceToUpdate.ServiceName;
                updatedService.Price = serviceToUpdate.Price;
                updatedService.Description = serviceToUpdate.Description;
                updatedService.IsActivated = serviceToUpdate.IsActivated;
                updatedService.ImagePath = serviceToUpdate.ImagePath;
                updatedService.ServiceType = serviceToUpdate.ServiceType;

                _dbContext.Services.Update(updatedService);

                _dbContext.ChangeTracker.DetectChanges();
                Console.WriteLine(_dbContext.ChangeTracker.DebugView.LongView);

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Service cannot be found.");
            }
        }
    }
}
