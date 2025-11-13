

using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using TestTaskWithDB.Enums;

namespace TestTaskWithDB.Tasks.TaskFour
{
    public class DataService:IAsyncDisposable
    {
        private readonly ILogger<DataService> _logger;
        private string strConnect;
        private NpgsqlConnection? connection;
        private NpgsqlBinaryImporter? writer;
        private object? look = new();

        public DataService(string strConnect, ILogger<DataService> logger)
        { 
            this.strConnect = strConnect;
            _logger = logger;

        }

        public async Task CreatingEmployees()
        {
            connection = new NpgsqlConnection(strConnect);
            connection.Open();
            writer = connection.BeginBinaryImport("COPY public.\"Employees\"(\"Id\",\"FullName\",\"DOB\",\"Gender\") FROM STDIN BINARY");
            

            int countTask = 10;
            List<Task> listTask = new();

            for(int i=0;i<countTask;i++)
            {
                var task = StartTask(i.ToString(), 100000);
                listTask.Add(task);
            }

            await Task.WhenAll(listTask);

            List<CustomEmployee> employees = new();
            for (int i = 0; i < 100; i++)
            {
                employees.Add(GenerateEmployee("F",Gender.Male));
            }

            Write(employees);

            writer.Complete();

            _logger.LogInformation("Все записи созданы!");
        }

        private Task StartTask(string nameTask,int count)
        {
            return Task.Run(() =>
            {
                int countEmployee = count;
                int printLogInfo = 10000;
                while (countEmployee > 0)
                {
                    if (countEmployee < printLogInfo)
                    {
                        printLogInfo = countEmployee;
                    }
                    var list = GenerateEmployees(printLogInfo);
                    Write(list);
                    _logger.LogInformation($"Задача {nameTask}: сгенерировано и отправлено {printLogInfo}");
                    countEmployee -= printLogInfo;
                }
            });
        }

        private List<CustomEmployee> GenerateEmployees(int count)
        {
            List<CustomEmployee> employees = new();
            for(int i = 0; i < count; i++)
            {
                employees.Add(GenerateEmployee());
            }
            return employees;
        }

        private CustomEmployee GenerateEmployee( string? prefix = null, Gender? gender = null)
        {
            Random random = new Random(DateTime.Now.Microsecond);
            Gender gend = gender ?? (random.Next() % 2==0 ? Gender.Male : Gender.Female);
            string name = prefix is not null ? prefix+"test": new string((char)(65 + random.Next() % 26), 10);
            int rnd = random.Next();
            DateOnly date = new DateOnly(1920 + rnd % 105, 1 + rnd % 12, 1 + rnd % 30);
            return new CustomEmployee()
            {
                Id = Guid.NewGuid(),
                FullName = name,
                DOB = date,
                Gender = gend
            };

        }

        public ValueTask DisposeAsync()
        {
            if(writer is not null)
            {
                writer.CloseAsync();
                writer.DisposeAsync();
            }

            if(connection is not null)
            {
                connection.CloseAsync();
                connection.DisposeAsync();
            }

            return ValueTask.CompletedTask;
        }

        private void Write(List<CustomEmployee> list)
        {
            lock (look!)
            {
                foreach(var  employee in list)
                {
                    writer!.StartRow();
                    writer!.Write<Guid>(employee.Id, NpgsqlDbType.Uuid);
                    writer!.Write<string>(employee.FullName, NpgsqlDbType.Text);
                    writer!.Write<DateOnly>(employee.DOB, NpgsqlDbType.Date);
                    writer!.Write<byte>((byte)employee.Gender, NpgsqlDbType.Smallint);
                }
            }
        }
    }

    public struct CustomEmployee
    {
        public Guid Id;
        public string FullName;
        public DateOnly DOB;
        public Gender Gender;
    }

}
