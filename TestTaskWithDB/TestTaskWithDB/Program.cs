using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TestTaskWithDB.Abstractions;

namespace TestTaskWithDB
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Сборка конфигурации приложения
            var configuration = CreateConfigurationBuilder()
                                .Build();
            // Создаём класс сборки приложения
            var startup = new Startup(configuration);
            // Собираем сервис
            var serviceProvider = startup.ConfigureServices(new ServiceCollection(),args)
                                         .BuildServiceProvider();
            // Получаем сервис запуска приложения и запускаем его
            var runner = serviceProvider.GetRequiredService<IApplicationRunner>();
            runner.Run();
        }

        /// <summary>
        /// Метод сборки конфигурации приложения
        /// </summary>
        /// <returns>Конструктор конфигурации</returns>
        private static IConfigurationBuilder CreateConfigurationBuilder()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!)
                .AddJsonFile(path: "appsettings.json", // Название конфигурации 
                                optional: false,       // Не допустимо отсутствие файла -> error
                                reloadOnChange: false) // При изменении перезагружать конфигурацию не нужно
                .AddEnvironmentVariables();
        }
    }
}
