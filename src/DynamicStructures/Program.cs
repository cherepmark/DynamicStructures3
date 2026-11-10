using System.Text;
using DynamicStructures.Cli;

namespace DynamicStructures;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
        }
        catch (IOException)
        {
        }

        if (args.Length > 0)
        {
            return CliCommandHandler.Run(args);
        }

        new ConsoleMenu(Console.In, Console.Out).Run();
        return 0;
    }
}
