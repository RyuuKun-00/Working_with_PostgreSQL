

using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;
using TestTaskWithDB.Abstractions;

namespace TestTaskWithDB.Tasks
{
    /// <summary>
    /// Реализация <see cref="ICommandHandler">ITask</see>
    /// <para>
    /// Задача 1
    /// <br/><b>Задача</b>: Создание таблицы с полями справочника сотрудников,
    /// <br/>представляющими "Фамилию Имя Отчество", "дату рождения", "пол".
    /// </para>
    /// </summary>
    public class TaskOne : ICommandHandler
    {
        private readonly ILogger<TaskOne> _logger;
        private readonly IDBService _dBService;
        public string Command { get; init; } = "1";
        public TaskOne(ILogger<TaskOne> logger,
                       IDBService dBService)
        {
            _logger = logger;
            _dBService = dBService;
        }
        public async Task<bool> Invoke(string[] args)
        {
            _logger.LogInformation(
                """
                Задача: Создание таблицы с полями справочника сотрудников,
                представляющими "Фамилию Имя Отчество", "дату рождения", "пол".
                """);
                
            return await _dBService.CreateDB();
        }

        public override string ToString()
        {
            return "Задача№1 "+this.GetType().Name;
        }
    }
}
