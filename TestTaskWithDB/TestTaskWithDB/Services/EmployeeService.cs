
using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.Model;

namespace TestTaskWithDB.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ILogger<EmployeeService> _logger;
        private readonly IEmployeeRepository _repository;

        public EmployeeService(ILogger<EmployeeService> logger,
                               IEmployeeRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<Guid> AddEmployee(Employee employee)
        {
            var id = await _repository.AddEmployee(employee);
            _logger.LogInformation($"Добавлен сотрудник.\r\nID: {id}");
            return id;
        }

        public async Task<int> AddEmployee(List<Employee> employees)
        {
            var count = await _repository.AddEmployee(employees);
            _logger.LogInformation($"Добавлено сотрудников: {count}");
            return count;
        }

        public async Task<List<Employee>> GetUniqueEmployees()
        {
            var result = await _repository.GetUniqueEmployees();
            _logger.LogInformation($"Получено сотрудников с уникальными ФИО+дата: {result.Count}");
            return result;
        }
    }
}
