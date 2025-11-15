using Microsoft.EntityFrameworkCore;
using TestTaskWithDB.DataAccess.Entities;
using TestTaskWithDB.Model;

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
            var isCreated = await Database.EnsureCreatedAsync();
            if (!isCreated)
            {
                return false;
            }

            // Добавление встроенной функции в бд для выпонения 2 задания
            // Получения уникальных сотрудников
            await Database.ExecuteSqlRawAsync(
                """
                CREATE FUNCTION GetEmployees()
                RETURNS TABLE
                (
                	"Id" uuid, 
                	"FullName" text,
                	"DOB" date,
                	"Gender" smallint
                	)
                AS $$
                BEGIN
                  RETURN QUERY
                    SELECT
                	    "Employees".*
                    FROM "Employees",
                    (
                	    SELECT "Employees"."FullName",
                			    "Employees"."DOB",
                			    COUNT(*) as "Count"
                	    FROM "Employees"
                	    GROUP BY "Employees"."FullName",
                			     "Employees"."DOB"
                	    HAVING COUNT(*) = 1
                	    ORDER BY "Employees"."FullName" ASC
                    ) AS "UniqueEmployees"
                    WHERE "Employees"."FullName" = "UniqueEmployees"."FullName"
                	    AND "Employees"."DOB" = "UniqueEmployees"."DOB";
                END;
                $$ LANGUAGE plpgsql;
                """);
            return true;
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
