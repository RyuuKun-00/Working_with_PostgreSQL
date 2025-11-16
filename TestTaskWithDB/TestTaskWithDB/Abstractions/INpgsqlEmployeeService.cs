using TestTaskWithDB.Enums;
using TestTaskWithDB.Model;

namespace TestTaskWithDB.Abstractions
{
    /// <summary>
    /// Шаблон сервиса записи и получения данных из бд через NPGSQL
    /// </summary>
    public interface INpgsqlEmployeeService
    {
        /// <summary>
        /// Метод пакетной отправки сотрудников в БД
        /// </summary>
        /// <param name="employees">Список сотрудников</param>
        /// <param name="token">Токен отмены записи</param>
        /// /// <returns>Кол-во добавленных сотрудников</returns>
        Task<int> AddEmployees(List<Employee> employees, CancellationToken token = default);
        /// <summary>
        /// Метод получения сотрудников из бд
        /// </summary>
        /// <param name="prefixFullName">Префикс ФИО сотрудника для выборки</param>
        /// <param name="gender">Пол сотрудника для выборки</param>
        /// <param name="token">Токен отмены</param>
        /// <returns>Список сотрудников</returns>
        Task<List<Employee>> GetData(string prefixFullName, Gender gender, CancellationToken token = default);
        /// <summary>
        /// Метод открытия соединения к БД
        /// </summary>
        /// <returns>Результат открытия</returns>
        bool ConnectionOpen();
    }
}