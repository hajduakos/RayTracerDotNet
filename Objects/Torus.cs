using RayTracer.Common;
using System;

namespace RayTracer.Objects
{
    /// <summary>
    /// Torus (tessellated)
    /// </summary>
    public class Torus : MeshObject
    {
        /// <summary>
        /// Create a new torus
        /// </summary>
        /// <param name="center">Center point</param>
        /// <param name="ro">Radius of big circle</param>
        /// <param name="ri">Radius of small circle</param>
        /// <param name="tessu">Numer of tessellation points along big circle</param>
        /// <param name="tessv">Number of tessellation points along small circle</param>
        /// <param name="shadingNormals">Use shading normals</param>
        /// <param name="material">Material</param>
        public Torus(Vec3 center, float ro, float ri, int tessu, int tessv, bool shadingNormals, Material material) : base(material)
        {
            // Create mesh points and normals
            Vec3[,] mesh = new Vec3[tessu + 1, tessv + 1];
            Vec3[,] norm = shadingNormals ? new Vec3[tessu + 1, tessv + 1] : null;
            float uf, vf;
            for (int u = 0; u <= tessu; ++u)
            {
                uf = u / (float)tessu * 2 * MathF.PI;
                for (int v = 0; v <= tessv; ++v)
                {
                    vf = v / (float)tessv * 2 * MathF.PI;
                    mesh[u, v] = center + new Vec3(
                        ri * MathF.Sin(vf),
                        (ro + ri * MathF.Cos(vf)) * MathF.Cos(uf),
                        (ro + ri * MathF.Cos(vf)) * MathF.Sin(uf));
                    if (norm != null)
                        norm[u, v] = new Vec3(
                            MathF.Sin(vf),
                            MathF.Cos(vf) * MathF.Cos(uf),
                            MathF.Cos(vf) * MathF.Sin(uf)).Normalize();
                }
            }
            // Create triangles
            for (int u = 0; u < tessu; ++u)
            {
                for (int v = 0; v < tessv; ++v)
                {
                    AddTriangle(new Triangle(mesh[u, v], mesh[u + 1, v], mesh[u, v + 1],
                        norm == null ? null : new ShadingNormals(norm[u, v], norm[u + 1, v], norm[u, v + 1])));
                    AddTriangle(new Triangle(mesh[u + 1, v], mesh[u + 1, v + 1], mesh[u, v + 1],
                        norm == null ? null : new ShadingNormals(norm[u + 1, v], norm[u + 1, v + 1], norm[u, v + 1])));
                }
            }
            // Can be bound with a sphere
            SetBound(new Sphere(center, ro + ri + Global.EPS, material));
        }
    }
}
