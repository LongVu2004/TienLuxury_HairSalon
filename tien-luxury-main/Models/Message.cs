using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;

namespace TienLuxury.Models
{
    [Collection("message")]
    public class Message
    {
        private ObjectId id;
        private string customerName;
        private string phoneNumber;
        private string? email = "";
        private DateTime createdAt = DateTime.Now;
        private string content;

        public ObjectId Id { get => id; set => id = value; }
        public string CustomerName { get => customerName; set => customerName = value; }
        public string PhoneNumber { get => phoneNumber; set => phoneNumber = value; }
        public string Content { get => content; set => content = value; }
        public string? Email { get => email; set => email = value; }
        public DateTime CreatedAt { get => createdAt; set => createdAt = value; }
    }
}