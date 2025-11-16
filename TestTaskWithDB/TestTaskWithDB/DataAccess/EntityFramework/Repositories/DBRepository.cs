using Microsoft.EntityFrameworkCore;
using TestTaskWithDB.Abstractions;

namespace TestTaskWithDB.DataAccess.EntityFramework.Repositories
{
    /// <summary>
    /// Реализация <see cref="IDBRepository">IDBRepository</see>
    /// <br/>Класс управления контекстом EF Core
    /// </summary>
    public class DBRepository : IDBRepository
    {
        private readonly ApplicationContext _context;

        public DBRepository(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Метод создания БД
        /// </summary>
        /// <returns>Результат содания</returns>
        public async Task<bool> CreateDB()
        {
            var isCreated = await _context.Database.EnsureCreatedAsync();
            if (!isCreated)
            {
                return false;
            }
            return true;
        }

        public async Task<int> ExecuteSql(string sql)
        {
            return await _context.Database.ExecuteSqlRawAsync(sql);
        }
        public async Task<bool> DeleteDB()
        {
            return await _context.Database.EnsureDeletedAsync();
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
