using RayTracer.Composition;

namespace RayTracer
{
    class Program
    {
        static void Main(string[] args)
        {
            Scene scene = SceneBuilder.FromXML(args[0]);
            using System.Drawing.Bitmap bmp = scene.Render().ToBitmap();
            bmp.Save(args[1]);
        }
    }
}
