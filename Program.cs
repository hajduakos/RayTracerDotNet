using RayTracer.Composition;
using RayTracer.Reporting;
using System.Diagnostics;

namespace RayTracer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Scene scene = SceneBuilder.FromXML(args[0]);
            scene.Reporter = new ConsoleReporter();
            using System.Drawing.Bitmap bmp = scene.Render().ToBitmap();
            bmp.Save(args[1]);
            sw.Stop();
            System.Console.WriteLine("Elapsed: " + sw.Elapsed);
        }
    }
}
