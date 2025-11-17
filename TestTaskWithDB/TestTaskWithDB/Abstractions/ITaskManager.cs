namespace TestTaskWithDB.Abstractions
{
    /// <summary>
    /// Шаблон обработки команд
    /// </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// Добавдение обработчика команды
        /// </summary>
        /// <param name="handler">Обработчик</param>
        void AddCommandHandler(ICommandHandler handler);
        /// <summary>
        /// Метод обрботки входных параметров
        /// </summary>
        /// <param name="args">Параметры</param>
        /// <returns>Результат выполнения</returns>
        Task<bool> Execute(string[] args);
    }
}