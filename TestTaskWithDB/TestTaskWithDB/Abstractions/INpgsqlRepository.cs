using TestTaskWithDB.Model;

/// <summary>
/// Шаблон репозитория записи и получения данных из бд через NPGSQL
/// </summary>
public interface INpgsqlRepository
{
    /// <summary>
    /// Метод высвобождения данных
    /// </summary>
    void Dispose();
    /// <summary>
    /// Метод пакетной отправки сотрудников в БД
    /// </summary>
    /// <param name="batch">Пакет сотрудников</param>
    /// <param name="sqlCommand">Команда для записи в бд</param>
    /// <param name="token">Токен отмены записи</param>
    /// <returns>Кол-во добавленных сотрудников</returns>
    Task<int> InsertBatch(List<Employee> batch, string sqlCommand, CancellationToken token = default);
    /// <summary>
    /// Метод получения сотрудников из бд
    /// </summary>
    /// <param name="sqlCommand">Команда для получения сотрудников</param>
    /// <param name="token">Токен отмены</param>
    /// <returns>Список сотрудников</returns>
    Task<List<Employee>> GetData(string sqlCommand, CancellationToken token = default);
    /// <summary>
    /// Метод открытия соединения к БД
    /// </summary>
    /// <returns>Результат открытия</returns>
    bool ConnectionOpen();
}