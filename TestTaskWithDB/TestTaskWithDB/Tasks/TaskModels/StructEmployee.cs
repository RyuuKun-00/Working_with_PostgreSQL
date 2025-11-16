using TestTaskWithDB.Enums;

namespace TestTaskWithDB.Tasks.TaskModels
{
    public struct CustomEmployee
    {
        public Guid Id;
        public string FullName;
        public DateOnly DOB;
        public Gender Gender;
    }
}
