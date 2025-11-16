using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.Enums;
using TestTaskWithDB.Model;

namespace TestTaskWithDB.Services
{
    /// <summary>
    /// Реализация <see cref="IEFEmployeeService">IEFEmployeeService</see>
    /// <br/>Класс сервиса по управлению сотрудниками
    /// </summary>
    public class EFEmployeeService : IEFEmployeeService
    {
        private readonly ILogger<EFEmployeeService> _logger;
        private readonly IEmployeeRepository _repository;

        public EFEmployeeService(ILogger<EFEmployeeService> logger,
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

        public async Task<List<Employee>> Get(string prefixFullName, Gender gender, bool asTracking)
        {
            var result = await _repository.Get(prefixFullName.Trim(),gender,asTracking);
            _logger.LogDebug($"Получено сотрудников: {result.Count}");
            return result;
        }

        public async Task<List<Employee>> GetFunc(string prefixFullName, Gender gender, bool asTracking)
        {
            var result = await _repository.GetFunc(prefixFullName.Trim(), gender,asTracking);
            _logger.LogDebug($"Получено сотрудников: {result.Count}");
            return result;
        }
    }
}
