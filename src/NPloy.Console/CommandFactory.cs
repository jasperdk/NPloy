using System.Collections.Generic;
using System.Linq;
using ManyConsole;

namespace NPloy
{
    public interface ICommandFactory
    {
        IEnumerable<ConsoleCommand> GetCommands();
        T GetCommand<T>() where T : ConsoleCommand;
    }

    internal class CommandFactory : ICommandFactory
    {
        public IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }

        public T GetCommand<T>() where T : ConsoleCommand
        {
            var result = GetCommands().FirstOrDefault(c => c.GetType() == typeof(T));
            if (result == null)
                return null;
            return (T)result;
        }
    }
}