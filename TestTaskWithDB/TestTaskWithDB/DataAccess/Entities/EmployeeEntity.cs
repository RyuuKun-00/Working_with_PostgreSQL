using TestTaskWithDB.Enums;

namespace TestTaskWithDB.DataAccess.Entities
{
    /// <summary>
    /// Сущность для хранения сотрудника в БД
    /// </summary>
    public class EmployeeEntity
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = String.Empty;
        public DateOnly DOB { get; set; }
        public Gender Gender { get; set; }
    }
}
