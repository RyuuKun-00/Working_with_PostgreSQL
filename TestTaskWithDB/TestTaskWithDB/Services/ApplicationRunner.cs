using TestTaskWithDB.Abstractions;
using Microsoft.Extensions.Logging;
using TestTaskWithDB.DataAccess;
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
        private readonly TaskOne _taskOne;

        public ApplicationRunner(ILogger<ApplicationRunner> logger,
                                 IInputArguments arguments,
                                 ITaskManager manager,
                                 TaskOne taskOne)
        {
            _logger = logger;
            _arguments = arguments;
            _manager = manager;
            _taskOne = taskOne;
        }

        public void Run()
        {
            // метод запуска приложения
            _logger.LogInformation("Приложение запущено...");

            _manager.AddCommandHandler(_taskOne);

            _manager.Execute(_arguments.Args);

            Console.ReadLine();
        }
    }
}
