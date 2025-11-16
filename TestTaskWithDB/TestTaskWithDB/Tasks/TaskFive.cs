using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.Enums;

namespace TestTaskWithDB.Tasks
{
    /// <summary>
    /// Реализация <see cref="ICommandHandler">ITask</see>
    /// <para>
    /// Задача 5
    /// <br/><b>Задача</b>: Результат выборки из таблицы по критерию: пол мужской, Фамилия начинается с "F". 
    /// <br/>Сделать замер времени выполнения.
    /// </para>
    /// </summary>
    public class TaskFive : ICommandHandler
    {
        private readonly ILogger<TaskFive> _logger;
        private readonly IDBService _dBService;
        private readonly INpgsqlEmployeeService _npgsqlEmployeeService;
        private readonly IServiceProvider _serviceProvider;
        public string Command { get; set; } = "5";

        public TaskFive(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TaskFive>>();
            _dBService = serviceProvider.GetRequiredService<IDBService>();
            _npgsqlEmployeeService = serviceProvider.GetRequiredService<INpgsqlEmployeeService>();
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
            
            // СТАРТОВЫЕ ДАННЫЕ для поиска
            (string,Gender)[] SearchValues = 
                [
                    ("F",Gender.Male),
                    ("Af",Gender.Female),
                    ("A",Gender.Male)
                ];

            int countSearchValues = SearchValues.Length;
            
            // Создаём хранилища результатов замеров
            // для EF Core проводятся дополнительные два замера с кешированием
            (long, int)[] EF_Func = new (long, int)[countSearchValues * 3];
            (long, int)[] EF_LINQ = new (long, int)[countSearchValues * 3];
            (long, int)[] NPGSQL = new (long, int)[SearchValues.Length];

            _logger.LogInformation("Получение результатов заммеров запросов,\r\nможет занять продолжительное время ...");
            // Выпоняем замеры
            await ExecuteRequests(SearchValues, EF_Func, EF_LINQ, NPGSQL);

            // Выводим рзультаты замеров в таблицу
            PrintTable(SearchValues,EF_Func, EF_LINQ, NPGSQL);

            _logger.LogInformation("!!!!!Конец работы задачи!!!!!");

            return true;
        }

