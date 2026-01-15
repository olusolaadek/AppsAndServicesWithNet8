using System.Diagnostics; // To use Stopwatch.
using static System.Console;

partial class Program
{
    private static void Main(string[] args)
    {
        OutputThreadInfo();
        Stopwatch timer = Stopwatch.StartNew();
        SectionTitle("Running methods synchronously on one thread.");

        MethodA();
        MethodB();
        MethodC();

        WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");
    }
}