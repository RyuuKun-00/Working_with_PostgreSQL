
namespace TestTaskWithDB.Abstractions
{
    /// <summary>
    /// Интрерфейс определяющий точку запуска приложения
    /// </summary>
    public interface IApplicationRunner
    {
        /// <summary>
        /// Метод запуска приложения
        /// </summary>
        void Run();
    }
}
