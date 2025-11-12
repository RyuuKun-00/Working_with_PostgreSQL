

using TestTaskWithDB.Abstractions;
using TestTaskWithDB.DataAccess.Entities;
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
    }
}
