using Kaye.Arguments;

namespace Kaye
{
    class Program
    {
        static void Main(string[] args)
        {
            KayeArguments Arguments = KayeArguments.Parse(args);
            Kaye Kaye = new Kaye(Arguments);
            Kaye.Run();
        }
    }
}