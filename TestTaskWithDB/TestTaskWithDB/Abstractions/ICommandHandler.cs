namespace TestTaskWithDB.Abstractions
{
    /// <summary>
    /// Шаблон обработчика команды
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// Метод вызова выполнения задачи
        /// </summary>
        /// <param name="args">Аргуметы задачи</param>
        /// <returns>результат выполнения задачи</returns>
        Task<bool> Invoke(string[] args);
        /// <summary>
        /// Определение команды
        /// </summary>
        string Command { get; }
    }
}