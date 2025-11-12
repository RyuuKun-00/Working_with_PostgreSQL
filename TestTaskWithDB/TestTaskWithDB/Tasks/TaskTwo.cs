

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.Enums;
using TestTaskWithDB.Model;
using TestTaskWithDB.Services;

namespace TestTaskWithDB.Tasks
{
    public class TaskTwo : ICommandHandler
    {
        private readonly ILogger<TaskTwo> _logger;
        private readonly IEmployeeService _employeeService;
        private readonly IDBService _dBService;
        public string Command { get; set; } = "2";

        public TaskTwo(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TaskTwo>>();
            _employeeService = serviceProvider.GetRequiredService<IEmployeeService>();
            _dBService = serviceProvider.GetRequiredService<IDBService>();
        }

        public async Task<bool> Invoke(string[] args)
        {
            _logger.LogInformation(
                """
                Задача:
                    Для работы с данными создать класс и создавать объекты.
                    При вводе создавать новый объект класса, с введенными пользователем данными.
                    При генерации строчек в базу создавать объект и 
                    его отправлять в базу/формировать строчку для отправки нескольких строк в БД.
                    У объекта должны быть методы, которые:
                        - отправляют объект в БД,
                        - рассчитывают возраст (полных лет).
                """);

            var employee = Deserialization(args);

            if(employee is null)
            {
                _logger.LogWarning("Не удалось десериализовать вводные параметры в объект.");
                return false;
            }

            var isExists =await _dBService.CheckDatabaseExistsAsync();

            if (!isExists)
            {
                _logger.LogWarning(
                    $"""
                    Не удалось выполнить операцию по добавлению сотрудника в БД.
                    Причина: БД не существует или строка подключения задана не верно.
                    """);
                return false;
            }

            await _employeeService.AddEmployee(employee);

            _logger.LogInformation($"Добавлен:\r\n{employee.ToString()}");

            return true;
        }

        private Employee? Deserialization(string[] args)
        {
            string fullName = args[1];

            var isTry = DateOnly.TryParse(args[2], out var dob);
            if (!isTry)
            {
                _logger.LogWarning(
                    $"""
                    Не удалось востановить объект "Сотрудник".
                    Причина:
                        Значение: {args[2]} - неудалось привести к типу даты.
                    """);
                return null;
            }

            isTry = Enum.TryParse<Gender>(args[3], true, out var gender);
            if (!isTry)
            {
                _logger.LogWarning(
                    $"""
                    Не удалось востановить объект "Сотрудник".
                    Причина:
                        Значение: {args[3]} - неудалось привести к типу Gender.
                    """);
                return null;
            }

            return new Employee(Guid.NewGuid(), fullName, dob, gender);
        }

        public override string ToString()
        {
            return "Задача№2 " + this.GetType().Name;
        }
    }
}
