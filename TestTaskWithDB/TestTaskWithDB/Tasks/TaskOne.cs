using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;

namespace TestTaskWithDB.Tasks
{
    /// <summary>
    /// Реализация <see cref="ICommandHandler">ITask</see>
    /// <para>
    /// Задача 1
    /// <br/><b>Задача</b>: Создание бд и таблицы с полями справочника сотрудников,
    /// <br/>представляющими "Фамилию Имя Отчество", "дату рождения", "пол".
    /// </para>
    /// </summary>
    public class TaskOne : ICommandHandler
    {
        private readonly ILogger<TaskOne> _logger;
        private readonly IDBService _dBService;
        public string Command { get; init; } = "1";
        public TaskOne(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TaskOne>>();
            _dBService = serviceProvider.GetRequiredService<IDBService>();
        }
        public async Task<bool> Invoke(string[] args)
        {
            // Выводим задание
            PrintTextTask();

            _logger.LogInformation(
                """
                !!!!!Начало работы задачи!!!!!
                Для подробностей измените уровень логирования в appsettings.json на Debug
                """);
            // Создаём таблицу и бд, если это необходимо
            bool result = await _dBService.CreateDB();

            _logger.LogInformation("!!!!!Конец работы задачи!!!!!");

            return result;
        }
        /// <summary>
        /// Метод вывода описания задачи в логгер
        /// </summary>
        private void PrintTextTask()
        {
            _logger.LogInformation(
                """
                Задача: 
                    Создание таблицы с полями справочника сотрудников,
                    представляющими "Фамилию Имя Отчество", "дату рождения", "пол".
                Решение:
                    Так как мы используем ADO.NET Entity Framework, то создание таблицы
                    как и базы данных, можно возложить на него через метод EnsureCreated().
                """);
        }

        public override string ToString()
        {
            return $"Задача: {this.GetType().Name} / Команда: {Command}";
        }
    }
}
