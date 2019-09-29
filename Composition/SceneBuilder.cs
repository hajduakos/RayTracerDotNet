using RayTracer.Common;
using RayTracer.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace RayTracer.Composition
{
    public class SceneBuilder
    {
        private static NumberFormatInfo nfi = CultureInfo.InvariantCulture.NumberFormat;
        public static Scene FromXML(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            int scenew = 1080;
            int sceneh = 720;
            Vec3 cameye = new Vec3(1, 1, 1);
            Vec3 camlookat = new Vec3(0, 0, 0);
            float camhfov = 50;

            XmlNode sceneNode = doc.ChildNodes[0];
            if (sceneNode.Attributes != null)
            {
                if (sceneNode.Attributes["w"] != null) scenew = Convert.ToInt32(sceneNode.Attributes["w"].Value);
                if (sceneNode.Attributes["h"] != null) sceneh = Convert.ToInt32(sceneNode.Attributes["h"].Value);
                if (sceneNode.Attributes["cameye"] != null) cameye = Vec3FromString(sceneNode.Attributes["cameye"].Value);
                if (sceneNode.Attributes["camlookat"] != null) camlookat = Vec3FromString(sceneNode.Attributes["camlookat"].Value);
                if (sceneNode.Attributes["camhfov"] != null) camhfov = Convert.ToSingle(sceneNode.Attributes["camhfov"].Value, nfi);
            }
            Scene scene = new Scene(scenew, sceneh, new Camera(cameye, camlookat, camhfov * MathF.PI / 180, scenew, sceneh));
            Dictionary<string, Material> materials = new Dictionary<string, Material>();

            foreach (XmlNode node in sceneNode.ChildNodes)
            {
                if (node.Name == "material")
                {
                    string id = node.Attributes["id"].Value;
                    Color ambient = node.Attributes["ambient"] == null ?
                        new Color(0, 0, 0) : ColorFromString(node.Attributes["ambient"].Value);
                    Color diffuse = node.Attributes["diffuse"] == null ?
                        new Color(0, 0, 0) : ColorFromString(node.Attributes["diffuse"].Value);
                    Color specular = node.Attributes["specular"] == null ?
                        new Color(0, 0, 0) : ColorFromString(node.Attributes["specular"].Value);
                    float shine = node.Attributes["shine"] == null ? 0 : Convert.ToSingle(node.Attributes["shine"].Value, nfi);
                    materials.Add(id, new Material(ambient, diffuse, specular, shine));
                }
                else if (node.Name == "light")
                {
                    scene.AddLight(new PointLight(
                        Vec3FromString(node.Attributes["pos"].Value),
                        ColorFromString(node.Attributes["lum"].Value)));
                }
                else if (node.Name == "planexy")
                {
                    scene.AddObject(new PlaneXY(materials[node.Attributes["material"].Value]));
                }
                else if (node.Name == "sphere")
                {
                    scene.AddObject(new Sphere(
                        Vec3FromString(node.Attributes["center"].Value),
                        Convert.ToSingle(node.Attributes["radius"].Value, nfi),
                        materials[node.Attributes["material"].Value]
                        ));
                }
                else
                {
                    throw new Exception("Unknown node: " + node.Name);
                }
            }
            
            return scene;
        }

        private static Vec3 Vec3FromString(string s)
        {
            string[] values = s.Split(" ");
            return new Vec3(
                Convert.ToSingle(values[0], nfi),
                Convert.ToSingle(values[1], nfi),
                Convert.ToSingle(values[2], nfi));
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
