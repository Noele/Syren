using Syren.Syren;

namespace Syren
{
    internal class Program
    {
        public static void Main(string[] args) => new Ship().SetSail(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]).GetAwaiter().GetResult();
    }
}