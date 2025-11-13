using TestTaskWithDB.Model;

namespace TestTaskWithDB.Abstractions
{
    /// <summary>
    /// Шаблон сервиса управлением моделями "Сотрудник"
    /// </summary>
    public interface IEmployeeService
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
        /// <summary>
        /// Метод получения всех строк справочника сотрудников, 
        /// <br/>с уникальным значением ФИО+дата, отсортированным по ФИО
        /// </summary>
        /// <returns>Список моделй</returns>
        Task<List<Employee>> GetUniqueEmployees();
    }
}