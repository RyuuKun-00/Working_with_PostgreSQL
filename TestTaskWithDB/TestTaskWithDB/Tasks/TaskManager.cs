

using Microsoft.Extensions.Logging;
using TestTaskWithDB.Abstractions;

namespace TestTaskWithDB.Tasks
{
    public class TaskManager : ITaskManager
    {
        private readonly ILogger<TaskManager> _logger;
        private Dictionary<string, ICommandHandler> handlers = new();

        public TaskManager(ILogger<TaskManager> logger)
        {
            _logger = logger;
        }

        public void AddCommandHandler(ICommandHandler handler)
        {
            handlers.Add(handler.Command, handler);
            _logger.LogDebug("Добавлена обработка ключа: {key}\r\nФункция: {task}", handler.Command, handler.ToString());
        }

        public async Task<bool> Execute(string[] args)
        {
            // Проверка на наличие аргументов
            if (args.Length == 0)
            {
                _logger.LogInformation("Список аргуметов пуст.");
                return false;
            }

            // Проверка на наличие обработчика команды
            var isTry = handlers.TryGetValue(args[0], out var task);

            if (!isTry)
            {
                _logger.LogInformation("Событие для команды: {key} - отсутствует.", args[0]);
                return false;
            }

            // Выполнение команды
            _logger.LogInformation("Выполнение команды: {key}\r\nОбъект: {task}\r\nСписок аргументов: {args}", 
                                    args[0], task!.ToString(),String.Join(", ",args));
            return await task!.Invoke(args);
        }
    }
}
