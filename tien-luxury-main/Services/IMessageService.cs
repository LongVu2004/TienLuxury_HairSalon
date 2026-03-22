using TienLuxury.Models;
using MongoDB.Bson;

namespace TienLuxury.Services
{
    public interface IMessageService
    {
        public Task<List<Message>> GetAllMessage();
        public Task CreateMessage(Message message);
        public Task DeleteMessage(Message message);
        public Task<Message> FindMessageById(ObjectId id);
        public Task<Message> GetMessageById(ObjectId id);

    }
}