using RayTracer.Common;

namespace RayTracer.Objects
{
    public class Cube : MeshObject
    {
        public Cube(Vec3 center, float side, Material material) : base(material)
        {
            float s = side / 2;
            Vec3 a = center + new Vec3(s, -s, -s);
            Vec3 b = center + new Vec3(s, s, -s);
            Vec3 c = center + new Vec3(-s, s, -s);
            Vec3 d = center + new Vec3(-s, -s, -s);
            Vec3 e = center + new Vec3(s, -s, s);
            Vec3 f = center + new Vec3(s, s, s);
            Vec3 g = center + new Vec3(-s, s, s);
            Vec3 h = center + new Vec3(-s, -s, s);

            AddTriangle(new Triangle(a, f, e));
            AddTriangle(new Triangle(a, b, f));
            AddTriangle(new Triangle(b, g, f));
            AddTriangle(new Triangle(b, c, g));
            AddTriangle(new Triangle(e, g, h));
            AddTriangle(new Triangle(e, f, g));
            AddTriangle(new Triangle(c, h, g));
            AddTriangle(new Triangle(c, d, h));
            AddTriangle(new Triangle(d, e, h));
            AddTriangle(new Triangle(d, a, e));
            AddTriangle(new Triangle(d, b, a));
            AddTriangle(new Triangle(d, c, b));
        }
    }
}
