using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TienLuxury.Models
{
    [Collection("reservation")]
    public class Reservation
    {
        private ObjectId Id;

        [Required(ErrorMessage = "Điền họ và tên")]
        [Display(Name = "Họ và tên")]
        private string fullName;

        [Required(ErrorMessage = "Điền số điện thoại")]
        [Display(Name = "Số điện thoại")]
        private string phoneNumber;

        [Required(ErrorMessage = "Nhập thời gian bạn muốn hẹn với tiệm")]
        [Display(Name = "Date")]
        private DateTime reservationDate;

        private DateTime createdDate;
        private string reservationStatus = "Chưa ghé";
        
        private List<ReservationDetail> reservationDetails = new List<ReservationDetail>();

        public ObjectId ID { get => Id; set => Id = value; }
        public string FullName { get => fullName; set => fullName = value; }
        public string PhoneNumber { get => phoneNumber; set => phoneNumber = value; }
        public DateTime ReservationDate { get => reservationDate; set => reservationDate = value; }
        public string ReservationStatus { get => reservationStatus; set => reservationStatus = value; }
        public List<ReservationDetail> ReservationDetails { get => reservationDetails; set => reservationDetails = value; }
        public DateTime CreatedDate { get => createdDate; set => createdDate = value; }
    }
}
