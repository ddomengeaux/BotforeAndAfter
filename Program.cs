using System;
using System.Threading.Tasks;

namespace BotforeAndAfters
{
    internal class Program
    {
        private static Task Main(string[] args)
        {
            try
            {
                return new Bot(args).StartAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}
