using RayTracer.Common;
using RayTracer.Filters;
using RayTracer.Objects;
using RayTracer.Reporting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RayTracer.Composition
{
    /// <summary>
    /// Scene that holds all objects, lights, etc. and is responsible
    /// for rendering the image.
    /// </summary>
    public sealed class Scene
    {
        private readonly int width;
        private readonly int height;
        private readonly int samplesPerPixel;

        /// <summary> Camera </summary>
        public Camera Cam { get; set; }

        private readonly Color ambient;
        private readonly List<IObject> objects;
        private readonly List<PointLight> lights;
        private readonly List<IToneMapper> toneMappers;

        public IReporter Reporter { get; set; }

        /// <summary>
        /// Create a scene with a given width, height and camera
        /// </summary>
        /// <param name="screenWidth">Scene width (px)</param>
        /// <param name="screenHeight">Scene height (px)</param>
        /// <param name="samplesPerPixel">Samples per pixel (in both directions)</param>
        /// <param name="cam">Camera</param>
        public Scene(int screenWidth, int screenHeight, Camera cam, int samplesPerPixel = 1)
        {
            this.width = screenWidth;
            this.height = screenHeight;
            this.Cam = cam;
            this.samplesPerPixel = samplesPerPixel;
            this.ambient = new Color(.8f, .9f, 1);
            this.lights = new List<PointLight>();
            this.objects = new List<IObject>();
            this.toneMappers = new List<IToneMapper>();
        }

        /// <summary>
        /// Add a new object to the scene
        /// </summary>
        /// <param name="obj">Object to be added</param>
        public void AddObject(IObject obj) => objects.Add(obj);

        /// <summary>
        /// Add a new light to the scene
        /// </summary>
        /// <param name="light">Light to be added</param>
        public void AddLight(PointLight light) => lights.Add(light);

        public void AddToneMapper(IToneMapper tm) => toneMappers.Add(tm);

        /// <summary>
        /// Render the scene
        /// </summary>
        /// <returns>Rendered raw image</returns>
        public RawImage Render()
        {
            if (Reporter != null) Reporter.Restart("Rendering");
            RawImage img = new RawImage(width, height);
            for (int x = 0; x < width; ++x)
            {
                Parallel.For(0, height, y => img[x, y] = TracePixel(x, y));
                if (Reporter != null) Reporter.Report(x, width - 1, "Rendering");
            }
            if (Reporter != null) Reporter.End("Rendering");
            foreach (IToneMapper tm in toneMappers)
            {
                tm.Reporter = this.Reporter;
                tm.ToneMap(img);
            }
            return img;
        }

        /// <summary>
        /// Trace the color for a given pixel
        /// </summary>
        /// <param name="x">Pixel X</param>
        /// <param name="y">Pixel Y</param>
        /// <returns></returns>
        Color TracePixel(int x, int y)
        {
            // One sample in the middle
            if (samplesPerPixel == 1) return Trace(Cam.GetRay(x, y), 0);

            // Multiple samples: N x N grid and average
            Color result = new Color(0, 0, 0);
            for(int dx = 0; dx < samplesPerPixel; ++dx)
            {
                float xOff = 1.0f / samplesPerPixel / 2.0f + dx * 1.0f / samplesPerPixel;
                for (int dy = 0; dy < samplesPerPixel; ++dy)
                {
                    float yOff = 1.0f / samplesPerPixel / 2.0f + dy * 1.0f / samplesPerPixel;
                    result += Trace(Cam.GetRay(x, y, xOff, yOff), 0);
                }
            }
            return result / (samplesPerPixel * samplesPerPixel);
        }

        /// <summary>
        /// Get the first object hit by a given ray
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <returns>First object hit or null if ray hits no object</returns>
        private Intersection FirstInteresction(Ray ray)
        {
            Intersection first = null;

            foreach (IObject obj in objects)
            {
                Intersection ints = obj.Intersect(ray);
                if (ints != null && (first == null || ints.T < first.T)) first = ints;
            }
            return first;
        }

        /// <summary>
        /// Calculate the direct light source for an intersected object
        /// </summary>
        /// <param name="ints">Intersection</param>
        /// <param name="ray">Ray</param>
        /// <returns>Color coming from direct light sources</returns>
        private Color DirectLightSource(Intersection ints, Ray ray)
        {
            Color color = new Color(0, 0, 0); // Start with black
            Vec3 pOffset = ints.IntsPt + ints.Normal * Global.EPS; // Offset a bit so that shadow ray does not hit the object itself
            foreach (PointLight light in lights)
            {
                // Check if light is directly visible
                Ray shadowRay = new Ray(pOffset, light.Pos - pOffset);
                Intersection shadowInts = FirstInteresction(shadowRay);
                // If no other object between current object and light source
                if (shadowInts == null || (pOffset - shadowInts.IntsPt).Length > (pOffset - light.Pos).Length)
                {
                    float dist = MathF.Pow((pOffset - light.Pos).Length, 2);
                    // Diffuse component
                    float costh = shadowRay.Dir * ints.Normal;
                    if (costh < Global.EPS) costh = 0;
                    color += light.Lum * (1 / dist) * ints.Mat.Diffuse * costh;
                    // Specular component
                    Vec3 h = (shadowRay.Dir - ray.Dir).Normalize();
                    float costh2 = h * ints.Normal;
                    if (costh2 < Global.EPS) costh2 = 0;
                    color += light.Lum * (1 / dist) * (ints.Mat.Specular * MathF.Pow(costh2, ints.Mat.Shine));
                }
            }
            return color;
        }

        private Nullable<Vec3> RefractRay(Ray ray, Vec3 normal, float n)
        {
            float cosIn = ray.Dir * (-1) * normal;
            if (MathF.Abs(cosIn) < Global.EPS) return null;

            // Snellius-Descartes
            float disc = 1 - (1 - cosIn * cosIn) / n / n;
            if (disc < 0) return null;

            return normal * (cosIn / n - MathF.Sqrt(disc)) + ray.Dir * (1 / n);
        }

        /// <summary>
        /// Recursively trace the color coming from a ray
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="d">Current depth</param>
        /// <returns>Accummulated color</returns>
        private Color Trace(Ray ray, int d)
        {
            // Recursion limit reached
            if (d > Global.DMAX) return ambient;

            // Check for intersection
            Intersection ints = FirstInteresction(ray);
            if (ints == null) return ambient;

            // Check if we are inside an object
            bool inside = false;
            if (ray.Dir * (-1) * ints.Normal < 0)
            {
                inside = true;
                ints.Normal *= -1;
            }

            Color color = new Color(0, 0, 0);

            // Rough materials: direct light source
            if (ints.Mat.IsRough)
            {
                color = ints.Mat.Ambient * ambient * ints.Mat.Rough;
                color += DirectLightSource(ints, ray) * ints.Mat.Rough;
            }

            // Smooth objects: reflection / refraction
            if (ints.Mat.IsSmooth)
            {
                float costh = ray.Dir * (-1) * ints.Normal;
                if (costh < 0) costh = 0;
                Color kr = ints.Mat.GetFresnel(costh);
                Color kt = new Color(1, 1, 1) - kr;

                if (ints.Mat.IsRefractive)
                {
                    float nv = ints.Mat.N.Lum; // Index of refraction (average)
                    if (inside) nv = 1 / nv; // Invert if inside

                    Nullable<Vec3> refractDir = RefractRay(ray, ints.Normal, nv);
                    if (refractDir.HasValue)
                    {
                        Ray refracted = new Ray(ints.IntsPt, refractDir.Value).Offset();
                        color += kt * Trace(refracted, d + 1) * ints.Mat.Smooth;
                    }
                    else kr = new Color(1, 1, 1); // If no refraction, reflection should be 100%
                }
                if (ints.Mat.IsReflective)
                {
                    Ray refl = new Ray(ints.IntsPt, ray.Dir - ints.Normal * 2 * (ints.Normal * ray.Dir)).Offset();
                    color += kr * Trace(refl, d + 1) * ints.Mat.Smooth;
                }
            }

            return color;
        }

    }
}
