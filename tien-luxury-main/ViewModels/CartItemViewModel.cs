using Humanizer;
using MongoDB.Bson;

namespace TienLuxury.ViewModels
{

    public class CartItemViewModel
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public int ProductPrice {get; set; }
        public string ImagePath { get; set; }
        public int Quantity { get; set; }
    }
}