using TestTaskWithDB.Abstractions;
using Microsoft.Extensions.Logging;

namespace TestTaskWithDB.Services
{
    /// <summary>
    /// Реализация <see cref="IApplicationRunner">IApplicationRunner</see>
    /// <br/>Класс запуска приложения
    /// </summary>
    public class ApplicationRunner: IApplicationRunner
    {
        private readonly ILogger _logger;

        public ApplicationRunner(ILogger<ApplicationRunner> logger)
        {
            _logger = logger;
        }

        public void Run()
        {
            // метод запуска приложения
            _logger.LogInformation("Приложение запущено...");

            Console.ReadLine();
        }
    }
}
