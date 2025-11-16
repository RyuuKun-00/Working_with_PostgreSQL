using TestTaskWithDB.Abstractions;
using Microsoft.Extensions.Logging;
using TestTaskWithDB.Tasks;

namespace TestTaskWithDB.Services
{
    /// <summary>
    /// Реализация <see cref="IApplicationRunner">IApplicationRunner</see>
    /// <br/>Класс запуска приложения
    /// </summary>
    public class ApplicationRunner: IApplicationRunner
    {
        private readonly ILogger _logger;
        private readonly IInputArguments _arguments;
        private readonly ITaskManager _manager;
        private readonly IServiceProvider _serviceProvider;

        public ApplicationRunner(ILogger<ApplicationRunner> logger,
                                 IInputArguments arguments,
                                 ITaskManager manager,
                                 IServiceProvider serviceProvider)
        {
            _logger = logger;
            _arguments = arguments;
            _manager = manager;
            _serviceProvider = serviceProvider;
        }

        public void Run()
        {
            try
            {
                // метод запуска приложения
                _logger.LogInformation("Приложение запущено...");
                // добавление обработчика для команды 1
                _manager.AddCommandHandler(new TaskOne(_serviceProvider));
                // добавление обработчика для команды 2
                _manager.AddCommandHandler(new TaskTwo(_serviceProvider));
                // добавление обработчика для команды 3
                _manager.AddCommandHandler(new TaskThree(_serviceProvider));
                // добавление обработчика для команды 4
                _manager.AddCommandHandler(new TaskFour(_serviceProvider));
                // добавление обработчика для команды 5
                _manager.AddCommandHandler(new TaskFive(_serviceProvider));
                // добавление обработчика для команды 6
                _manager.AddCommandHandler(new TaskSix(_serviceProvider));
                // Ожидаем выполнения команды
                _manager.Execute(_arguments.Args).Wait();

                _logger.LogInformation("Нажмите Enter для завершения работы.");

                Console.ReadLine();

                _logger.LogInformation("Работа приложения завершена...");
            }catch(Exception ex)
            {
                _logger.LogCritical(ex, "Получена не предвиденная ошибка. Приложение завершает работу.");
            }
        }
    }
}
