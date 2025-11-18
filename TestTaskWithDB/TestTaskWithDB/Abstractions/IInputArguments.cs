namespace TestTaskWithDB.Abstractions
{
    /// <summary>
    /// Интерфейс хранения входящих аргументов
    /// </summary>
    public interface IInputArguments
    {
        /// <summary>
        /// Входящие аргументы консоли
        /// </summary>
        string[] Args { get; init; }
    }
}