using ManyConsole;

namespace NPloy
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            // locate any commands in the assembly (or use an IoC container, or whatever source)
            var commandFactory = new CommandFactory();
            var commands = commandFactory.GetCommands();


            // then run them.
            try
            {
                return ConsoleCommandDispatcher.DispatchCommand(commands, args, System.Console.Out);
            }
            catch (ConsoleException e)
            {

                return e.ExitCode;
            }
            
        }
    }
}
