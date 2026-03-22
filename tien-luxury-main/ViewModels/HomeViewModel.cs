using System.ComponentModel.DataAnnotations;
using TienLuxury.Models;

namespace TienLuxury.ViewModels
{
    public class HomeViewModel
    {
        // public IEnumerable<Service> Services { get; set; }
        public IEnumerable<Employee> Employees { get; set; }
    }
}