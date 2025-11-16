using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Npgsql.Internal;
using NpgsqlTypes;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.Enums;
using TestTaskWithDB.Tasks.Services;
using TestTaskWithDB.Tasks.TaskModels;

namespace TestTaskWithDB.Tasks
{
    public class TaskFour : ICommandHandler,IAsyncDisposable
    {
        private readonly ILogger<TaskFour> _logger;
        private readonly IDBService _dBService;
        private readonly IServiceProvider _serviceProvider;
        private NpgsqlService? service;
        private string? sqlQuery;
        private object? look = new();

        public string Command { get; set; } = "4";

        public TaskFour(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TaskFour>>();
            _dBService = serviceProvider.GetRequiredService<IDBService>();
            _serviceProvider = serviceProvider;
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
            // Создаём сервис для подключения к бд
            service = new NpgsqlService(_serviceProvider);
            // Запрос для записи в БД через поток
            sqlQuery = "COPY public.\"Employees\"(\"Id\",\"FullName\",\"DOB\",\"Gender\") FROM STDIN BINARY";

            // Создаём сотрудников
            await CreatingEmployees(1000000);

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
                    Заполнение автоматически 1000000 строк справочника сотрудников. 
                    Распределение пола в них должно быть относительно равномерным, 
                    начальной буквы ФИО также. Добавить заполнение автоматически 
                    100 строк в которых пол мужской и Фамилия начинается с "F".
                    
                    У класса необходимо создать метод, который пакетно отправляет данные в БД, 
                    принимая массив объектов.
                
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
        /// Метод генерации записей сотрудников
        /// </summary>
        /// <param name="countEmployees">Кол-во генерируемых записей</param>
        /// <param name="exporter">Поток для записи</param>
        /// <returns></returns>
        public async Task CreatingEmployees(int countEmployees)
        {
            // Кол-во задач для генерации значений
            int countTask = 10;
            // Список задач
            List<Task> listTask = new();
            // Такса генерации одной задачи
            int countGenerate = countEmployees / countTask;
            // Создаём задачи
            for (int i = 0; i < countTask; i++)
            {
                // Если последняя задача, то должна забрать свою таксу и остаток если он есть
                if (i + 1 == countTask)
                {
                    countGenerate = countEmployees - countGenerate * i;
                }
                // Создаём задачу
                var task = StartTask(i.ToString(), countGenerate);
                listTask.Add(task);
            }

            // Ждём выполнение всех задач
            await Task.WhenAll(listTask);

            // Генерируем 100 сотрудников в которых пол мужской и Фамилия начинается с "F".
            List <CustomEmployee> employees = new();
            for (int i = 0; i < 100; i++)
            {
                employees.Add(GeneratorEmployees.GenerateEmployee("F", Gender.Male));
            }

            // Записываем в БД
            Write(employees);

            _logger.LogInformation("Созданы записи сотрудников в БД. В кол-ве: {countEmployees}", countEmployees);
        }
        /// <summary>
        /// Метод создания задачи, для записи сотрудников в БД
        /// </summary>
        /// <param name="nameTask">Имя задачи</param>
        /// <param name="count">Ко-во генерируемых сотрудников</param>
        private Task StartTask(string nameTask, int count)
        {
            // Создаём задачу
            return Task.Run(async () =>
            {
                int countEmployee = count;
                // Кол-во записываемых строк за генераци.
                int printLogInfo = 10000;
                while (countEmployee > 0)
                {
                    // Если осталось меньше чем printLogInfo
                    if (countEmployee < printLogInfo)
                    {
                        printLogInfo = countEmployee;
                    }
                    // Генерируем записи сотрудников
                    var list = GeneratorEmployees.Generate(printLogInfo);
                    // Записываем сотрудников в БД
                    await Write(list);
                    countEmployee -= printLogInfo;
                    _logger.LogInformation($"Задача {nameTask}: сгенерировано и отправлено {printLogInfo}.\r\nВсего готово: {count- countEmployee}/{count}.");
                    
                }
            });
        }

        private async Task<NpgsqlBinaryImporter> GetNpgsqlBinaryImportAsync()
        {
            // Получаем поток для записи
            return await service!.GetNpgsqlBinaryImportAsync(sqlQuery!);
        }

        /// <summary>
        /// Метод записи сотрудников в бд через поток
        /// </summary>
        /// <param name="list">Список сотрудников</param>
        private async Task Write(List<CustomEmployee> list)
        {
            lock (look!)
            {
                var writer =await GetNpgsqlBinaryImportAsync();

                foreach (var employee in list)
                {
                    writer!.StartRow();
                    writer!.Write<Guid>(employee.Id, NpgsqlDbType.Uuid);
                    writer!.Write<string>(employee.FullName, NpgsqlDbType.Text);
                    writer!.Write<DateOnly>(employee.DOB, NpgsqlDbType.Date);
                    writer!.Write((byte)employee.Gender, NpgsqlDbType.Smallint);
                }
            }
        }
        public override string ToString()
        {
            return $"Задача: {GetType().Name} / Команда: {Command}";
        }

        public ValueTask DisposeAsync()
        {
            if(writer is not null)
            {
                writer.DisposeAsync();
            }
            return ValueTask.CompletedTask;
        }
    }
}
