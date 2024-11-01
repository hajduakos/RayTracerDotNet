﻿namespace RayTracer.Reporting
{
    public interface IReporter
    {
        void Restart(string message = "");

        void Report(int progress, int max, string message = "");

        void End(string message = "");
    }
}
