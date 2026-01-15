using System;
using System.Collections.Generic;
using System.Text;

partial class Program
{
    private static void MethodA()
    {
        TaskTitle("Starting Method A...");
        OutputThreadInfo();
        Thread.Sleep(3000);
        TaskTitle("Method A complete.");
    }

    private static void MethodB()
    {
        TaskTitle("Starting Method B...");
        OutputThreadInfo();
        Thread.Sleep(2000);
        TaskTitle("Method B complete.");
    }

    private static void MethodC()
    {
        TaskTitle("Starting Method C...");
        OutputThreadInfo();
        Thread.Sleep(1000);
        TaskTitle("Method C complete.");
    }
}
