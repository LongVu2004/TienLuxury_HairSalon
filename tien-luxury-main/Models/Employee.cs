using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;

namespace TienLuxury.Models
{

    [Collection("employee")]
    public class Employee
    {
        private ObjectId id;
        private string name;
        private bool gender;
        private string position;
        private string imagePath = string.Empty;

        public ObjectId ID { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public bool Gender { get => gender; set => gender = value; }
        public string Position { get => position; set => position = value; }
        public string ImagePath { get => imagePath; set => imagePath = value; }
    }
}