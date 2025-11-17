namespace TestTaskWithDB.Abstractions
{
    /// <summary>
    /// Шаблон управлением БД через EF Core
    /// </summary>
    public interface IDBRepository
    {
        /// <summary>
        /// Метод создания БД
        /// </summary>
        /// <returns>Результат создания</returns>
        Task<bool> CreateDB();
        /// <summary>
        /// Метод удаления  БД
        /// </summary>
        /// <returns>Результат удланения БД</returns>
        Task<bool> DeleteDB();
        /// <summary>
        /// Метод выполнения SQL запроса к бд
        /// </summary>
        /// <param name="sql">Запрос</param>
        /// <returns>Кол-во затронутых строк</returns>
        Task<int> ExecuteSql(string sql);
        /// <summary>
        /// Метод проверки доступности и существования БД
        /// </summary>
        /// <returns>Результат проверки</returns>
        Task<bool> CheckDatabaseExistsAsync();
        /// <summary>
        /// Метод очистки текущего кеша EF
        /// </summary>
        void ClearCashe();
    }
}