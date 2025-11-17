using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;

namespace TestTaskWithDB.Tasks
{
    public class TaskSix : ICommandHandler
    {
        private readonly ILogger<TaskSix> _logger;
        private readonly IDBService _dBService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IInputArguments _arguments;
        public string Command { get; init; } = "6";
        public TaskSix(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TaskSix>>();
            _dBService = serviceProvider.GetRequiredService<IDBService>();
            _arguments = serviceProvider.GetRequiredService<IInputArguments>();
            _serviceProvider = serviceProvider;
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

            // Проверка БД
            var isExist = await CheckDatabaseExists();
            if (!isExist)
            {
                return false;
            }

            _logger.LogInformation("Удаляем индекс (employees_name_gender), если он есть в БД");
            // Удаляем индекс из бд
            string dropIndex = "DROP INDEX IF EXISTS employees_name_gender;";

            _logger.LogInformation("Вызываем задачу 5 для замеров без использования индексов!!!");

            await _dBService.ExecuteSql(dropIndex);

            // Запускаем выполнение задачи пять по поиску сотрудников
            var servise = new TaskFive(_serviceProvider);

            await servise.Invoke(_arguments.Args);

            _logger.LogInformation("Добавляем индекс (employees_name_gender), если его нет в БД");
            // Создаём индекс в БД
            string createIndex = "CREATE INDEX IF NOT EXISTS employees_name_gender ON public.\"Employees\" (\"FullName\",\"Gender\");";

            await _dBService.ExecuteSql(createIndex);

            _logger.LogInformation("Вызываем задачу 5 для замеров с использованием индексов!!!");

            // Запускаем выполнение задачи пять по поиску сотрудников
            await servise.Invoke(_arguments.Args);

            _logger.LogInformation("!!!!!Конец работы задачи!!!!!");

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
                    Произвести оптимизацию базы данных или запросов к базе для 
                    ускорения выполнения задачи 5. 
                    Убедиться, что время исполнения уменьшилось. 
                    Объяснить смысл произведенных действий. 
                    Предоставить результаты замера времени выполнения до и после оптимизации.
                Решение:
                    Основное решение добавление индекса по столбцам выборки:
                        CREATE INDEX IF NOT EXISTS employees_name_gender 
                            ON public."Employees" ("FullName","Gender");
                    Дополнительные рекомендации, так как мы используем ADO.NET Entity Framework
                    он является узким местом для повышения производительности выполнения запросов.
                    Его можно заменить на Drapper (низко-уровневая ORM) с NPGSQL командой (COPY),
                    что даст нам высокий прирост к скорости.

                    Результаты замеров представлены в логе без индекса и с индексом соответственно.
                    Сравнения EF запросов с кешированием и без, а также запросы через NPGSQL 
                    командой (COPY).
                Выводы:
                    - NPGSQL командой (COPY) - работает лучше, что и ожидалось.
                    - Повторные запросы EF с кешированием выполняются быстрее.
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

            _logger.LogDebug("БД доступна для запросов!!!");

            return true;
        }
        public override string ToString()
        {
            return $"Задача: {this.GetType().Name} / Команда: {Command}";
        }
    }
}
