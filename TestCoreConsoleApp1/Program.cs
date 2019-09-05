using System;
using System.Threading.Tasks;

namespace TestCoreConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World 1!");

            //await MyMethod();

            Console.WriteLine("Hello World 2!");

            Bob().ContinueWith((_) => Console.WriteLine(_.Result)).Wait();
            //Task.Factory.StartNew()

            Console.ReadLine();
        }

        private static async Task MyMethod()
        {
            await Task.Delay(10);
            
        }

        private static Task<int> Bob()
        {
            return Task.Run(() => {Task.Delay(100); Console.WriteLine("1"); }).ContinueWith((_) => {Console.WriteLine("2"); return 3; });
        }
    }
}
