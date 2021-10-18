using RayTracer.Common;
using RayTracer.Composition.Camera;
using RayTracer.Composition.Light;
using RayTracer.Filters;
using RayTracer.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace RayTracer.Composition
{
    /// <summary>
    /// Scene loader from file
    /// </summary>
    public static class SceneBuilder
    {
        private static readonly NumberFormatInfo nfi = CultureInfo.InvariantCulture.NumberFormat;
        public static Scene FromXML(string text)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);

            int scenew = 1080;
            int sceneh = 720;
            ICamera camera = new PerspectiveCamera(new Vec3(1, 1, 1), new Vec3(0, 0, 0), 50 * MathF.PI / 180, 1, scenew, sceneh);
            int spp = 1;
            int dofs = 1;
            float dofr = 0;

            XmlNode sceneNode = doc.ChildNodes[0];
            if (sceneNode.Attributes != null)
            {
                if (sceneNode.Attributes["w"] != null) scenew = Convert.ToInt32(sceneNode.Attributes["w"].Value);
                if (sceneNode.Attributes["h"] != null) sceneh = Convert.ToInt32(sceneNode.Attributes["h"].Value);
                if (sceneNode.Attributes["samplesperpixel"] != null) spp = Convert.ToInt32(sceneNode.Attributes["samplesperpixel"].Value);
                if (sceneNode.Attributes["dofsamples"] != null) dofs = Convert.ToInt32(sceneNode.Attributes["dofsamples"].Value);
                if (sceneNode.Attributes["dofradius"] != null) dofr = Convert.ToSingle(sceneNode.Attributes["dofradius"].Value, nfi);
            }
            Scene scene = new Scene(scenew, sceneh, camera, spp, dofs, dofr);
            Dictionary<string, Material> materials = new Dictionary<string, Material>();

            foreach (XmlNode node in sceneNode.ChildNodes)
            {
                if (Eq(node.Name, typeof(PerspectiveCamera)) || Eq(node.Name, typeof(FisheyeCamera)))
                {
                    Vec3 eye = Vec3FromString(node.Attributes["eye"].Value);
                    Vec3 lookat = Vec3FromString(node.Attributes["lookat"].Value);
                    float hfov = 50;
                    float focalDist = (lookat - eye).Length;
                    bool diagonal = false;
                    if (node.Attributes["hfov"] != null) hfov = Convert.ToSingle(node.Attributes["hfov"].Value, nfi);
                    if (node.Attributes["focaldist"] != null) focalDist = Convert.ToSingle(node.Attributes["focaldist"].Value, nfi);
                    if (node.Attributes["diagonal"] != null) diagonal = Convert.ToBoolean(node.Attributes["diagonal"].Value);
                    if (Eq(node.Name, typeof(PerspectiveCamera)))
                        scene.Cam = new PerspectiveCamera(eye, lookat, hfov * MathF.PI / 180, focalDist, scenew, sceneh);
                    else
                        scene.Cam = new FisheyeCamera(eye, lookat, focalDist, scenew, sceneh, diagonal);
                }
                else if (Eq(node.Name, typeof(Material)))
                {
                    string id = node.Attributes["id"].Value;
                    float rough = node.Attributes["rough"] == null ? 1 : Convert.ToSingle(node.Attributes["rough"].Value, nfi);
                    Color ambient = node.Attributes["ambient"] == null ? new Color(0, 0, 0) : ColorFromString(node.Attributes["ambient"].Value);
                    Color diffuse = node.Attributes["diffuse"] == null ? new Color(0, 0, 0) : ColorFromString(node.Attributes["diffuse"].Value);
                    Color specular = node.Attributes["specular"] == null ? new Color(0, 0, 0) : ColorFromString(node.Attributes["specular"].Value);
                    float shine = node.Attributes["shine"] == null ? 0 : Convert.ToSingle(node.Attributes["shine"].Value, nfi);

                    float smooth = node.Attributes["smooth"] == null ? 0 : Convert.ToSingle(node.Attributes["smooth"].Value, nfi);
                    Color n = node.Attributes["n"] == null ? new Color(0, 0, 0) : ColorFromString(node.Attributes["n"].Value);
                    Color kap = node.Attributes["kap"] == null ? new Color(0, 0, 0) : ColorFromString(node.Attributes["kap"].Value);
                    bool isReflective = node.Attributes["isreflective"] != null && Convert.ToBoolean(node.Attributes["isreflective"].Value);
                    bool isRefractive = node.Attributes["isrefractive"] != null && Convert.ToBoolean(node.Attributes["isrefractive"].Value);
                    float blur = node.Attributes["blur"] == null ? 0 : Convert.ToSingle(node.Attributes["blur"].Value, nfi);
                    int blursamples = node.Attributes["blursamples"] == null ? 1 : Convert.ToInt32(node.Attributes["blursamples"].Value);

                    materials.Add(id, new Material(rough, ambient, diffuse, specular, shine, smooth, isReflective, isRefractive, n, kap, blur, blursamples));
                }
                else if (Eq(node.Name, typeof(PointLight)))
                {
                    scene.AddLight(Construct<PointLight>(node, materials));
                }
                else if (Eq(node.Name, typeof(DirLight)))
                {
                    scene.AddLight(Construct<DirLight>(node, materials));
                }
                else if (Eq(node.Name, typeof(AreaLight)))
                {
                    scene.AddLight(Construct<AreaLight>(node, materials));
                }
                else if (Eq(node.Name, typeof(Plane)))
                {
                    scene.AddObject(Construct<Plane>(node, materials));
                }
                else if (Eq(node.Name, typeof(CheckerBoard)))
                {
                    scene.AddObject(Construct<CheckerBoard>(node, materials));
                }
                else if (Eq(node.Name, typeof(Sphere)))
                {
                    scene.AddObject(Construct<Sphere>(node, materials));
                }
                else if (Eq(node.Name, typeof(Cube)))
                {
                    scene.AddObject(Construct<Cube>(node, materials));
                }
                else if (Eq(node.Name, typeof(Torus)))
                {
                    scene.AddObject(Construct<Torus>(node, materials));
                }
                else if (Eq(node.Name, typeof(Cylinder)))
                {
                    scene.AddObject(Construct<Cylinder>(node, materials));
                }
                else if (Eq(node.Name, typeof(Cone)))
                {
                    scene.AddObject(Construct<Cone>(node, materials));
                }
                else if (node.Name == "tonemapper")
                {
                    if (node.Attributes["type"].Value == "maxlinear")
                        scene.AddToneMapper(new MaxLinearToneMapper());
                    else if (node.Attributes["type"].Value == "shlick")
                        scene.AddToneMapper(new MaxLinearToneMapper());
                    else if (node.Attributes["type"].Value == "nonlinear")
                    {
                        float p = node.Attributes["p"] == null ? 1 : Convert.ToSingle(node.Attributes["p"].Value, nfi);
                        scene.AddToneMapper(new NonLinearToneMapper(p));
                    }
                    else
                        throw new XmlException($"Unknown tone mapper: {node.Attributes["type"]}");
                }
                else
                {
                    throw new XmlException($"Unknown node: {node.Name}");
                }
            }

            return scene;
        }

        private static bool Eq(string str, Type t)
        {
            return string.Equals(str, t.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        private static T Construct<T>(XmlNode n, Dictionary<string, Material> materials)
        {
            ConstructorInfo ci = typeof(T).GetConstructors()[0];
            object[] parameters = new object[ci.GetParameters().Length];
            for (int i = 0; i < ci.GetParameters().Length; ++i)
            {
                var pi = ci.GetParameters()[i];
                if (n.Attributes[pi.Name] == null)
                    throw new Exception($"Attribute {pi.Name} not found for {typeof(T).Name}");
                string txt = n.Attributes[pi.Name].Value;
                if (pi.ParameterType == typeof(Vec3))
                    parameters[i] = Vec3FromString(txt);
                else if (pi.ParameterType == typeof(Color))
                    parameters[i] = ColorFromString(txt);
                else if (pi.ParameterType == typeof(Int32))
                    parameters[i] = Convert.ToInt32(txt);
                else if (pi.ParameterType == typeof(Single))
                    parameters[i] = Convert.ToSingle(txt, nfi);
                else if (pi.ParameterType == typeof(Boolean))
                    parameters[i] = Convert.ToBoolean(txt);
                else if (pi.ParameterType == typeof(Material))
                    parameters[i] = materials[txt];
            }
            return (T)ci.Invoke(parameters);
        }

        private static Vec3 Vec3FromString(string s)
        {
            string[] values = s.Split(" ");
            Vec3 origin = new Vec3(
                Convert.ToSingle(values[0], nfi),
                Convert.ToSingle(values[1], nfi),
                Convert.ToSingle(values[2], nfi));
            if (values.Length == 6)
                return Vec3.FromAngle(
                    origin,
                    Convert.ToSingle(values[3], nfi) * MathF.PI / 180,
                    Convert.ToSingle(values[4], nfi) * MathF.PI / 180,
                    Convert.ToSingle(values[5], nfi));
            return origin;
        }
        private static Color ColorFromString(string s)
        {
            string[] values = s.Split(" ");
            return new Color(
                Convert.ToSingle(values[0], nfi),
                Convert.ToSingle(values[1], nfi),
                Convert.ToSingle(values[2], nfi));
        }
    }
}
