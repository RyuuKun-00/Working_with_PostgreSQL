using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.Enums;
using TestTaskWithDB.Model;

namespace TestTaskWithDB.Services
{
    /// <summary>
    /// Реализация <see cref="INpgsqlEmployeeService">INpgsqlEmployeeService</see>
    /// <br/>Класс сервиса по управлению сотрудниками
    /// </summary>
    public class NpgsqlEmployeeService : INpgsqlEmployeeService
    {
        private readonly ILogger<NpgsqlEmployeeService> _logger;
        private readonly INpgsqlRepository _repository;

        public NpgsqlEmployeeService(ILogger<NpgsqlEmployeeService> logger,
                                     INpgsqlRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public bool ConnectionOpen()
        {
            return _repository.ConnectionOpen();
        }

        public async Task<int> AddEmployees(List<Employee> employees, CancellationToken token = default)
        {
            var sqlCommand = "COPY public.\"Employees\"(\"Id\",\"FullName\",\"DOB\",\"Gender\") FROM STDIN BINARY";
            int countEmployees = await _repository.InsertBatch(employees, sqlCommand, token);
            _logger.LogDebug("Добавлено сотрудников в БД: {countEmployees}", countEmployees);
            return countEmployees;
        }

        public async Task<List<Employee>> GetData(string prefixFullName,Gender gender, CancellationToken token = default)
        {
            var sqlCommand = 
                $"""
                COPY (SELECT *
                      FROM public."Employees"
                      WHERE "Gender" = {(byte)gender}
                        AND "FullName" ILIKE '{prefixFullName.Trim()}%') 
                TO STDIN BINARY
                """;
            var employees = await _repository.GetData( sqlCommand, token);
            _logger.LogDebug("Получено сотрудников: {Count}", employees.Count);
            return employees;
        }
    }
}
