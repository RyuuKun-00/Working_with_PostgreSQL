using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.DataAccess.EntityFramework;
using TestTaskWithDB.DataAccess.EntityFramework.Repositories;
using TestTaskWithDB.DataAccess.Npgsql;
using TestTaskWithDB.Model;
using TestTaskWithDB.Services;
using TestTaskWithDB.Tasks;
using TestTaskWithDB.Utilities;

namespace TestTaskWithDB
{
    /// <summary>
    /// Класс сборки DI контейнера
    /// </summary>
    public class Startup
    {
        private IConfiguration _configuration { get; init; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Метод установки зависимостей для приложения
        /// </summary>
        /// <param name="services">Коллекция сервисов для привязки</param>
        /// <param name="args">Параметры командной строки</param>
        /// <returns></returns>
        public IServiceCollection ConfigureServices( IServiceCollection services, string[] args)
        {
            // Стартовый сервис запуска приложения
            services.AddTransient<IApplicationRunner,ApplicationRunner>();
            // Добавление сервиса логгирования
            services.AddLogging(builder => builder.AddConfiguration(_configuration.GetSection("Logging")).AddConsole());
            // Добавление конфигурации в контейнер
            services.AddSingleton<IConfiguration>(_configuration);
            // Добавление и сохраниение входных параметров консоли
            services.AddSingleton<IInputArguments, InputArguments>(_ => new InputArguments(args));
            // Добавление строки подключения к бд в контейнер
            var strCon = GetStringConnection();
            services.AddDbContext<ApplicationContext>(o =>
            {
                o.UseNpgsql(strCon)
                 .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
            //==================ДОБАВЛЕНИЕ СЕРВИСОВ==================

            services.AddTransient<IDBRepository, DBRepository>();
            services.AddTransient<IDBService, DBService>();
            
            services.AddTransient<IEmployeeRepository, EmployeeRepository>();
            services.AddTransient<IEFEmployeeService, EFEmployeeService>();

            services.AddTransient<INpgsqlRepository, NpgsqlRepository>();
            services.AddTransient<INpgsqlEmployeeService, NpgsqlEmployeeService>();

            services.AddSingleton<ITaskManager, TaskManager>();
            services.AddSingleton<IGeneratorEmployees, GeneratorEmployees>();




            return services;
        }

        private string GetStringConnection()
        {
            // Получение строки подключения к бд
            return _configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException($"Connection string \"DefaultConnection\" not found.");
        }
    }
}
