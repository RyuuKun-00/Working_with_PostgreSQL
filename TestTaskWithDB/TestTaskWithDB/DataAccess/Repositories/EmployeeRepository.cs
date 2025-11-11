

namespace TestTaskWithDB.DataAccess.Repositories
{
    public class EmployeeRepository
    {
        private readonly ApplicationContext _context;

        public EmployeeRepository(ApplicationContext context)
        {
            _context = context;
        }
    }
}
