using TienLuxury.Models;
using MongoDB.Bson;

namespace TienLuxury.Services
{
    public interface IEmployeeService
    {
        public Task AddEmployee(Employee employee);
        public Task UpdateEmployee(Employee employeeToUpdate);

        public Task DeleteEmployee(ObjectId id);

        public Task<IEnumerable<Employee>> GetAllEmployees();

        public Task<Employee> GetEmployee(ObjectId id);

    }
}