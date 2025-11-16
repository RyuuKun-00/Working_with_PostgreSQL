using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using TestTaskWithDB.DataAccess;

namespace TestTaskWithDB.Tasks.Services
{
    /// <summary>
    /// Класс создания подключения к бд
    /// </summary>
    public class NpgsqlService : IAsyncDisposable
    {
        private readonly ILogger<NpgsqlService> _logger;
        private string strConnection { get; set; }
        private NpgsqlConnection? connection;
        private object? look = new();

        public NpgsqlService(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<NpgsqlService>>();
            var context = serviceProvider.GetRequiredService<ApplicationContext>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            strConnection = context.Database.GetConnectionString()
                         ?? config.GetConnectionString("DefaultConnection")
                         ?? throw new InvalidOperationException($"Строка подключения \"DefaultConnection\" не обнаружена.");
        }

        /// <summary>
        /// Метод создания подключения к БД
        /// </summary>
        /// <param name="stringConnection"></param>
        /// <exception cref="ArgumentException"></exception>
        public void CreateConnection(string stringConnection)
        {
            connection = new NpgsqlConnection(stringConnection);
            if (connection is null)
            {
                throw new ArgumentException($"Не удаётся создать подключиться по указанной строке:\r\n{strConnection}");
            }
            strConnection = stringConnection;
            _logger.LogDebug("Создано подключение: {strConnection}", strConnection);
        }
        /// <summary>
        /// Метод открытия подключения
        /// </summary>
        /// <returns></returns>
        public Task OpenConnectionAsyns()
        {
            if (connection is null)
            {
                CreateConnection(strConnection);
            }
            if (connection!.FullState == System.Data.ConnectionState.Open)
            {
                return Task.CompletedTask;
            }

            var task = connection!.OpenAsync();

            _logger.LogDebug("Открыто подключение: {strConnection}", strConnection);

            return task;
        }
        /// <summary>
        /// Метод закрытия подключения
        /// </summary>
        /// <returns></returns>
        public Task CloseConnectionAsyns()
        {
            if (connection is null)
            {
                return Task.CompletedTask;
            }

            var task = connection!.CloseAsync();

            _logger.LogDebug("Закрыто подключение: {strConnection}", strConnection);

            return task;
        }
        /// <summary>
        /// Метод получения выгрузчика данных по запросу
        /// </summary>
        /// <param name="sqlQuery">Запрос выгрузки</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Генерируется исключение если не удалось создать выгрузчик данных по запросу</exception>
        public async Task<NpgsqlBinaryExporter> GetNpgsqlBinaryExportAsync(string sqlQuery)
        {
            if (connection is null)
            {
                await OpenConnectionAsyns();
            }

            var npgsql = connection!.BeginBinaryExport(sqlQuery);

            if(npgsql == null)
            {
                throw new ArgumentException($"Не удалось создать загрузчик данных в бд.\r\nДля запроса: {sqlQuery}");
            }

            return npgsql;
        }
        /// <summary>
        /// Метод получения загрузчика в БД по зарпросу
        /// </summary>
        /// <param name="sqlQuery">Запрос загрузки</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Генерируется исключение если не удалось создать загрузчик данных по запросу</exception>
        public async Task<NpgsqlBinaryImporter> GetNpgsqlBinaryImportAsync(string sqlQuery)
        {
            if (connection is null)
            {
                await OpenConnectionAsyns();
            }

            var npgsql = connection!.BeginBinaryImport(sqlQuery);

            if (npgsql == null)
            {
                throw new ArgumentException($"Не удалось создать выгрузчик данных из бд.\r\nДля запроса: {sqlQuery}");
            }

            return npgsql;
        }

        public ValueTask DisposeAsync()
        {

            if (connection is not null)
            {
                connection.CloseAsync();
                connection.DisposeAsync();
            }

            return ValueTask.CompletedTask;
        }
    }
}
