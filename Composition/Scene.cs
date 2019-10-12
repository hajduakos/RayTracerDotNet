using RayTracer.Common;
using RayTracer.Filters;
using RayTracer.Objects;
using RayTracer.Reporting;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

        private readonly ThreadSafeRandom rnd;

        /// <summary>
        /// Create a scene with a given width, height and camera
        /// </summary>
        /// <param name="screenWidth">Scene width (px)</param>
        /// <param name="screenHeight">Scene height (px)</param>
        /// <param name="samplesPerPixel">Samples per pixel (in both directions)</param>
        /// <param name="cam">Camera</param>
        public Scene(int screenWidth, int screenHeight, Camera cam, int samplesPerPixel = 1)
        {
            Contract.Requires(screenWidth > 0, "Screen width must be greater than 0");
            Contract.Requires(screenHeight > 0, "Screen height must be greater than 0");
            Contract.Requires(cam != null, "Camera must not be null");
            Contract.Requires(samplesPerPixel > 0, "Samples per pixel must be greater than 0");
            this.width = screenWidth;
            this.height = screenHeight;
            this.Cam = cam;
            this.samplesPerPixel = samplesPerPixel;
            this.ambient = new Color(.8f, .9f, 1);
            this.lights = new List<PointLight>();
            this.objects = new List<IObject>();
            this.toneMappers = new List<IToneMapper>();
            this.rnd = new ThreadSafeRandom();
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
            // Step 1: render the raw image
            RawImage img = new RawImage(width, height);
            for (int x = 0; x < width; ++x)
            {
                Parallel.For(0, height, y => img[x, y] = TracePixel(x, y));
                if (Reporter != null) Reporter.Report(x, width - 1, "Rendering");
            }
            if (Reporter != null) Reporter.End("Rendering");
            // Step 2: apply tone mapping
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
        private Color TracePixel(int x, int y)
        {
            // One sample in the middle
            if (samplesPerPixel == 1) return Trace(Cam.GetRay(x, y), 0);

            // Multiple samples: trace N x N grid and calculate average
            Color result = new Color(0, 0, 0);
            for (int dx = 0; dx < samplesPerPixel; ++dx)
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

        /// <summary>
        /// Calculate refracted ray (if any)
        /// </summary>
        /// <param name="ray">Original ray</param>
        /// <param name="normal">Intersection normal</param>
        /// <param name="n">Index of refraction</param>
        /// <returns>Refracted ray or null</returns>
        private Nullable<Vec3> RefractRay(Ray ray, Vec3 normal, float n)
        {
            float cosIn = ray.Dir * (-1) * normal;
            if (MathF.Abs(cosIn) < Global.EPS) return null; // No refraction

            // Snellius-Descartes
            float disc = 1 - (1 - cosIn * cosIn) / n / n;
            if (disc < 0) return null; // No refraction

            return normal * (cosIn / n - MathF.Sqrt(disc)) + ray.Dir * (1 / n);
        }

        /// <summary>
        /// Generate a random vector within a sphere with a given radius
        /// </summary>
        /// <param name="radius">Radius</param>
        /// <returns>Random vector with length <= radius</returns>
        private Vec3 RndVec(float radius)
        {
            float theta = rnd.NextFloat() * MathF.PI;
            float phi = rnd.NextFloat() * MathF.PI * 2;
            float r2 = rnd.NextFloat() * radius;
            float x = r2 * MathF.Sin(theta) * MathF.Cos(phi);
            float y = r2 * MathF.Sin(theta) * MathF.Sin(phi);
            float z = r2 * MathF.Cos(theta);
            return new Vec3(x, y, z);
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

            // Rough materials: ambient and direct light source
            Color roughColor = new Color(0, 0, 0);
            if (ints.Mat.IsRough)
            {
                roughColor = ints.Mat.Ambient * ambient;
                roughColor += DirectLightSource(ints, ray);
            }

            // Smooth objects: reflection / refraction
            Color smoothColor = new Color(0, 0, 0);
            if (ints.Mat.IsSmooth)
            {
                Vec3 originalNormal = ints.Normal;

                int blurSamples = ints.Mat.BlurSamples;
                if (d > 0) blurSamples = 1; // Only sample multiple at first hit
                for (int blur = 0; blur < blurSamples; ++blur)
                {
                    // Smooth materials: blur using random offset (only at first hit)
                    if (ints.Mat.Blur > Global.EPS && d == 0)
                    {
                        Vec3 offset = RndVec(ints.Mat.Blur);
                        ints.Normal = (originalNormal + offset).Normalize();
                    }

                    float costh = ray.Dir * (-1) * ints.Normal;
                    if (costh < 0) costh = 0;
                    Color kr = ints.Mat.GetFresnel(costh); // Reflection ratio
                    Color kt = new Color(1, 1, 1) - kr; // Refraction ratio

                    if (ints.Mat.IsRefractive)
                    {
                        float nv = ints.Mat.N.Lum; // Index of refraction (average)
                        if (inside) nv = 1 / nv; // Invert if inside

                        Nullable<Vec3> refractedDir = RefractRay(ray, ints.Normal, nv);
                        if (refractedDir.HasValue)
                        {
                            Ray refractedRay = new Ray(ints.IntsPt, refractedDir.Value).Offset();
                            smoothColor += kt * Trace(refractedRay, d + 1);
                        }
                        else kr = new Color(1, 1, 1); // If no refraction, reflection should be 100%
                    }
                    if (ints.Mat.IsReflective)
                    {
                        Ray reflectedRay = new Ray(ints.IntsPt, ray.Dir - ints.Normal * 2 * (ints.Normal * ray.Dir)).Offset();
                        smoothColor += kr * Trace(reflectedRay, d + 1);
                    }
                }
                smoothColor /= blurSamples;
            }

            return roughColor * ints.Mat.Rough + smoothColor * ints.Mat.Smooth;
        }

    }
}
