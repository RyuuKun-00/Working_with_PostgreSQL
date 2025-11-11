namespace TestTaskWithDB.Abstractions
{
    /// <summary>
    /// Шаблон управлением бд (создание и удаление)
    /// </summary>
    public interface IDBService
    {
        /// <summary>
        /// Метод создания БД
        /// <br/><b>Примечание:</b> при наличии уже существующей БД,
        /// вернётся: <b>true</b>
        /// </summary>
        Task<bool> CreateDB();
        /// <summary>
        /// Метод удаления БД
        /// <br/><b>Примечание:</b> при отсутствии БД,
        /// вернётся: <b>true</b>
        /// </summary>
        Task<bool> DeleteDB();
        /// <summary>
        /// Метод проверки на существования БД
        /// </summary>
        Task<bool> CheckDatabaseExistsAsync();
    }
}