using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using TestTaskWithDB.Enums;
using TestTaskWithDB.Model;

namespace TestTaskWithDB.DataAccess.Npgsql
{
    /// <summary>
    /// Реализация <see cref="INpgsqlRepository">INpgsqlRepository</see>
    /// <br/>Репозиторий управлнием сущностями <see cref="Employee">Employee</see> (Сотрудниками)
    /// </summary>
    public class NpgsqlRepository : IDisposable, INpgsqlRepository
    {
        private NpgsqlConnection? _connection;
        private readonly string connectionString;
        private volatile bool _isDisposed;
        private readonly object _syncRoot = new object();

        public NpgsqlRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection") ??
                              throw new InvalidOperationException($"Connection string \"DefaultConnection\" not found.");
        }

        public bool ConnectionOpen()
        {
            _connection = new NpgsqlConnection(connectionString);
            if (_connection is null)
            {
                return false;
            }
            _connection.Open();
            return true;
        }
        public async Task<int> InsertBatch(List<Employee> batch, string sqlCommand, CancellationToken token = default)
        {
            // Проверка на наличие подключения к БД
            if (_connection is null || _connection.FullState == ConnectionState.Closed)
            {
                throw new NullReferenceException("Перед обращением к БД, нужно открыть подключение методом ConnectionOpen.");
            }
            // Открытие транзакции
            using var transaction = await _connection.BeginTransactionAsync(token);
            int count = 0;
            try
            {
                // Открытие потока записи
                using var writer = await _connection.BeginBinaryImportAsync(sqlCommand, token);

                foreach (var employee in batch)
                {
                    count++;
                    writer.StartRow();
                    writer.Write(employee.Id, GetNpgsqlDbType(employee.Id));
                    writer.Write(employee.FullName, GetNpgsqlDbType(employee.FullName));
                    writer.Write(employee.DOB, GetNpgsqlDbType(employee.DOB));
                    writer.Write((byte)employee.Gender, GetNpgsqlDbType(employee.Gender));
                }
                await writer.CompleteAsync(token);
                await writer.CloseAsync(token);
                await transaction.CommitAsync(token);
                return count;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(token);
                Console.WriteLine($"Ошибка при выполнении транзакции: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<Employee>> GetData(string sqlCommand, CancellationToken token = default)
        {
            // Проверка на наличие подключения к БД
            if (_connection is null || _connection.FullState == ConnectionState.Closed)
            {
                throw new NullReferenceException("Перед обращением к БД, нужно открыть подключение методом ConnectionOpen.");
            }

            try
            {
                // Открытие потока считывания
                using var reader = await _connection.BeginBinaryExportAsync(sqlCommand, token);
                var list = new List<Employee>();
                while (reader.StartRow() != -1)
                {
                    list.Add(new Employee(
                        reader.Read<Guid>(NpgsqlDbType.Uuid),
                        reader.Read<string>(NpgsqlDbType.Text),
                        reader.Read<DateOnly>(NpgsqlDbType.Date),
                        (Gender)(reader.Read<byte>(NpgsqlDbType.Smallint))
                        ));
                }
                await reader.CancelAsync();
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении запроса к БД: {ex.Message}");
                return new();
            }
        }
        /// <summary>
        /// Метод конвертации типа приложения в тип БД
        /// </summary>
        /// <param name="value">Тип приложения</param>
        /// <returns></returns>
        private static NpgsqlDbType GetNpgsqlDbType(object value)
        {
            return value switch
            {
                Guid => NpgsqlDbType.Uuid,
                string => NpgsqlDbType.Text,
                DateOnly => NpgsqlDbType.Date,
                byte => NpgsqlDbType.Smallint,
                Gender => NpgsqlDbType.Smallint,
                _ => NpgsqlDbType.Text
            };
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            lock (_syncRoot)
            {
                if (_isDisposed) return;

                _isDisposed = true;

                _connection?.Close();
                _connection?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}