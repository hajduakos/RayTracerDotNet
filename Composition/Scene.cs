using RayTracer.Common;
using RayTracer.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RayTracer.Composition
{
    public sealed class Scene
    {
        private readonly int w;
        private readonly int h;

        public Camera Cam { get; set; }
        private Color ambient;
        private readonly List<ObjectBase> objects;
        private readonly List<PointLight> lights;

        public Scene(int w, int h, Camera cam)
        {
            this.w = w;
            this.h = h;
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

        public RawImage Render()
        {
            RawImage img = new RawImage(w, h);
            for (int x = 0; x < w; ++x)
            {
                Console.Write("\rRendering {0}/{1}", (x + 1), w);
                Parallel.For(0, h, y => img[x, y] = Trace(Cam.GetRay(x, y), 0));
            }
            Console.WriteLine();
            img.ToneMap();
            return img;
        }

        private class ObjHit
        {
            public ObjectBase Obj { get; }
            public Vec3 P { get; }
            public Vec3 N { get; set; }

            public ObjHit(ObjectBase obj, Vec3 p, Vec3 n)
            {
                this.Obj = obj;
                this.P = p;
                this.N = n;
            }
        }

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

        private Color DirectLightSource(Vec3 x, Vec3 n, Ray ray, ObjectBase obj)
        {
            Color color = new Color(0, 0, 0);
            Vec3 x_offset = x + n * Global.EPS;
            foreach (PointLight light in lights)
            {
                Ray shadowRay = new Ray(x_offset, light.Pos - x_offset);
                ObjHit ints = FirstInteresction(shadowRay);
                if (ints == null || (x - ints.P).Length > (x - light.Pos).Length)
                {
                    float dist = MathF.Pow((x - light.Pos).Length, 2);
                    float costh = shadowRay.Dir * n;
                    if (costh < Global.EPS) costh = 0;
                    color += light.Lum * (1 / dist) * obj.Material.Diffuse * costh;
                    Vec3 h = (shadowRay.Dir - ray.Dir).Normalize();
                    float costh2 = h * n;
                    if (costh2 < Global.EPS) costh2 = 0;
                    color += light.Lum * (1 / dist) * (obj.Material.Specular * MathF.Pow(costh2, obj.Material.Shine));
                }
            }
            return color;
        }

        private Color Trace(Ray ray, int d)
        {
            if (d > Global.DMAX) return ambient;

            ObjHit hit = FirstInteresction(ray);
            if (hit == null) return ambient;
            bool inside = false;
            if (ray.Dir * (-1) * hit.N < 0)
            {
                inside = true;
                hit.N = hit.N * -1;
            }

            Color color = new Color(0, 0, 0);

            if (!hit.Obj.Material.IsReflective && !hit.Obj.Material.IsRefractive)
            {
                color = hit.Obj.Material.Ambient * ambient;
                color += DirectLightSource(hit.P, hit.N, ray, hit.Obj);
            }

            // TODO reflection / refraction

            return color;
        }

    }
}
