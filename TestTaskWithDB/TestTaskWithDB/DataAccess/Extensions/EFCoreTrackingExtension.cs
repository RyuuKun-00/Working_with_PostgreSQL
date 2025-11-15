using Microsoft.EntityFrameworkCore;

namespace TestTaskWithDB.DataAccess.Extensions
{
    public static class EFCoreTrackingExtension
    {
        public static IQueryable<T> SetTracking<T>(this IQueryable<T> listEmployees, 
                                                   bool asTracking = false) 
                                                   where T : class
        {
            return asTracking ? listEmployees.AsTracking<T>() : listEmployees.AsNoTracking<T>();
        }
    }
}
