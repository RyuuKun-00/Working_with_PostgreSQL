using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.Enums;
using TestTaskWithDB.Model;

namespace TestTaskWithDB.Tasks
{
    /// <summary>
    /// Реализация <see cref="ICommandHandler">ITask</see>
    /// <para>
    /// Задача 4
    /// <br/><b>Задача</b>: Заполнение автоматически 1000000 строк справочника сотрудников. 
    /// <br/>Распределение пола в них должно быть относительно равномерным,
    /// <br/>начальной буквы ФИО также.Добавить заполнение автоматически 
    /// <br/>100 строк в которых пол мужской и Фамилия начинается с "F".
    /// <br/>У класса необходимо создать метод, который пакетно отправляет данные в БД,
    /// <br/>принимая массив объектов.
    /// </para>
    /// </summary>
    public class TaskFour : ICommandHandler
    {
        private readonly ILogger<TaskFour> _logger;
        private readonly IDBService _dBService;
        private readonly INpgsqlEmployeeService _employeeService;
        private readonly IGeneratorEmployees _generatorEmployees;
        // Размер пакета для отправки
        private int batchSize = 10000;

        public string Command { get; set; } = "4";

        public TaskFour(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TaskFour>>();
            _dBService = serviceProvider.GetRequiredService<IDBService>();
            _employeeService = serviceProvider.GetRequiredService<INpgsqlEmployeeService>();
            _generatorEmployees = serviceProvider.GetRequiredService<IGeneratorEmployees>();
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
            _logger.LogInformation("Добавление сотрудников может занять некоторое время");
            // Сколько элементов нужно создать
            int countEmployees = 1000000;
            // Создаём записи сотрудников
            var count = await CreatedEmployee(countEmployees);

            _logger.LogInformation("В БД добавлены сотрудники в кол-ве: {count}/{countEmployees}", count, countEmployees);

            // Создаём записи 100 сорудников в которых пол мужской и Фамилия начинается с "F".
            count = await CreatedEmployee(100,"F",Gender.Male);

            _logger.LogInformation("В БД добавлены сотрудники в которых пол мужской и Фамилия начинается с \"F\".\r\n"+
                                   "В кол-ве: {count}/{countEmployees}", count, 100);

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
                    Заполнение автоматически 1000000 строк справочника сотрудников. 
                    Распределение пола в них должно быть относительно равномерным, 
                    начальной буквы ФИО также. Добавить заполнение автоматически 
                    100 строк в которых пол мужской и Фамилия начинается с "F".
                    У класса необходимо создать метод, который пакетно отправляет данные в БД, 
                    принимая массив объектов.
                Решение:
                    Так как мы используем ADO.NET Entity Framework, то в нём есть пакетная отправка
                    группы сотрудников через метод AddRangeAsync() и вызова SaveChangesAsync().

                    Также реализована пакетная отправка сотрудников, через прямой доступ к бд и
                    команды COPY, что даёт нам максимальную скорость загрузки данных в бд.
                    (реализация NpgsqlRepository -> InsertBatch).
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
        /// Метод создания и добавления сгенерированных сотрудников в БД
        /// </summary>
        /// <param name="countEmployees">Кол-во сотрудников для генерации</param>
        /// <param name="prefix">Префикс фамилии сотрудника</param>
        /// <param name="gender">Пол сотрудника</param>
        /// <returns>Кол-во добавленных сотрудников в БД</returns>
        public async Task<int> CreatedEmployee(int countEmployees,
                                    string? prefix=null,
                                    Gender? gender=null)
        {
            // Создаём конкурентную очередь
            var queue = new ConcurrentQueue<Employee>();
            // Создаём ресурсы для токена отмены
            var cancellationTokenSource = new CancellationTokenSource();
            // Создание контролируеммой задачи для ожидания
            var tcs = new TaskCompletionSource<int>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            // Получение(создание) рабочей очереди
            var workerTask = Task.Run(() => ProcessQueue(queue,
                                                      countEmployees, 
                                                      tcs,
                                                      cancellationTokenSource.Token));
            // Открываем подключение к БД
            _employeeService.ConnectionOpen();
            // Паралельная генерация и добавление сотрудников в очередь
            Parallel.For(0, countEmployees, (i) =>
            {
                var employee = _generatorEmployees.GenerateEmployee(prefix,gender);

                queue.Enqueue(employee);
            });
            // Ожидаем добавление всех сотрудников в БД
            var numberAdded = await tcs.Task;
            // Завершаем работу очереди
            cancellationTokenSource.Cancel();

            try
            {
                workerTask.Wait(TimeSpan.FromSeconds(5));
            }
            catch (AggregateException ex)
            {
                _logger.LogWarning($"Ошибка завершения обработки очереди: {ex.Flatten().Message}");
            }

            cancellationTokenSource.Dispose();

            return numberAdded;

        }
        /// <summary>
        /// Метод запуска пакетной отправки в данных в БД
        /// </summary>
        /// <param name="employees">Очередь для отслеживания сгенерированных сотрудников</param>
        /// <param name="count">Кол-во сотрудников для генерации</param>
        /// <param name="tcs">Задача для отслеживания текущей задачи</param>
        /// <param name="token">Токен отмены задачи</param>
        /// <returns></returns>
        private async Task ProcessQueue(ConcurrentQueue<Employee> employees, 
                                        int count, 
                                        TaskCompletionSource<int> tcs,
                                        CancellationToken token)
        {
            // Содаём объект для отправки в бд
            var data = new List<Employee>(batchSize);
            int countEmployees = 0;
            int numberAdded = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Собираем данные из очереди
                    while (data.Count < batchSize && employees!.TryDequeue(out var item))
                    {
                        data.Add(item);
                    }
                    countEmployees += data.Count;
                    // Отправляем данные в БД
                    if (data.Count > 0)
                    {
                        var numAdd = await _employeeService.AddEmployees(data, token);
                        numberAdded += numAdd;
                        _logger.LogDebug(
                            $"""
                            В БД добавлено сотрудники в кол-ве: {numAdd}
                            Всего обработано: {countEmployees}/{count}
                            Всего добавлено: {numberAdded}/{count}
                            """);
                        data.Clear();
                    }
                    else
                    {
                        await Task.Delay(100, token);
                    }
                    // Когда обработано сколько запланировано завершаем задачу
                    if (countEmployees >= count)
                    {
                        tcs.SetResult(numberAdded);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("Событие: отмена беспрерывной пакетной отправки данных в БД.");
                    if (countEmployees < count)
                    {
                        tcs.SetResult(numberAdded);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    // Логирование ошибки
                    _logger.LogError(ex,"Ошибка беспрерывной пакетной отправки данных в БД.");
                    throw;
                }
            }
            _logger.LogDebug("Очередь завершила работу.");
        }

    }
}
