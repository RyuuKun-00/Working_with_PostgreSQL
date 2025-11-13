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
            _logger.LogInformation(
                """ 
                Задача:
                    Вывод всех строк справочника сотрудников, с уникальным значением ФИО+дата, 
                    отсортированным по ФИО. Вывести ФИО, Дату рождения, пол, кол-во полных лет.
                """);

            var isExists = await _dBService.CheckDatabaseExistsAsync();

            if (!isExists)
            {
                _logger.LogWarning(
                    $"""
                    Не удалось выполнить операцию по поиску уникальных сотрудников ФИО+дата.
                    Причина: БД не существует или строка подключения задана не верно.
                    """);
                return false;
            }

            if (args[1] == "+")
            {
                await AddEmployees();
            }

            var result = await _employeeService.GetUniqueEmployees();

            PrintEmployees(result);

            return true;
        }

        private async Task AddEmployees()
        {
            await _employeeService.AddEmployee(new List<Employee>()
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
            });
        }

        private void PrintEmployees(List<Employee> employees)
        {

            Func<string, string, string, string, string, string> funcFormat =
                (string n, string fio, string dob, string gender, string age) =>
                String.Format("| {0,-3} | {1,-30} | {2,-10} | {3,-10} | {4,-7} |\r\n", n, fio, dob, gender, age);

            string mes = "\r\nТаблица уникальных сотрудников ФИО+дата\r\n" +
                         funcFormat("№", "ФИО", "Дата рож.", "Пол", "Возраст");

            int count = 0;
            foreach (var empl in employees)
            {
                count++;
                mes += funcFormat(count.ToString(),
                                 empl.FullName,
                                 empl.DOB.ToString(),
                                 empl.Gender.ToString(),
                                 empl.Age.ToString());
            }

            _logger.LogInformation(mes);
        }

        public override string ToString()
        {
            return "Задача№3 " + this.GetType().Name;
        }
    }
}
