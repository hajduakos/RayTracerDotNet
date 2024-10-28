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
            if (args.Length != 1)
            {
                Console.WriteLine("Exactly one argument is required: input scene (xml)");
                return 1;
            }
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                AnimationPreprocessor ap = new(File.ReadAllText(args[0]));
                for (int f = 0; f < ap.Frames; ++f)
                {
                    Console.WriteLine($"Frame {(f + 1)}/{ap.Frames}");
                    Scene scene = new SceneBuilder().FromXML(ap.GetFrame(f));
                    scene.Reporter = new ConsoleReporter();
                    scene.Render().WriteToFile($"{args[0]}{f:D3}.bmp");
                }
                sw.Stop();
                Console.WriteLine($"Elapsed: {sw.Elapsed}");
                return 0;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File not found: {ex.FileName}");
                return 2;
            }
            catch (XmlException ex)
            {
                Console.WriteLine($"XML exception: {ex.Message}");
                return 3;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Other exception: {ex.Message}");
                return 4;
            }
        }
    }
}
