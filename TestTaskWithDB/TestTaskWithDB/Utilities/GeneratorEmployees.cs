using TestTaskWithDB.Abstractions;
using TestTaskWithDB.Enums;
using TestTaskWithDB.Model;

namespace TestTaskWithDB.Utilities
{
    /// <summary>
    /// Статический класс генерации сотрудников
    /// </summary>
    public class GeneratorEmployees : IGeneratorEmployees
    {
        public List<Employee> Generate(int count)
        {
            List<Employee> employees = new();
            for (int i = 0; i < count; i++)
            {
                employees.Add(GenerateEmployee());
            }
            return employees;
        }
        public Employee GenerateEmployee(string? prefix = null, Gender? gender = null)
        {
            Random random = new Random(DateTime.Now.Microsecond);
            // Генерируем пол если не задан
            Gender gend = gender ?? (Gender)(random.Next() % Enum.GetValues(typeof(Gender)).Length);
            // Генерируем имя с префиксом если задано
            string name = GenerateFullName(prefix);
            int rnd = random.Next();
            // Генериуем дату рождения
            DateOnly date = new DateOnly(1920 + rnd % 105, 1 + rnd % 12, 1 + rnd % 30);

            return new Employee(Guid.NewGuid(), name, date, gend);
        }

        /// <summary>
        /// Метод генерации ФИО
        /// </summary>
        /// <param name="prefixName">Префикс ФИО</param>
        /// <returns></returns>
        private string GenerateFullName(string? prefixName)
        {
            // Общая длина ФИО
            int lenFIO = 30;
            char[] fullName = new char[lenFIO];
            var prefix = prefixName?.ToCharArray() ?? [];
            Random random = new Random(DateTime.Now.Microsecond);
            // генерируем ФИО из английских символов нижнего регистра
            for (int i = 0; i < lenFIO; i++)
            {
                if (i < prefix.Length)
                {
                    fullName[i] = prefix[i];
                }
                else
                {
                    fullName[i] = (char)(97 + random.Next() % 26);
                }
            }
            return new string(fullName);
        }
    }
}
