

using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using TestTaskWithDB.Enums;
using TestTaskWithDB.Model;

namespace TestTaskWithDB.Tasks.TaskFive
{
    public class DatService:IAsyncDisposable
    {
        private readonly ILogger<DatService> _logger;
        private string strConnect;
        private NpgsqlConnection? connection;
        private NpgsqlBinaryExporter? reader;
        private object? look = new();

        public DatService(string strConnect, ILogger<DatService> logger)
        { 
            this.strConnect = strConnect;
            _logger = logger;

        }



        public List<Employee> GetEmployees(string prefixFullName, Gender gender)
        {
            connection = new NpgsqlConnection(strConnect);
            connection.Open();
            List<Employee> list = new();
            reader = connection.BeginBinaryExport(
                $"""
                COPY (SELECT "Id","FullName","DOB","Gender" 
                      FROM public."Employees"
                      WHERE "Gender" = {(byte)gender}
                        AND "FullName" LIKE '{prefixFullName}%') 
                TO STDIN BINARY
                
                """);

            while (reader.StartRow() != -1)
            {
                list.Add(new Employee(
                        reader.Read<Guid>(NpgsqlDbType.Uuid),
                        reader.Read<string>(NpgsqlDbType.Text),
                        reader.Read<DateOnly>(NpgsqlDbType.Date),
                        (Gender)(reader.Read<byte>(NpgsqlDbType.Smallint))
                    ));
            }

            return list;
        }

        public ValueTask DisposeAsync()
        {
            if(reader is not null)
            {
                reader.DisposeAsync();
            }

            if(connection is not null)
            {
                connection.CloseAsync();
                connection.DisposeAsync();
            }

            return ValueTask.CompletedTask;
        }

    }
}
