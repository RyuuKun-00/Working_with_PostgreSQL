using Microsoft.EntityFrameworkCore;

namespace TestTaskWithDB.DataAccess.EntityFramework.Extensions
{
    /// <summary>
    /// Класс расширения для LINQ EF Core
    /// Указывает на возможность кеширования данных
    /// </summary>
    public static class EFCoreTrackingExtension
    {
        /// <summary>
        /// Расширение LINQ EF Core, для установки кеширования запросов
        /// </summary>
        /// <typeparam name="T">Тип коллекции</typeparam>
        /// <param name="listEmployees">К каким данным будет применять кеширование</param>
        /// <param name="asTracking">Значение кеширования</param>
        /// <returns></returns>
        public static IQueryable<T> SetTracking<T>(this IQueryable<T> listEmployees, 
                                                   bool asTracking = false) 
                                                   where T : class
        {
            return asTracking ? listEmployees.AsTracking() : listEmployees.AsNoTracking();
        }
    }
}
