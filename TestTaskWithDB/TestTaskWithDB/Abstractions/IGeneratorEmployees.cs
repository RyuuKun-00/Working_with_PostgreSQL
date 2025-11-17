using TestTaskWithDB.Enums;
using TestTaskWithDB.Model;

namespace TestTaskWithDB.Abstractions
{
    /// <summary>
    /// Щаблон генерации сотрудников
    /// </summary>
    public interface IGeneratorEmployees
    {
        /// <summary>
        /// Метод для генерации сотрудников в заданном кол-ве
        /// </summary>
        /// <param name="count">Кол-во сотрудников для генерации</param>
        /// <returns>Список сгенерированных сотрудников</returns>
        List<Employee> Generate(int count);
        /// <summary>
        /// Метод генерации сотрудника с указанным префиксом ФИО и полом
        /// </summary>
        /// <param name="prefix">Начало ФИО</param>
        /// <param name="gender">Пол</param>
        /// <returns>Сорудник</returns>
        Employee GenerateEmployee(string? prefix = null, Gender? gender = null);
    }
}