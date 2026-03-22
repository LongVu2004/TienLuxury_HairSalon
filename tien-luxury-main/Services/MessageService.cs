using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TienLuxury.Models;
using MongoDB.Bson;

namespace TienLuxury.Services
{
    public class MessageService(DBContext dbContext) : IMessageService
    {
        private readonly DBContext _dbContext = dbContext;
        public async Task CreateMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await _dbContext.Messages.AddAsync(message);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _dbContext.Messages.Remove(message);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Message> FindMessageById(ObjectId id)
            => await _dbContext.Messages.FirstOrDefaultAsync();

        public async Task<List<Message>> GetAllMessage()
            => await _dbContext.Messages.Take(100).OrderByDescending(m => m.CreatedAt).ToListAsync();

        public async Task<Message> GetMessageById(ObjectId id)
            => await _dbContext.Messages.FirstOrDefaultAsync(m => m.Id == id);
    }
}