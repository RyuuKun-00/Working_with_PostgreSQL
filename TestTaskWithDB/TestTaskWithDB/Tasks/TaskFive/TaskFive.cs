

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.DataAccess;
using TestTaskWithDB.Enums;
using TestTaskWithDB.Services;

namespace TestTaskWithDB.Tasks.TaskFive
{
    public class TaskFive : ICommandHandler
    {
        private readonly ILogger<TaskFive> _logger;
        private readonly ILogger<DatService> _loggerDS;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDBService _dBService;
        private readonly string strConnection;
        public string Command { get; set; } = "5";

        public TaskFive(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TaskFive>>();
            _loggerDS = serviceProvider.GetRequiredService<ILogger<DatService>>();
            _dBService = serviceProvider.GetRequiredService<IDBService>();
            _serviceProvider = serviceProvider;
            var context = serviceProvider.GetRequiredService<ApplicationContext>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            strConnection = context.Database.GetConnectionString()
                         ?? config.GetConnectionString("DefaultConnection")
                         ?? throw new InvalidOperationException($"Connection string \"DefaultConnection\" not found.");
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

            Stopwatch stopwatch = new Stopwatch();
            var _employeeService = _serviceProvider.GetRequiredService<IEmployeeService>();

            (string,Gender)[] SearchValues = 
                [
                    ("Af",Gender.Male),
                    ("F",Gender.Male),
                    ("A",Gender.Male)
                ];

            int countSearchValues = SearchValues.Length;

            
            (long, int)[] EF_Func = new (long, int)[countSearchValues * 3];
            (long, int)[] EF_LINQ = new (long, int)[countSearchValues * 3];
            // Кэширования запросов нет
            _dBService.ClearCashe();
            for (int i = 0; i < countSearchValues; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                var employees = await _employeeService.GetFunc(SearchValues[i].Item1,
                                                       SearchValues[i].Item2,
                                                       false);
                stopwatch.Stop();
                EF_Func[i] = (stopwatch.ElapsedMilliseconds, employees.Count);

                stopwatch.Reset();
                stopwatch.Start();
                employees = await _employeeService.Get(SearchValues[i].Item1,
                                                       SearchValues[i].Item2,
                                                       false);
                stopwatch.Stop();
                EF_LINQ[i] = (stopwatch.ElapsedMilliseconds, employees.Count);
            }

            // Кеширование включено
            _dBService.ClearCashe();
            for (int i = 0; i < countSearchValues; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                var employees = await _employeeService.Get(SearchValues[i].Item1,
                                                       SearchValues[i].Item2,
                                                       true);
                stopwatch.Stop();
                EF_LINQ[countSearchValues + i * 2] = (stopwatch.ElapsedMilliseconds, employees.Count);

                stopwatch.Reset();
                stopwatch.Start();
                employees = await _employeeService.Get(SearchValues[i].Item1,
                                                       SearchValues[i].Item2,
                                                       true);
                stopwatch.Stop();
                EF_LINQ[countSearchValues + i * 2 + 1] = (stopwatch.ElapsedMilliseconds, employees.Count);
            }

            // Кеширование включено
            _dBService.ClearCashe();
            for (int i = 0; i < countSearchValues; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                var employees = await _employeeService.GetFunc(SearchValues[i].Item1,
                                                       SearchValues[i].Item2,
                                                       true);
                stopwatch.Stop();
                EF_Func[countSearchValues+i * 2] = (stopwatch.ElapsedMilliseconds, employees.Count);

                stopwatch.Reset();
                stopwatch.Start();
                employees = await _employeeService.GetFunc(SearchValues[i].Item1,
                                                       SearchValues[i].Item2,
                                                       true);
                stopwatch.Stop();
                EF_Func[countSearchValues+i * 2+1] = (stopwatch.ElapsedMilliseconds, employees.Count);
            }



            var service = new DatService(strConnection, _loggerDS);

            (long, int)[] NPGSQL = new (long, int)[SearchValues.Length];
            for (int i = 0; i < SearchValues.Length; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                var employees = service.GetEmployees(SearchValues[i].Item1,
                                                     SearchValues[i].Item2);
                stopwatch.Stop();
                NPGSQL[i] = (stopwatch.ElapsedMilliseconds, employees.Count);
            }

            string outputData = GetRowTable(["№",
                                             "PrefixName",
                                             "Gender",
                                             "Сache",
                                             "Result",
                                             "EF_Func",
                                             "EF_LINQ", 
                                             "Npgsql"]);

            int lenData = outputData.Length-2;
            outputData =  new string('-', lenData) + "\r\n" 
                        + outputData 
                        + new string('-', lenData) + "\r\n";

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

                outputData += GetRowTable(["","","",
                                           true,
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
            return true;
        }

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
            return String.Format("|{0,-3}|{1,-10}|{2,-8}|{3,5}|{4,10}|{5,-10}|{6,-10}|{7,-10}|\r\n",
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
            return $"Задача: {this.GetType().Name} / Команда: {Command}";
        }
    }
}
