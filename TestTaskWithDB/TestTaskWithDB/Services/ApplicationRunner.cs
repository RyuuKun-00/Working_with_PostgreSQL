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
            // метод запуска приложения
            _logger.LogInformation("Приложение запущено...");
            // добавление обработчика для команды 1
            _manager.AddCommandHandler(new TaskOne(_serviceProvider));

            _manager.Execute(_arguments.Args);

            Console.ReadLine();
        }
    }
}