        /// <summary>
        /// Метод получения времени выполнения обращений к БД
        /// </summary>
        /// <param name="SearchValues">Данные для поиска</param>
        /// <param name="EF_Func">Замеры прямого запроса через EF</param>
        /// <param name="EF_LINQ">Замеры запроса через LINQ в EF</param>
        /// <param name="NPGSQL">Замеры прямого запроса через NPGSQL (COPY)</param>
        /// <returns></returns>
        private async Task ExecuteRequests((string, Gender)[] SearchValues,
                                     (long, int)[] EF_Func,
                                     (long, int)[] EF_LINQ,
                                     (long, int)[] NPGSQL)
        {
            Stopwatch stopwatch = new Stopwatch();
            var _employeeService = _serviceProvider.GetRequiredService<IEFEmployeeService>();
            int countSearchValues = SearchValues.Length;
            // Тестовый запрос для инициализации внутренних элементов
            var employees = await _employeeService.Get("A", Gender.Female, false) ;
            employees = null;
            GC.Collect();
            // Кэширования запросов нет
            _dBService.ClearCashe();
            for (int i = 0; i < countSearchValues; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                employees = await _employeeService.GetFunc(SearchValues[i].Item1,
                                                       SearchValues[i].Item2,
                                                       false);
                stopwatch.Stop();
                EF_Func[i] = (stopwatch.ElapsedMilliseconds, employees.Count);
                employees = null;
                GC.Collect();

                stopwatch.Reset();
                stopwatch.Start();
                employees = await _employeeService.Get(SearchValues[i].Item1,
                                                       SearchValues[i].Item2,
                                                       false);
                stopwatch.Stop();
                EF_LINQ[i] = (stopwatch.ElapsedMilliseconds, employees.Count);
                employees = null;
                GC.Collect();
            }
            _logger.LogDebug("Запросы без кеширования выполнены.");

            // Кеширование включено
            _dBService.ClearCashe();
            for (int i = 0; i < countSearchValues; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                employees = await _employeeService.Get(SearchValues[i].Item1,
                                                       SearchValues[i].Item2,
                                                       true);
                stopwatch.Stop();
                EF_LINQ[countSearchValues + i * 2] = (stopwatch.ElapsedMilliseconds, employees.Count);
                employees = null;
                GC.Collect();

                stopwatch.Reset();
                stopwatch.Start();
                employees = await _employeeService.Get(SearchValues[i].Item1,
                                                       SearchValues[i].Item2,
                                                       true);
                stopwatch.Stop();
                EF_LINQ[countSearchValues + i * 2 + 1] = (stopwatch.ElapsedMilliseconds, employees.Count);
                employees = null;
                GC.Collect();
            }
            _logger.LogDebug("Запросы c кешированием через конструктор EF Core выполнены.");

            // Кеширование включено
            _dBService.ClearCashe();
            for (int i = 0; i < countSearchValues; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                employees = await _employeeService.GetFunc(SearchValues[i].Item1,
                                                       SearchValues[i].Item2,
                                                       true);
                stopwatch.Stop();
                EF_Func[countSearchValues + i * 2] = (stopwatch.ElapsedMilliseconds, employees.Count);
                employees = null;
                GC.Collect();

                stopwatch.Reset();
                stopwatch.Start();
                employees = await _employeeService.GetFunc(SearchValues[i].Item1,
                                                       SearchValues[i].Item2,
                                                       true);
                stopwatch.Stop();
                EF_Func[countSearchValues + i * 2 + 1] = (stopwatch.ElapsedMilliseconds, employees.Count);
                employees = null;
                GC.Collect();
            }

            _logger.LogDebug("Запросы c кешированием через функцию БД при помощи EF Core выполнены.");

            // Замеры запросов через NPGSQL
            _npgsqlEmployeeService.ConnectionOpen();

            for (int i = 0; i < SearchValues.Length; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                employees = await _npgsqlEmployeeService.GetData(SearchValues[i].Item1,
                                                               SearchValues[i].Item2);
                stopwatch.Stop();
                NPGSQL[i] = (stopwatch.ElapsedMilliseconds, employees.Count);
                employees = null;
                GC.Collect();
            }

            _logger.LogDebug("Запросы при помощи NPGSQL выполнены.");
        }
        /// <summary>
        /// Методы вывода таблицы в лог
        /// </summary>
        /// <param name="SearchValues">Данные для поиска</param>
        /// <param name="EF_Func">Замеры прямого запроса через EF</param>
        /// <param name="EF_LINQ">Замеры запроса через LINQ в EF</param>
        /// <param name="NPGSQL">Замеры прямого запроса через NPGSQL (COPY)</param>
        private void PrintTable((string, Gender)[] SearchValues,
                                (long, int)[] EF_Func,
                                (long, int)[] EF_LINQ,
                                (long, int)[] NPGSQL)
        {
            // Создание шапки
            int countSearchValues = SearchValues.Length;
            string outputData = GetRowTable(["№",
                                             "PrefixName",
                                             "Gender",
                                             "Сache",
                                             "Result",
                                             "EF_Func",
                                             "EF_LINQ",
                                             "Npgsql"]);

            int lenData = outputData.Length - 2;
            outputData = new string('-', lenData) + "\r\n"
                        + outputData
                        + new string('-', lenData) + "\r\n";
            // Создание тела таблицы
            for (int i = 0; i < SearchValues.Length; i++)
            {
                outputData += GetRowTable([i+1,
                                           SearchValues[i].Item1,
                                           SearchValues[i].Item2,
                                           false,
                                           "Count:",
                                           EF_Func[i].Item2,
                                           EF_LINQ[i].Item2,
                                           NPGSQL[i].Item2]);

                outputData += GetRowTable(["","","","",
                                           "Time:",
                                           EF_Func[i].Item1.ToString() +"ms",
                                           EF_LINQ[i].Item1.ToString() +"ms",
                                           NPGSQL[i].Item1.ToString() +"ms"]);
                // Вывод кешированных запросов
                outputData += GetRowTable();

                outputData += GetRowTable(["","","",
                                           true,
                                           "Count:",
                                           EF_Func[countSearchValues+i*2].Item2,
                                           EF_LINQ[countSearchValues+i*2].Item2,
                                           " - "]);

                outputData += GetRowTable(["","","","",
                                           "Time:",
                                           EF_Func[countSearchValues+i*2].Item1.ToString() +"ms",
                                           EF_LINQ[countSearchValues+i*2].Item1.ToString() +"ms",
                                           " - "]);

                outputData += GetRowTable(["","","","",
                                           new string('-',10),
                                           new string('-',10),
                                           new string('-',10),
                                           new string('-',10)]);

                outputData += GetRowTable(["","","","",
                                           "Count:",
                                           EF_Func[countSearchValues+i*2+1].Item2,
                                           EF_LINQ[countSearchValues+i*2+1].Item2,
                                           " - "]);

                outputData += GetRowTable(["","","","",
                                           "Time:",
                                           EF_Func[countSearchValues+i*2+1].Item1.ToString() +"ms",
                                           EF_LINQ[countSearchValues+i*2+1].Item1.ToString() +"ms",
                                           " - "]);

                outputData += new string('-', lenData) + "\r\n";

            }

            _logger.LogInformation(outputData);
        }

        /// <summary>
        /// Метод вывода описания задачи в логгер
        /// </summary>
        private void PrintTextTask()
        {
            _logger.LogInformation(
                """
                Задача:
                    Результат выборки из таблицы по критерию: пол мужской, Фамилия начинается с "F". 
                    Сделать замер времени выполнения.
                Решение:
                    Так как мы используем ADO.NET Entity Framework, то будем получать значения через
                    LINQ(может кешироваться), FromSqlRaw(нет кеширования) и для сравнения через прямой 
                    доступ к бд и команды COPY, что даёт нам максимальную скорость загрузки данных в бд.
                    (реализация NpgsqlRepository -> GetData).
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
        /// <summary>
        /// Метод формирования строки таблицы
        /// </summary>
        /// <param name="strings">параметры строки</param>
        /// <returns></returns>
        private string GetRowTable(object[]? strings = null)
        {
            if(strings is null)
            {
                strings = ["","","",
                           new string('-',5),
                           new string('-',10),
                           new string('-',10),
                           new string('-',10),
                           new string('-',10)];
            }
            return string.Format("|{0,-3}|{1,-10}|{2,-8}|{3,5}|{4,10}|{5,-10}|{6,-10}|{7,-10}|\r\n",
                                 strings[0],
                                 strings[1],
                                 strings[2],
                                 strings[3],
                                 strings[4],
                                 strings[5],
                                 strings[6],
                                 strings[7]);
        }

        public override string ToString()
        {
            return $"Задача: {GetType().Name} / Команда: {Command}";
        }
    }
}
