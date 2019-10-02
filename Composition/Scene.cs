using RayTracer.Common;
using RayTracer.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RayTracer.Composition
{
    public sealed class Scene
    {
        private readonly int width;
        private readonly int height;

        /// <summary> Camera </summary>
        public Camera Cam { get; set; }

        private Color ambient;
        private readonly List<ObjectBase> objects;
        private readonly List<PointLight> lights;

        public Scene(int w, int h, Camera cam)
        {
            this.width = w;
            this.height = h;
            this.Cam = cam;
            this.ambient = new Color(.8f, .9f, 1);
            this.lights = new List<PointLight>();
            this.objects = new List<ObjectBase>();
        }

        public void AddObject(ObjectBase obj)
        {
            objects.Add(obj);
        }

        public void AddLight(PointLight light)
        {
            lights.Add(light);
        }

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
            img.ToneMap();
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

            foreach(ObjectBase obj in objects)
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
                if (shadowInts == null || (ints.IntsPt - shadowInts.IntsPt).Length > (ints.IntsPt - light.Pos).Length)
                {
                    float dist = MathF.Pow((ints.IntsPt - light.Pos).Length, 2);
                    // Diffuse component
                    float costh = shadowRay.Dir * ints.Normal;
                    if (costh < Global.EPS) costh = 0;
                    color += light.Lum * (1 / dist) * ints.Obj.Material.Diffuse * costh;
                    // Specular component
                    Vec3 h = (shadowRay.Dir - ray.Dir).Normalize();
                    float costh2 = h * ints.Normal;
                    if (costh2 < Global.EPS) costh2 = 0;
                    color += light.Lum * (1 / dist) * (ints.Obj.Material.Specular * MathF.Pow(costh2, ints.Obj.Material.Shine));
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
            if (!ints.Obj.Material.IsReflective && !ints.Obj.Material.IsRefractive)
            {
                color = ints.Obj.Material.Ambient * ambient;
                color += DirectLightSource(ints, ray);
            }

            // TODO reflection / refraction

            return color;
        }

    }
}
