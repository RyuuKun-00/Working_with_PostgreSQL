

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.DataAccess;

namespace TestTaskWithDB.Tasks.TaskFour
{
    public class TaskFour : ICommandHandler
    {
        private readonly ILogger<TaskFour> _logger;
        private readonly ILogger<DataService> _loggerDS;
        private readonly IDBService _dBService;
        private readonly string strConnection;
        private readonly IServiceProvider _serviceProvider;

        public string Command { get; set; } = "4";

        public TaskFour(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TaskFour>>();
            _loggerDS = serviceProvider.GetRequiredService<ILogger<DataService>>();
            _dBService = serviceProvider.GetRequiredService<IDBService>();
            var context = serviceProvider.GetRequiredService<ApplicationContext>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            strConnection = context.Database.GetConnectionString()
                         ?? config.GetConnectionString("DefaultConnection") 
                         ?? throw new InvalidOperationException($"Connection string \"DefaultConnection\" not found.");
            _serviceProvider = serviceProvider;
        }

        public async Task<bool> Invoke(string[] args)
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

            var service = new DataService(strConnection, _loggerDS);
            await service.CreatingEmployees();

            return true;
        }

        public override string ToString()
        {
            return "Задача№4 " + GetType().Name;
        }
    }
}
