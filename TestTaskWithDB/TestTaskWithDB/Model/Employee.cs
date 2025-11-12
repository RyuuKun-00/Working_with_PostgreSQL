using TestTaskWithDB.Enums;

namespace TestTaskWithDB.Model
{   
    /// <summary>
    /// Модель представляющая сотрудника
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; init; }
        public string FullName { get; init; }
        public DateOnly DOB { get; init; }
        public Gender Gender { get; init; }

        public Employee(Guid id, string fullname,DateOnly dob, Gender gender)
        {
            Id = id;
            FullName = fullname;
            DOB = dob;
            Gender = gender;
        }

        public int Age { 
        get
            {
                var now = DateOnly.FromDateTime(DateTime.Now);
                int age = now.Year - DOB.Year;
                // Корректируем возраст, если день рождения еще не наступил в этом году
                if (now.DayOfYear < DOB.DayOfYear)
                {
                    age--;
                }
                return age;
            } 
        }

        public override string ToString()
        {
            return 
                $"""
                Модель сотрудник.
                ID: {Id.ToString()}
                FullName: {FullName}
                Day of birth: {DOB.ToString("yyyy.MM.dd")}
                Gender: {Gender.ToString()}
                """;
        }
    }
}
