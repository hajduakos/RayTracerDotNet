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
    public sealed class SceneBuilder
    {
        private static readonly NumberFormatInfo nfi = CultureInfo.InvariantCulture.NumberFormat;
        readonly Dictionary<string, Material> materials = new();
        public Scene FromXML(string text)
        {
            XmlDocument doc = new();
            doc.LoadXml(text);
            materials.Clear();
            XmlNode sceneNode = doc.ChildNodes[0];
            Scene scene = Construct<Scene>(sceneNode);

            foreach (XmlNode node in sceneNode.ChildNodes)
            {
                if (node.Name == "#comment")
                    continue; // Ignore comments
                else if (IsType(node, typeof(PerspectiveCamera)))
                    scene.Cam = Construct<PerspectiveCamera>(node);
                else if (IsType(node, typeof(FisheyeCamera)))
                    scene.Cam = Construct<FisheyeCamera>(node);
                else if (IsType(node, typeof(Material)))
                    materials.Add(node.Attributes["id"].Value, Construct<Material>(node));
                else if (IsType(node, typeof(PointLight)))
                    scene.AddLight(Construct<PointLight>(node));
                else if (IsType(node, typeof(DirLight)))
                    scene.AddLight(Construct<DirLight>(node));
                else if (IsType(node, typeof(AreaLight)))
                    scene.AddLight(Construct<AreaLight>(node));
                else if (IsType(node, typeof(Plane)))
                    scene.AddObject(Construct<Plane>(node));
                else if (IsType(node, typeof(CheckerBoard)))
                    scene.AddObject(Construct<CheckerBoard>(node));
                else if (IsType(node, typeof(Sphere)))
                    scene.AddObject(Construct<Sphere>(node));
                else if (IsType(node, typeof(Cube)))
                    scene.AddObject(Construct<Cube>(node));
                else if (IsType(node, typeof(Torus)))
                    scene.AddObject(Construct<Torus>(node));
                else if (IsType(node, typeof(Cylinder)))
                    scene.AddObject(Construct<Cylinder>(node));
                else if (IsType(node, typeof(Cone)))
                    scene.AddObject(Construct<Cone>(node));
                else if (IsType(node, typeof(MaxLinearToneMapper)))
                    scene.AddToneMapper(Construct<MaxLinearToneMapper>(node));
                else if (IsType(node, typeof(NonLinearToneMapper)))
                    scene.AddToneMapper(Construct<NonLinearToneMapper>(node));
                else if (IsType(node, typeof(SchlickToneMapper)))
                    scene.AddToneMapper(Construct<SchlickToneMapper>(node));
                else
                    throw new XmlException($"Unknown node: {node.Name}");
            }

            return scene;
        }

        private static bool IsType(XmlNode n, Type t)
        {
            return string.Equals(n.Name, t.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        private T Construct<T>(XmlNode n)
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
                        throw new XmlException($"Attribute {pi.Name} not found for {typeof(T).Name}.");
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
                    else if (pi.ParameterType == typeof(Single) || pi.ParameterType == typeof(Single?))
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
            if (values.Length != 3 && values.Length != 6)
                throw new XmlException($"Invalid vector '{s}', expected exactly 3 or 6 components.");
            Vec3 origin = new(
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
            if (values.Length != 3)
                throw new XmlException($"Invalid color '{s}', expected exactly 3 components.");
            return new Color(
                Convert.ToSingle(values[0], nfi),
                Convert.ToSingle(values[1], nfi),
                Convert.ToSingle(values[2], nfi));
        }
    }
}
