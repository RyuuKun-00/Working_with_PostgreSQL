using TestTaskWithDB.Enums;
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

        /// <summary>
        /// Метод получения всех строк справочника сотрудников, 
        /// <br/>с уникальным значением ФИО+дата, отсортированным по ФИО
        /// </summary>
        /// <returns>Список моделй</returns>
        Task<List<Employee>> GetUniqueEmployees();
        /// <summary>
        /// Метод получения всех строк справочника сотрудников, 
        /// <br/>заданного пола и чьё полное имя начинается спереданных символов.
        /// <br/>Получение через LINQ
        /// </summary>
        /// <returns>Список моделй</returns>
        Task<List<Employee>> Get(string prefixFullName, Gender gender, bool asTracking);
        /// <summary>
        /// Метод получения всех строк справочника сотрудников, 
        /// <br/>заданного пола и чьё полное имя начинается спереданных символов.
        /// <br/>Получение через FromSqlRaw
        /// </summary>
        /// <returns>Список моделй</returns>
        Task<List<Employee>> GetFunc(string prefixFullName, Gender gender, bool asTracking);
    }
}