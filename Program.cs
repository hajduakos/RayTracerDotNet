using RayTracer.Common;
using RayTracer.Composition;
using RayTracer.Objects;
using System;
using System.Diagnostics;

namespace RayTracer
{
    class Program
    {
        static void Main(string[] args)
        {
            Scene scene = new Scene(1280, 720, new Camera(new Vec3(7, 0, 2.5f), new Vec3(0, 0, 1), 50 / 180f * MathF.PI, 1280, 720));
            scene.AddLight(new PointLight(new Vec3(40, 40, 40), new Color(6400, 6400, 6400)));
            scene.AddObject(new PlaneXY(new Material(new Color(170f / 2550, 117f / 2550, 0), new Color(170f / 255, 117f / 255, 0), new Color(), 0)));
            scene.AddObject(new Sphere(new Vec3(0, 0, 1), 1, new Material(new Color(0.1f, 0, 0), new Color(1, 0, 0), new Color(), 0)));
            scene.AddObject(new Sphere(new Vec3(0, -2, 1), 1, new Material(new Color(0, 0.1f, 0), new Color(0, .5f, 0), new Color(0, .5f, 0), 5)));
            scene.AddObject(new Sphere(new Vec3(0, 2, 1), 1, new Material(new Color(0, 0, 0.1f), new Color(), new Color(0, 0, 1), 10)));
            Stopwatch sw = Stopwatch.StartNew();
            System.Drawing.Bitmap bmp = scene.Render().ToBitmap();
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds.ToString() + "ms");
            bmp.Save("test.bmp");
        }
    }
}
