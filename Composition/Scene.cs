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
        private readonly int dofSamples;
        private readonly float dofRadius;

        /// <summary> Camera </summary>
        public ICamera Cam { get; set; }

        private readonly Color ambient;
        private readonly List<IObject> objects;
        private readonly List<PointLight> pointLights;
        private readonly List<DirLight> dirLights;
        private readonly List<IToneMapper> toneMappers;

        public IReporter Reporter { get; set; }

        private readonly ThreadSafeRandom rnd;

        /// <summary>
        /// Create a scene with a given width, height and camera
        /// </summary>
        /// <param name="screenWidth">Scene width (px)</param>
        /// <param name="screenHeight">Scene height (px)</param>
        /// <param name="samplesPerPixel">Samples per pixel (in both directions)</param>
        /// <param name="dofSamples">Samples to simulate depth of field</param>
        /// <param name="dofRadius">Radius for depth of field (aperture)</param>
        /// <param name="cam">Camera</param>
        public Scene(int screenWidth, int screenHeight, ICamera cam, int samplesPerPixel = 1, int dofSamples = 1, float dofRadius = 0.0f)
        {
            Contract.Requires(screenWidth > 0, "Screen width must be greater than 0");
            Contract.Requires(screenHeight > 0, "Screen height must be greater than 0");
            Contract.Requires(cam != null, "Camera must not be null");
            Contract.Requires(samplesPerPixel > 0, "Samples per pixel must be greater than 0");
            Contract.Requires(dofSamples > 0, "DoF samples must be greater than 0");
            this.width = screenWidth;
            this.height = screenHeight;
            this.Cam = cam;
            this.samplesPerPixel = samplesPerPixel;
            this.dofSamples = dofSamples;
            this.dofRadius = dofRadius;
            this.ambient = new Color(.8f, .9f, 1);
            this.pointLights = new List<PointLight>();
            this.dirLights = new List<DirLight>();
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
        public void AddLight(PointLight light) => pointLights.Add(light);

        /// <summary>
        /// Add a new light to the scene
        /// </summary>
        /// <param name="light">Light to be added</param>
        public void AddLight(DirLight light) => dirLights.Add(light);

        /// <summary>
        /// Add a new light to the scene
        /// </summary>
        /// <param name="light">Light to be added</param>
        public void AddLight(AreaLight light) => pointLights.AddRange(light.ToPointLights());

        public void AddToneMapper(IToneMapper tm) => toneMappers.Add(tm);

        /// <summary>
        /// Render the scene
        /// </summary>
        /// <returns>Rendered raw image</returns>
        public RawImage Render()
        {
            Reporter?.Restart("Rendering");
            // Step 1: render the raw image
            RawImage img = new RawImage(width, height);
            for (int x = 0; x < width; ++x)
            {
                Parallel.For(0, height, y => img[x, y] = TracePixel(x, y));
                Reporter?.Report(x, width - 1, "Rendering");
            }
            Reporter?.End("Rendering");
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
            int samples = 0;
            // Multiple samples: trace N x N grid and calculate average
            Color totalColor = new Color(0, 0, 0);
            Vec3 camr = Cam.Right.Normalize();
            Vec3 camu = Cam.Up.Normalize();
            for (int dx = 0; dx < samplesPerPixel; ++dx)
            {
                float xOffset = 1.0f / samplesPerPixel / 2.0f + dx * 1.0f / samplesPerPixel;
                for (int dy = 0; dy < samplesPerPixel; ++dy)
                {
                    float yOffset = 1.0f / samplesPerPixel / 2.0f + dy * 1.0f / samplesPerPixel;
                    Vec3? maybeRayEnd = Cam.GetRayEnd(x, y, xOffset, yOffset);
                    if (maybeRayEnd == null) continue;
                    Vec3 rayEnd = maybeRayEnd.Value;
                    // Depth of field: keep endpoint of original ray (which is on the focal plane)
                    // but offset origin (eye), sampled uniformly in square with +/- radius
                    for (int dofx = 0; dofx < dofSamples; ++dofx)
                    {
                        Vec3 dofXoff = camr * (-dofRadius + (dofx + 1) * 2 * dofRadius / (dofSamples + 1));
                        for (int dofy = 0; dofy < dofSamples; ++dofy)
                        {
                            Vec3 dofYoff = camu * (-dofRadius + (dofy + 1) * 2 * dofRadius / (dofSamples + 1));
                            Vec3 eyeOffset = dofXoff + dofYoff;
                            if (eyeOffset.Length > dofRadius + Global.EPS) continue; // Drop points outside of circle
                            Vec3 eyePos = Cam.Eye + eyeOffset;
                            Ray ray = new Ray(eyePos, rayEnd - eyePos);
                            totalColor += Trace(ray, 0);

                            ++samples;
                        }
                    }
                }
            }
            if (samples == 0) return totalColor;
            return totalColor / samples;
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
            Color totalColor = new Color(0, 0, 0); // Start with black
            foreach (PointLight light in pointLights)
            {
                // Check if light is directly visible
                Ray shadowRay = new Ray(ints.IntsPt, light.Pos - ints.IntsPt).Offset();
                Intersection shadowInts = FirstInteresction(shadowRay);
                // If no other object between current object and light source
                if (shadowInts == null || (ints.IntsPt - shadowInts.IntsPt).Length > (ints.IntsPt - light.Pos).Length)
                {
                    float squaredDistance = MathF.Pow((ints.IntsPt - light.Pos).Length, 2);
                    // Diffuse component
                    float costhd = shadowRay.Dir * ints.Normal;
                    if (costhd < Global.EPS) costhd = 0;
                    totalColor += light.Lum * (1 / squaredDistance) * ints.Mat.Diffuse * costhd;
                    // Specular component
                    Vec3 h = (shadowRay.Dir - ray.Dir).Normalize();
                    float cosths = h * ints.Normal;
                    if (cosths < Global.EPS) cosths = 0;
                    totalColor += light.Lum * (1 / squaredDistance) * (ints.Mat.Specular * MathF.Pow(cosths, ints.Mat.Shine));
                }
            }
            foreach (DirLight light in dirLights)
            {
                // Check if light is directly visible
                Ray shadowRay = new Ray(ints.IntsPt, light.Dir).Offset();
                Intersection shadowInts = FirstInteresction(shadowRay);
                if (shadowInts == null)
                {
                    // Diffuse component
                    float costhd = shadowRay.Dir * ints.Normal;
                    if (costhd < Global.EPS) costhd = 0;
                    totalColor += light.Lum * ints.Mat.Diffuse * costhd;
                    // Specular component
                    Vec3 h = (shadowRay.Dir - ray.Dir).Normalize();
                    float cosths = h * ints.Normal;
                    if (cosths < Global.EPS) cosths = 0;
                    totalColor += light.Lum * (ints.Mat.Specular * MathF.Pow(cosths, ints.Mat.Shine));
                }
            }
            return totalColor;
        }

        /// <summary>
        /// Calculate refracted ray (if any)
        /// </summary>
        /// <param name="ray">Original ray</param>
        /// <param name="normal">Intersection normal</param>
        /// <param name="n">Index of refraction</param>
        /// <returns>Refracted ray or null</returns>
        private Vec3? RefractRay(Ray ray, Vec3 normal, float n)
        {
            float cosIn = ray.Dir * -normal;
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
            return new Vec3(r2 * MathF.Sin(theta) * MathF.Cos(phi), r2 * MathF.Sin(theta) * MathF.Sin(phi), r2 * MathF.Cos(theta));
        }

        /// <summary>
        /// Recursively trace the color coming from a ray
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="currentDepth">Current depth</param>
        /// <returns>Accummulated color</returns>
        private Color Trace(Ray ray, int currentDepth)
        {
            // Recursion limit reached
            if (currentDepth > Global.MAXDEPTH) return ambient;

            // Check for intersection
            Intersection ints = FirstInteresction(ray);
            if (ints == null) return ambient;

            // Check if we are inside an object
            bool inside = false;
            if (ray.Dir * -ints.Normal < 0)
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
                if (currentDepth > 0) blurSamples = 1; // Only sample multiple at first hit
                for (int blur = 0; blur < blurSamples; ++blur)
                {
                    // Smooth materials: blur using random offset (only at first hit)
                    if (ints.Mat.Blur > Global.EPS && currentDepth == 0)
                    {
                        Vec3 offset = RndVec(ints.Mat.Blur);
                        ints.Normal = (originalNormal + offset).Normalize();
                    }

                    float costh = ray.Dir * -ints.Normal;
                    if (costh < 0) costh = 0;
                    Color kr = ints.Mat.GetFresnel(costh); // Reflection ratio
                    Color kt = new Color(1, 1, 1) - kr; // Refraction ratio

                    if (ints.Mat.IsRefractive)
                    {
                        float nv = ints.Mat.N.Lum; // Index of refraction (average)
                        if (inside) nv = 1 / nv; // Invert if inside

                        Vec3? refractedDir = RefractRay(ray, ints.Normal, nv);
                        if (refractedDir.HasValue)
                        {
                            Ray refractedRay = new Ray(ints.IntsPt, refractedDir.Value).Offset();
                            smoothColor += kt * Trace(refractedRay, currentDepth + 1);
                        }
                        else kr = new Color(1, 1, 1); // If no refraction, reflection should be 100%
                    }
                    if (ints.Mat.IsReflective)
                    {
                        Ray reflectedRay = new Ray(ints.IntsPt, ray.Dir - ints.Normal * 2 * (ints.Normal * ray.Dir)).Offset();
                        smoothColor += kr * Trace(reflectedRay, currentDepth + 1);
                    }
                }
                smoothColor /= blurSamples;
            }

            return roughColor * ints.Mat.Rough + smoothColor * ints.Mat.Smooth;
        }

    }
}
