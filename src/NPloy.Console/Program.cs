using System.Collections.Generic;
using ManyConsole;

namespace NPloy
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            // locate any commands in the assembly (or use an IoC container, or whatever source)
            var commands = GetCommands();


            // then run them.
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, System.Console.Out);
        }


        public static IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof (Program));
        }
    }
}
