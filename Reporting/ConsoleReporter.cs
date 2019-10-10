using System;

namespace RayTracer.Reporting
{
    public class ConsoleReporter : IReporter
    {
        private int prevBar = -1;
        private readonly int barWidth = 40;
        public void End(string message = "")
        {
            Console.Write("\r[");
            for (int i = 0; i < barWidth; ++i) Console.Write('#');
            Console.WriteLine("] " + message);
            prevBar = -1;
        }

        public void Report(int progress, int max, string message = "")
        {
            int bar = (int)MathF.Round(progress / (float)max * (barWidth - 1));
            if (bar == prevBar) return;
            Console.Write("\r[");
            for (int i = 0; i < barWidth; i++) Console.Write(i <= bar ? '#' : ' ');
            Console.Write("] " + message);
            prevBar = bar;
        }

        public void Restart(string message = "")
        {
            Console.Write("\r[");
            for (int i = 0; i < barWidth; ++i) Console.Write(' ');
            Console.Write("] " + message);
            prevBar = 0;
        }
    }
}
