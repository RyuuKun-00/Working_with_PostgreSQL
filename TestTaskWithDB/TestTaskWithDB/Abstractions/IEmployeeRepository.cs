using TestTaskWithDB.Model;

namespace TestTaskWithDB.Abstractions
{
    /// <summary>
    /// Шаблон репозитория управлением сущностями "Сотрудник"
    /// </summary>
    public interface IEmployeeRepository
    {
        /// <summary>
        /// Метод добавления сотрудника в БД
        /// </summary>
        /// <param name="employee">Модель сотрудник</param>
        /// <returns>ID добавленного сотрудника</returns>
        Task<Guid> AddEmployee(Employee employee);
        /// <summary>
        /// Метод добавления сотрудников в БД
        /// </summary>
        /// <param name="employees">Список моделей сотрудников</param>
        /// <returns>Кол-во добавленных записей</returns>
        Task<int> AddEmployee(List<Employee> employees);
    }
}