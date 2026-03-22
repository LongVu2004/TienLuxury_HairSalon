using MongoDB.Bson;

namespace TienLuxury.Areas.Admin.ViewModels
{
    public class ServiceDeleteViewModel
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
    }
}
