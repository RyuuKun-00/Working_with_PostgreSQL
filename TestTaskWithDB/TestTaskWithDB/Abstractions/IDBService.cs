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
        /// Метод проверки доступности и существования БД
        /// </summary>
        /// <returns>Результат проверки</returns>
        Task<bool> CheckDatabaseExistsAsync();
        /// <summary>
        /// Метод выполнения SQL запроса к бд
        /// </summary>
        /// <param name="sql">Запрос</param>
        /// <returns>Кол-во затронутых строк</returns>
        Task<int> ExecuteSql(string sql);
        /// <summary>
        /// Метод очистки текущего кеша EF
        /// </summary>
        void ClearCashe();
    }
}