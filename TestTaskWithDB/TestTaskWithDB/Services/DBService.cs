using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;

namespace TestTaskWithDB.Services
{
    /// <summary>
    /// Реализация <see cref="IDBService">IDBService</see>
    /// <br/>Класс для управлением создания и удаления БД
    /// </summary>
    public class DBService : IDBService
    {
        private readonly ILogger<DBService> _logger;
        private readonly IDBRepository _dBRepository;

        public DBService(ILogger<DBService> logger,
                         IDBRepository dBRepository)
        {
            _logger = logger;
            _dBRepository = dBRepository;
        }

        public async Task<bool> CreateDB()
        {
            var isExists = await _dBRepository.CheckDatabaseExistsAsync();
            var result = String.Empty;
            var isCreated = true;

            if (isExists)
            {
                result = "База данных существует.";
            }
            else
            {
                isCreated = await _dBRepository.CreateDB();
                result = isCreated
                         ? "Создана база данных."
                         : "Не удалось создать базу данных.";
            }

            _logger.LogInformation(result);
            return isCreated;
        }

        public async Task<bool> DeleteDB()
        {
            var isExists = await _dBRepository.CheckDatabaseExistsAsync();
            var result = String.Empty;
            var isDeleted = true;
            if (isExists)
            {
                result = "База данных не обнаружена.";
            }
            else
            {
                isDeleted = await _dBRepository.DeleteDB();
                result = isDeleted
                         ? "Удалена база данных."
                         : "Не удалось удалить базу данных.";
            }

            _logger.LogInformation(result);
            return isDeleted;
        }

        public async Task<int> ExecuteSql(string sql)
        {
            return await _dBRepository.ExecuteSql(sql);
        }
        public async Task<bool> CheckDatabaseExistsAsync()
        {
            return await _dBRepository.CheckDatabaseExistsAsync();
        }

        public void ClearCashe()
        {
            _dBRepository.ClearCashe();
        }
    }
}
