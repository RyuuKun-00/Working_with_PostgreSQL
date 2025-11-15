

using Microsoft.EntityFrameworkCore;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.DataAccess.Entities;
using TestTaskWithDB.DataAccess.Extensions;
using TestTaskWithDB.Enums;
using TestTaskWithDB.Model;

namespace TestTaskWithDB.DataAccess.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationContext _context;

        public EmployeeRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<Guid> AddEmployee(Employee employee)
        {
            var employeeEntity = new EmployeeEntity()
            {
                Id = employee.Id,
                FullName = employee.FullName,
                DOB = employee.DOB,
                Gender = employee.Gender
            };

            await _context.Employees.AddAsync(employeeEntity);
            await _context.SaveChangesAsync();

            return employeeEntity.Id;
        }

        public async Task<int> AddEmployee(List<Employee> employees)
        {
            List<EmployeeEntity> employeeEntities = new List<EmployeeEntity>();
            foreach (var employee in employees)
            {
                var employeeEntity = new EmployeeEntity()
                {
                    Id = new Guid(),
                    FullName = employee.FullName,
                    DOB = employee.DOB,
                    Gender = employee.Gender
                };

                employeeEntities.Add(employeeEntity);
            }
            await _context.Employees.AddRangeAsync(employeeEntities);
            var count = await _context.SaveChangesAsync();

            return count;
        }

        public async Task<List<Employee>> GetUniqueEmployees()
        {
            var entities = await _context.Employees
                                         .FromSqlRaw("SELECT * FROM GetEmployees();")
                                         .ToListAsync();
            return entities.Select<EmployeeEntity, Employee>(e => new Employee(e.Id, e.FullName, e.DOB, e.Gender))
                           .ToList();
        }

        public async Task<List<Employee>> Get(string prefixFullName,Gender gender, bool asTracking)
        {
            var entities =await _context.Employees
                                  .Where(e => e.Gender == gender && EF.Functions.Like(e.FullName, $"{prefixFullName}%"))
                                  .SetTracking<EmployeeEntity>(asTracking)
                                  .ToListAsync();

            return entities.Select<EmployeeEntity, Employee>(e => new Employee(e.Id, e.FullName, e.DOB, e.Gender))
                           .ToList();
        }

        public async Task<List<Employee>> GetFunc(string prefixFullName, Gender gender,bool asTracking)
        {
            var entities = await _context.Employees
                                         .FromSqlRaw("Select * FROM GetEmpl({0}::varchar ,{1}::smallint);",
                                         prefixFullName, gender)
                                         .SetTracking<EmployeeEntity>(asTracking)
                                         .ToListAsync();

            return entities.Select<EmployeeEntity, Employee>(e => new Employee(e.Id, e.FullName, e.DOB, e.Gender))
                           .ToList();
        }


    }
}
