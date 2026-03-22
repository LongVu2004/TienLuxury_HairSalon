using Microsoft.EntityFrameworkCore;
using TienLuxury.Models;
using MongoDB.Bson;

namespace TienLuxury.Services
{
    public class EmployeeService(DBContext dbContext) : IEmployeeService
    {
        private readonly DBContext _dbContext = dbContext;

        public async Task AddEmployee(Employee employee)
        {
            await _dbContext.Employees.AddAsync(employee);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateEmployee(Employee employeeToUpdate)
        {
            Employee employeeUpdated = await _dbContext.Employees.FirstOrDefaultAsync(e => e.ID == employeeToUpdate.ID);

            if (employeeUpdated != null)
            {
                employeeUpdated.Name = employeeToUpdate.Name;
                employeeUpdated.Gender = employeeToUpdate.Gender;
                employeeUpdated.Position = employeeToUpdate.Position;
                employeeUpdated.ImagePath = employeeToUpdate.ImagePath;

                _dbContext.Employees.Update(employeeUpdated);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new System.Exception("Employee not found");
            }
        }

        public async Task DeleteEmployee(ObjectId id)
        {
            Employee employee = _dbContext.Employees.FirstOrDefault(e => e.ID == id);

            if (employee != null)
            {
                _dbContext.Employees.Remove(employee);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new System.Exception("Employee not found");
            }
        }

        public async Task<IEnumerable<Employee>> GetAllEmployees()
            => await _dbContext.Employees.AsNoTracking().ToListAsync();
        

        public async Task<Employee> GetEmployee(ObjectId id)
            => await _dbContext.Employees.FirstOrDefaultAsync(e => e.ID == id);


    }
}