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
        /// Helper class representing an object hit by a ray
        /// </summary>
        private class ObjHit
        {
            /// <summary> Object being hit </summary>
            public ObjectBase Obj { get; }

            /// <summary> Intersection point </summary>
            public Vec3 P { get; }

            /// <summary> Normal vector </summary>
            public Vec3 N { get; set; }

            public ObjHit(ObjectBase obj, Vec3 p, Vec3 n)
            {
                this.Obj = obj;
                this.P = p;
                this.N = n;
            }
        }

        /// <summary>
        /// Get the first object hit by a given ray
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <returns>First object hit or null if ray hits no object</returns>
        private ObjHit FirstInteresction(Ray ray)
        {
            ObjHit first = null;
            float tmin = 0;

            foreach(ObjectBase obj in objects)
            {
                Intersection ints = obj.Intersect(ray);
                if (ints.IsHit && (first == null || ints.T < tmin))
                {
                    first = new ObjHit(obj, ray.Start + ray.Dir * ints.T, ints.Normal);
                    tmin = ints.T;
                }
            }
            return first;
        }

        /// <summary>
        /// Calculate the direct light source for an intersected object
        /// </summary>
        /// <param name="hit">Intersection</param>
        /// <param name="ray">Ray</param>
        /// <returns>Color coming from direct light sources</returns>
        private Color DirectLightSource(ObjHit hit, Ray ray)
        {
            Color color = new Color(0, 0, 0); // Start with black
            Vec3 pOffset = hit.P + hit.N * Global.EPS; // Offset a bit so that shadow ray does not hit the object itself
            foreach (PointLight light in lights)
            {
                // Check if light is directly visible
                Ray shadowRay = new Ray(pOffset, light.Pos - pOffset);
                ObjHit ints = FirstInteresction(shadowRay);
                // If no other object between current object and light source
                if (ints == null || (hit.P - ints.P).Length > (hit.P - light.Pos).Length)
                {
                    float dist = MathF.Pow((hit.P - light.Pos).Length, 2);
                    // Diffuse component
                    float costh = shadowRay.Dir * hit.N;
                    if (costh < Global.EPS) costh = 0;
                    color += light.Lum * (1 / dist) * hit.Obj.Material.Diffuse * costh;
                    // Specular component
                    Vec3 h = (shadowRay.Dir - ray.Dir).Normalize();
                    float costh2 = h * hit.N;
                    if (costh2 < Global.EPS) costh2 = 0;
                    color += light.Lum * (1 / dist) * (hit.Obj.Material.Specular * MathF.Pow(costh2, hit.Obj.Material.Shine));
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
            ObjHit hit = FirstInteresction(ray);
            if (hit == null) return ambient;

            // Check if we are inside an object
            bool inside = false;
            if (ray.Dir * (-1) * hit.N < 0)
            {
                inside = true;
                hit.N = hit.N * -1;
            }

            Color color = new Color(0, 0, 0);

            // Rough materials: direct light source
            if (!hit.Obj.Material.IsReflective && !hit.Obj.Material.IsRefractive)
            {
                color = hit.Obj.Material.Ambient * ambient;
                color += DirectLightSource(hit, ray);
            }

            // TODO reflection / refraction

            return color;
        }

    }
}
