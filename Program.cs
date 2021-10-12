using RayTracer.Composition;
using RayTracer.Reporting;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace RayTracer
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Two arguments are required: input scene (xml) and output image (png)");
                return 1;
            }
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                Scene scene = SceneBuilder.FromXML(File.ReadAllText(args[0]));
                scene.Reporter = new ConsoleReporter();
                using System.Drawing.Bitmap bmp = scene.Render().ToBitmap();
                bmp.Save(args[1]);
                sw.Stop();
                Console.WriteLine("Elapsed: " + sw.Elapsed);
                return 0;
            }
            catch(FileNotFoundException ex)
            {
                Console.WriteLine("File not found: " + ex.FileName);
                return 2;
            }
            catch(XmlException ex)
            {
                Console.WriteLine("XML exception: " + ex.Message);
                return 3;
            }
        }
    }
}
