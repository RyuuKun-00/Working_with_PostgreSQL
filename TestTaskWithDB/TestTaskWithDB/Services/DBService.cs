using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;
using TestTaskWithDB.DataAccess;

namespace TestTaskWithDB.Services
{
    /// <summary>
    /// Реализация <see cref="IDBService">IDBService</see>
    /// <br/>Класс для управлением создания и удаления БД
    /// </summary>
    public class DBService : IDBService
    {
        private readonly ILogger<DBService> _logger;
        private readonly ApplicationContext _context;

        public DBService(ILogger<DBService> logger,
                         ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<bool> CreateDB()
        {
            var isExists = await CheckDatabaseExistsAsync();
            var result = String.Empty;
            var isCreated = true;

            if (isExists)
            {
                result = "База данных существует.";
            }
            else
            {
                isCreated = await _context.CreateDB();
                result = isCreated
                         ? "Создана база данных."
                         : "Не удалось создать базу данных.";
            }

            _logger.LogInformation(result);
            return isCreated;
        }

        public async Task<bool> DeleteDB()
        {
            var isExists = await CheckDatabaseExistsAsync();
            var result = String.Empty;
            var isDeleted = true;
            if (isExists)
            {
                result = "База данных не обнаружена.";
            }
            else
            {
                isDeleted = await _context.DeleteDB();
                result = isDeleted
                         ? "Удалена база данных."
                         : "Не удалось удалить базу данных.";
            }

            _logger.LogInformation(result);
            return isDeleted;
        }

        public async Task<bool> CheckDatabaseExistsAsync()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }

        }

        public void ClearCashe()
        {
            _context.ChangeTracker.Clear();
        }
    }
}
