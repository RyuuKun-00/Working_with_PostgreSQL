using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.Enums;
using TestTaskWithDB.Model;

namespace TestTaskWithDB.Tasks
{
    public class TaskThree : ICommandHandler
    {
        private readonly ILogger<TaskThree> _logger;
        private readonly IEmployeeService _employeeService;
        private readonly IDBService _dBService;
        public string Command { get; set; } = "3";

        public TaskThree(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TaskThree>>();
            _employeeService = serviceProvider.GetRequiredService<IEmployeeService>();
            _dBService = serviceProvider.GetRequiredService<IDBService>();
        }

        public async Task<bool> Invoke(string[] args)
        {
            // Выводим задание
            PrintTextTask();

            // Проверка БД
            var isExist = await CheckDatabaseExists();
            if (!isExist)
            {
                return false;
            }

            // Проверка на наличие аргумента для добавление тестовых значений в бд
            if (args.Length>1 && args[1] == "+")
            {
                await AddEmployees();
            }

            // Создание функции в БД
            await CreateFunctionGetUniqueEmployees();

            // Получение уникальных сотрудников ФИО+дата
            // через встроенную функцию GetEmployees()
            var result = await _employeeService.GetUniqueEmployees();

            PrintEmployees(result);

            return true;
        }

        /// <summary>
        /// Метод вывода описания задачи в логгер
        /// </summary>
        private void PrintTextTask()
        {
            _logger.LogInformation(
                $""" 
                Задача:
                    Вывод всех строк справочника сотрудников, с уникальным значением ФИО+дата, 
                    отсортированным по ФИО. Вывести ФИО, Дату рождения, пол, кол-во полных лет.
                Решение:
                    Для выполнения данного запроса было решено добавить в бд напрямую функцию,
                    которая находит униклаьных ползователей по параметру "ФИО+дата".
                Модификации:
                    После команды "{Command}" можно указать дополнительную команду "+",
                    что добавит перед выполнением запроса тестовые записи в бд с уникальными "ФИО+дата".
                    Пример:
                        {AppDomain.CurrentDomain.FriendlyName}.exe {Command} +
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
        /// Метод создания функции в БД для получения уникальных сотрудников ФИО+дата
        /// </summary>
        /// <returns></returns>
        private async Task<int> CreateFunctionGetUniqueEmployees()
        {
            // Запрос для добавление встроенной функции в бд для выпонения 2 задания
            // Получения уникальных сотрудников
            var sqlQuery =
                """
                CREATE OR REPLACE FUNCTION GetEmployees()
                RETURNS TABLE
                (
                	"Id" uuid, 
                	"FullName" text,
                	"DOB" date,
                	"Gender" smallint
                	)
                AS $$
                BEGIN
                  RETURN QUERY
                    SELECT
                	    "Employees".*
                    FROM "Employees",
                    (
                	    SELECT "Employees"."FullName",
                			    "Employees"."DOB",
                			    COUNT(*) as "Count"
                	    FROM "Employees"
                	    GROUP BY "Employees"."FullName",
                			     "Employees"."DOB"
                	    HAVING COUNT(*) = 1
                	    ORDER BY "Employees"."FullName" ASC
                    ) AS "UniqueEmployees"
                    WHERE "Employees"."FullName" = "UniqueEmployees"."FullName"
                	    AND "Employees"."DOB" = "UniqueEmployees"."DOB";
                END;
                $$ LANGUAGE plpgsql;

                ALTER FUNCTION public.getemployees()
                OWNER TO postgres;
                """;

            var result = await _dBService.ExecuteSql(sqlQuery);

            _logger.LogDebug("Добавлена встроенная функция в бд для выпонения 2 задания.\r\nКоманда:\r\n{sqlQuery}", sqlQuery);

            return result;
        }

        /// <summary>
        /// Метод добавления сотрудников в БД
        /// </summary>
        private async Task AddEmployees()
        {
            List<Employee> employees = new List<Employee>()
            {
                new Employee(Guid.NewGuid(),"Семёнов Роман Олегович",new DateOnly(1999,6,1),Gender.Male),
                new Employee(Guid.NewGuid(),"Семёнов Артём Олегович",new DateOnly(1999,6,1),Gender.Male),// Уникальный
                new Employee(Guid.NewGuid(),"Семёнов Михаил Олегович",new DateOnly(1999,6,1),Gender.Male),// Уникальный
                new Employee(Guid.NewGuid(),"Семёнов Роман Олегович",new DateOnly(1999,6,1),Gender.Male),
                new Employee(Guid.NewGuid(),"Семёнов Роман Олегович",new DateOnly(1999,6,1),Gender.Male),
                new Employee(Guid.NewGuid(),"Киррова Милена Михайловна",new DateOnly(1999,6,3),Gender.Female),
                new Employee(Guid.NewGuid(),"Киррова Милена Михайловна",new DateOnly(1999,6,3),Gender.Female),
                new Employee(Guid.NewGuid(),"Киррова Милена Михайловна",new DateOnly(2004,6,3),Gender.Female),// Уникальный
                new Employee(Guid.NewGuid(),"Юсупова Елена Владимировна",new DateOnly(2005,6,3),Gender.Female),// Уникальный
                new Employee(Guid.NewGuid(),"Юсупова Елена Владимировна",new DateOnly(1999,6,3),Gender.Female)// Уникальный
            };

            await _employeeService.AddEmployee(employees);

            _logger.LogDebug($"В БД добавлены \"тестовые\" сотрудники в кол-ве: {employees.Count}");
        }

        /// <summary>
        /// Метод вывода информации о полученных сотрудниках
        /// </summary>
        /// <param name="employees">Списко сотрудников</param>
        private void PrintEmployees(List<Employee> employees)
        {
            // Максимальное кол-во выводимых строк в лог(чтобы не перегружать систему)
            int numberToBreak = 100;
            // Делегат функции дл сериализации объекта Employee (Сотрудник)
            Func<string, string, string, string, string, string> funcFormat =
                (string n, string fio, string dob, string gender, string age) =>
                String.Format("| {0,-3} | {1,-30} | {2,-10} | {3,-10} | {4,-7} |\r\n", n, fio, dob, gender, age);

            // Формируем шапку таблицы
            string table = funcFormat("№", "ФИО", "Дата рож.", "Пол", "Возраст");

            
            int widthTable = table.Length - 2;

            table = "\r\nТаблица уникальных сотрудников ФИО+дата\r\n" 
                  + new string('-', widthTable) + "\r\n"
                  + table
                  + new string('-', widthTable) + "\r\n";

            // Формируем тело таблицы
            int count = 0;
            foreach (var empl in employees)
            {
                count++;
                table += funcFormat(count.ToString(),
                                 empl.FullName,
                                 empl.DOB.ToString(),
                                 empl.Gender.ToString(),
                                 empl.Age.ToString());

                if (count >= numberToBreak)
                {
                    break;
                }
            }

            table += new string('-', widthTable) + "\r\n";

            _logger.LogInformation(table);
        }

        public override string ToString()
        {
            return $"Задача: {this.GetType().Name} / Команда: {Command}";
        }
    }
}
