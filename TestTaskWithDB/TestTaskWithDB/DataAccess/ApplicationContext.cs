using Microsoft.EntityFrameworkCore;
using TestTaskWithDB.DataAccess.Entities;

namespace TestTaskWithDB.DataAccess
{
    /// <summary>
    /// Класс реализации контекста для нашей БД
    /// </summary>
    public class ApplicationContext : DbContext
    {
        /// <summary>
        /// Список сотрудников
        /// </summary>
        public DbSet<EmployeeEntity> Employees { get; set; } = null!;

        public ApplicationContext(DbContextOptions<ApplicationContext> options) 
            : base(options) { }
        /// <summary>
        /// Метод создания БД
        /// </summary>
        /// <returns>Результат содания</returns>
        public async Task<bool> CreateDB()
        {
            return await Database.EnsureCreatedAsync();
        }
        /// <summary>
        /// Метод удаления БД
        /// </summary>
        /// <returns>Результат удаления</returns>
        public async Task<bool> DeleteDB()
        {
            return await Database.EnsureDeletedAsync();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeEntity>();
        }
    }
}
