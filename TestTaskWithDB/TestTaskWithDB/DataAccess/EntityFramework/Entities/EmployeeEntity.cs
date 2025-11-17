using TestTaskWithDB.Enums;

namespace TestTaskWithDB.DataAccess.EntityFramework.Entities
{
    /// <summary>
    /// Сущность для хранения сотрудника в БД
    /// </summary>
    public class EmployeeEntity
    {
        /// <summary>
        /// Индитефикатор сотрудника
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// ФИО
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateOnly DOB { get; set; }
        /// <summary>
        /// Пол сотрудника
        /// </summary>
        public Gender Gender { get; set; }
    }
}
