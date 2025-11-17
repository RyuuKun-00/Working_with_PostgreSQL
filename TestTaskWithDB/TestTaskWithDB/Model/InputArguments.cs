using TestTaskWithDB.Abstractions;

namespace TestTaskWithDB.Model
{
    /// <summary>
    /// Реализация <see cref="IInputArguments">IInputArguments</see>
    /// <br/>Класс хранения входящих аргументов
    /// </summary>
    /// <param name="Args">Входящие аргументы</param>
    public record class InputArguments(string[] Args) : IInputArguments;
}
