using TestTaskWithDB.DataAccess.Entities.Enums;

namespace TestTaskWithDB.DataAccess.Entities
{
    public class EmployeeEntity
    {
        public int Id { get; set; }
        public string FullName { get; set; } = String.Empty;
        public DateOnly DOB { get; set; }
        public Gender Gender { get; set; }
}
}
