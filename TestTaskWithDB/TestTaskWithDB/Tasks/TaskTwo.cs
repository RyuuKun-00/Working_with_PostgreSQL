using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.Enums;
using TestTaskWithDB.Model;

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
            // Выводим задание
            PrintTextTask();
            // Десериализуем входные аргументы
            var employees = Deserialization(args);

            if(employees is null)
            {
                return false;
            }
            // Проверка БД
            var isExist = await CheckDatabaseExists();
            if (!isExist)
            {
                return false;
            }

            // Добавление сотрудника
            var countEmployees = await _employeeService.AddEmployee(employees);

            // Вывод результата работы 
            string textResult = $"В БД добавлено {countEmployees} сотрудников.\r\n";

            for(int i =0;i < employees.Count;i++)
            {
                textResult += $"# {i + 1}\r\n{employees[i].ToString()}";
            }

            _logger.LogInformation(textResult);

            return true;
        }

        /// <summary>
        /// Метод вывода описания задачи в логгер
        /// </summary>
        private void PrintTextTask()
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
                Решение:
                    Так как мы используем ADO.NET Entity Framework, то сотрудники на строне БД
                    представляются сущностями EmployeeEntity, а на стороне бизнес-логики
                    моделями Employee.
                    Возраст сорудника возвращается из модели Employee, через свойство Age от 
                    сегодняшнего дня(если ДР. сегодня, то год прибавляется).
                    Для отправки в бд используется единичная отправка или сразу группы сотрудников
                    через перегруженный метод одной транзакцией(внутренняя функциональность EF).
                """);
        }
        /// <summary>
        /// Метод проверки возможности работы с БД
        /// </summary>
        /// <returns>Результат проверки</returns>
        private async Task<bool> CheckDatabaseExists()
        {
            var isExists = await _dBService.CheckDatabaseExistsAsync();

            if (!isExists)
            {
                _logger.LogWarning(
                    $"""
                    Не удалось выполнить операцию по добавлению сотрудника в БД.
                    Причина: БД не существует или строка подключения задана не верно.
                    """);
                return false;
            }

            _logger.LogDebug("БД досутпна для запросов!!!");

            return true;
        }

        /// <summary>
        /// Метод десериализации аргументов приложения в объект <see cref="Employee">Employee</see> (Сотрудник)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private List<Employee>? Deserialization(string[] args)
        {
            var result = new List<Employee>();
            // Парсим сотрудников
            for(int i = 1;i<args.Length;i+=3)
            {
                // Первый аргумент ФИО
                string fullName = args[i];
                // Проверка на наличие аргумента
                if(i+1 >= args.Length)
                {
                    _logger.LogWarning(
$"""
                    Не удалось востановить объект "Сотрудник".
                    ФИО: {fullName}
                    Причина:
                        Не заданы "Дата рождения" и "Пол" сотрудника.
                    """);
                    return null;
                }
                // Попытка парсинга даты рождения сотрудника
                var isTry = DateOnly.TryParse(args[i+1], out var dob);
                // Если это не дата рождения -> ошибка
                if (!isTry)
                {
                    _logger.LogWarning(
                    $"""
                    Не удалось востановить объект "Сотрудник".
                    ФИО: {fullName}
                    Причина:
                        Значение: {args[i+1]} - неудалось привести к типу даты.
                    """);
                    return null;
                }
                // Проверка на наличие аргумента
                if (i + 1 >= args.Length)
                {
                    _logger.LogWarning(
                    $"""
                    Не удалось востановить объект "Сотрудник".
                    ФИО: {fullName}
                    Дата рож.: {args[i + 1]}
                    Причина:
                        Не задан "Пол" сотрудника.
                    """);
                    return null;
                }
                // Попытка парсинга пола сотрудника
                isTry = Enum.TryParse<Gender>(args[i+2], true, out var gender);
                // Если это не пол -> ошибка
                if (!isTry)
                {
                    _logger.LogWarning(
                        $"""
                    Не удалось востановить объект "Сотрудник".
                    Причина:
                        Значение: {args[i + 2]} - неудалось привести к типу Gender.
                    """);
                    return null;
                }

                result.Add(new Employee(Guid.NewGuid(), fullName, dob, gender));

                _logger.LogDebug($"Всего спаршено сотрудников из аргументов: {result.Count}");
            }
            return result;
        }

        public override string ToString()
        {
            return $"Задача: {this.GetType().Name} / Команда: {Command}";
        }
    }
}
