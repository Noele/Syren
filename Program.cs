using Syren.Syren;

namespace Syren
{
    internal class Program
    {
        public static void Main(string[] args) => new Ship().SetSail(args[0], args[1], args[2], args[3]).GetAwaiter().GetResult();
    }
}