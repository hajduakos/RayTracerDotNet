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
                if (IsType(node, typeof(PerspectiveCamera)) || IsType(node, typeof(FisheyeCamera)))
                {
                    Vec3 eye = Vec3FromString(node.Attributes["eye"].Value);
                    Vec3 lookat = Vec3FromString(node.Attributes["lookat"].Value);
                    float hfov = 50;
                    float focalDist = (lookat - eye).Length;
                    bool diagonal = false;
                    if (node.Attributes["hfov"] != null) hfov = Convert.ToSingle(node.Attributes["hfov"].Value, nfi);
                    if (node.Attributes["focaldist"] != null) focalDist = Convert.ToSingle(node.Attributes["focaldist"].Value, nfi);
                    if (node.Attributes["diagonal"] != null) diagonal = Convert.ToBoolean(node.Attributes["diagonal"].Value);
                    if (IsType(node, typeof(PerspectiveCamera)))
                        scene.Cam = new PerspectiveCamera(eye, lookat, hfov * MathF.PI / 180, focalDist, scenew, sceneh);
                    else
                        scene.Cam = new FisheyeCamera(eye, lookat, focalDist, scenew, sceneh, diagonal);
                }
                else if (IsType(node, typeof(Material)))
                    materials.Add(node.Attributes["id"].Value, Construct<Material>(node, materials));
                else if (IsType(node, typeof(PointLight)))
                    scene.AddLight(Construct<PointLight>(node, materials));
                else if (IsType(node, typeof(DirLight)))
                    scene.AddLight(Construct<DirLight>(node, materials));
                else if (IsType(node, typeof(AreaLight)))
                    scene.AddLight(Construct<AreaLight>(node, materials));
                else if (IsType(node, typeof(Plane)))
                    scene.AddObject(Construct<Plane>(node, materials));
                else if (IsType(node, typeof(CheckerBoard)))
                    scene.AddObject(Construct<CheckerBoard>(node, materials));
                else if (IsType(node, typeof(Sphere)))
                    scene.AddObject(Construct<Sphere>(node, materials));
                else if (IsType(node, typeof(Cube)))
                    scene.AddObject(Construct<Cube>(node, materials));
                else if (IsType(node, typeof(Torus)))
                    scene.AddObject(Construct<Torus>(node, materials));
                else if (IsType(node, typeof(Cylinder)))
                    scene.AddObject(Construct<Cylinder>(node, materials));
                else if (IsType(node, typeof(Cone)))
                    scene.AddObject(Construct<Cone>(node, materials));
                else if (IsType(node, typeof(MaxLinearToneMapper)))
                    scene.AddToneMapper(Construct<MaxLinearToneMapper>(node, materials));
                else if (IsType(node, typeof(NonLinearToneMapper)))
                    scene.AddToneMapper(Construct<NonLinearToneMapper>(node, materials));
                else if (IsType(node, typeof(SchlickToneMapper)))
                    scene.AddToneMapper(Construct<SchlickToneMapper>(node, materials));
                else
                    throw new XmlException($"Unknown node: {node.Name}");
            }

            return scene;
        }

        private static bool IsType(XmlNode n, Type t)
        {
            return string.Equals(n.Name, t.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        private static T Construct<T>(XmlNode n, Dictionary<string, Material> materials)
        {
            var constructors = typeof(T).GetConstructors();
            if (constructors.Length != 1)
                throw new Exception($"Object {typeof(T).Name} does not have an unique constructor.");
            ConstructorInfo ci = constructors[0];
            object[] parameters = new object[ci.GetParameters().Length];
            for (int i = 0; i < ci.GetParameters().Length; ++i)
            {
                var pi = ci.GetParameters()[i];
                if (n.Attributes[pi.Name] == null)
                {
                    if (pi.HasDefaultValue)
                        parameters[i] = pi.DefaultValue;
                    else
                        throw new Exception($"Attribute {pi.Name} not found for {typeof(T).Name}.");
                }
                else
                {
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
                    else
                        throw new Exception($"Type {pi.ParameterType.Name} of attribute {pi.Name} not supported.");
                }
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
