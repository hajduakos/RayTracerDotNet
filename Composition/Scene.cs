using RayTracer.Common;
using RayTracer.Filters;
using RayTracer.Objects;
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

        /// <summary> Camera </summary>
        public Camera Cam { get; set; }

        private Color ambient;
        private readonly List<IObject> objects;
        private readonly List<PointLight> lights;
        private readonly List<IToneMapper> toneMappers;

        /// <summary>
        /// Create a scene with a given width, height and camera
        /// </summary>
        /// <param name="screenWidth">Scene width (px)</param>
        /// <param name="screenHeight">Scene height (px)</param>
        /// <param name="cam">Camera</param>
        public Scene(int screenWidth, int screenHeight, Camera cam)
        {
            this.width = screenWidth;
            this.height = screenHeight;
            this.Cam = cam;
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
            RawImage img = new RawImage(width, height);
            for (int x = 0; x < width; ++x)
            {
                Console.Write("\rRendering {0}/{1}", (x + 1), width);
                Parallel.For(0, height, y => img[x, y] = Trace(Cam.GetRay(x, y), 0));
            }
            Console.WriteLine();
            foreach (IToneMapper tm in toneMappers)
                tm.ToneMap(img);
            return img;
        }

        /// <summary>
        /// Get the first object hit by a given ray
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <returns>First object hit or null if ray hits no object</returns>
        private Intersection FirstInteresction(Ray ray)
        {
            Intersection first = null;

            foreach(IObject obj in objects)
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
            if (!ints.Mat.IsReflective && !ints.Mat.IsRefractive)
            {
                color = ints.Mat.Ambient * ambient;
                color += DirectLightSource(ints, ray);
            }

            // TODO reflection / refraction

            return color;
        }

    }
}
