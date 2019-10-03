using RayTracer.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RayTracer.Objects
{
    public class Toroid : MeshObject
    {
        public Toroid(Vec3 center, float ro, float ri, int tessu, int tessv, Material material) : base(material)
        {
            Vec3[,] grid = new Vec3[tessu+1, tessv+1];
            Vec3[,] norm = new Vec3[tessu + 1, tessv + 1];
            float uf, vf;
            for(int u = 0; u <= tessu; ++u)
            {
                uf = u / (float)tessu * 2 * MathF.PI;
                for (int v = 0; v <= tessv; ++v)
                {
                    vf = v / (float)tessv * 2 * MathF.PI;
                    grid[u, v] = center + new Vec3(
                        ri * MathF.Sin(vf),
                        (ro + ri * MathF.Cos(vf)) * MathF.Cos(uf),
                        (ro + ri * MathF.Cos(vf)) * MathF.Sin(uf));

                    norm[u, v] = new Vec3(
                        MathF.Sin(vf),
                        MathF.Cos(vf) * MathF.Cos(uf),
                        MathF.Cos(vf) * MathF.Sin(uf)).Normalize();
                }
            }

            for(int u = 0; u < tessu; ++u)
            {
                for(int v = 0; v < tessv; ++v)
                {
                    AddTriangle(new Triangle(grid[u, v], grid[u + 1, v], grid[u, v + 1],
                        new ShadingNormals(norm[u, v], norm[u + 1, v], norm[u, v + 1])));
                    AddTriangle(new Triangle(grid[u + 1, v], grid[u + 1, v + 1], grid[u, v + 1],
                        new ShadingNormals(norm[u + 1, v], norm[u + 1, v + 1], norm[u, v + 1])));
                }
            }
            SetBound(new Sphere(center, ro + ri + Global.EPS, material));
        }
    }
}
