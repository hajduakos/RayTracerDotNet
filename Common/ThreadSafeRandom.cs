using System;

namespace RayTracer.Common
{
    /// <summary>
    /// Thread-safe random number generator based on
    /// https://stackoverflow.com/questions/3049467/is-c-sharp-random-number-generator-thread-safe
    /// </summary>
    public class ThreadSafeRandom
    {
        private static readonly Random _global = new Random();
        [ThreadStatic] private static Random _local;

        public float NextFloat()
        {
            if (_local == null)
            {
                lock (_global)
                {
                    if (_local == null)
                    {
                        int seed = _global.Next();
                        _local = new Random(seed);
                    }
                }
            }

            return (float)_local.NextDouble();
        }
    }
}
