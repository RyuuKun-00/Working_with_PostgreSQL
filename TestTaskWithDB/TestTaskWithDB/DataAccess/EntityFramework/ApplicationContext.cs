using Microsoft.EntityFrameworkCore;
using TestTaskWithDB.DataAccess.EntityFramework.Entities;

namespace TestTaskWithDB.DataAccess.EntityFramework
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
    }
}
