using RayTracer.Common;
using RayTracer.Filters;
using RayTracer.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace RayTracer.Composition
{
    /// <summary>
    /// Scene loader from file
    /// </summary>
    public static class SceneBuilder
    {
        private static readonly NumberFormatInfo nfi = CultureInfo.InvariantCulture.NumberFormat;
        public static Scene FromXML(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            int scenew = 1080;
            int sceneh = 720;
            Vec3 cameye = new Vec3(1, 1, 1);
            Vec3 camlookat = new Vec3(0, 0, 0);
            float camhfov = 50;
            int spp = 1;
            int dofs = 1;
            float dofr = 0;

            XmlNode sceneNode = doc.ChildNodes[0];
            if (sceneNode.Attributes != null)
            {
                if (sceneNode.Attributes["w"] != null) scenew = Convert.ToInt32(sceneNode.Attributes["w"].Value);
                if (sceneNode.Attributes["h"] != null) sceneh = Convert.ToInt32(sceneNode.Attributes["h"].Value);
                if (sceneNode.Attributes["cameye"] != null) cameye = Vec3FromString(sceneNode.Attributes["cameye"].Value);
                if (sceneNode.Attributes["camlookat"] != null) camlookat = Vec3FromString(sceneNode.Attributes["camlookat"].Value);
                if (sceneNode.Attributes["camhfov"] != null) camhfov = Convert.ToSingle(sceneNode.Attributes["camhfov"].Value, nfi);
                if (sceneNode.Attributes["samplesperpixel"] != null) spp = Convert.ToInt32(sceneNode.Attributes["samplesperpixel"].Value);
                if (sceneNode.Attributes["dofsamples"] != null) dofs = Convert.ToInt32(sceneNode.Attributes["dofsamples"].Value);
                if (sceneNode.Attributes["dofradius"] != null) dofr = Convert.ToSingle(sceneNode.Attributes["dofradius"].Value, nfi);
            }
            Scene scene = new Scene(scenew, sceneh, new Camera(cameye, camlookat, camhfov * MathF.PI / 180, scenew, sceneh), spp, dofs, dofr);
            Dictionary<string, Material> materials = new Dictionary<string, Material>();

            foreach (XmlNode node in sceneNode.ChildNodes)
            {
                if (node.Name == "material")
                {
                    string id = node.Attributes["id"].Value;
                    float rough = node.Attributes["rough"] == null ? 1 : Convert.ToSingle(node.Attributes["rough"].Value, nfi);
                    Color ambient = node.Attributes["ambient"] == null ?
                        new Color(0, 0, 0) : ColorFromString(node.Attributes["ambient"].Value);
                    Color diffuse = node.Attributes["diffuse"] == null ?
                        new Color(0, 0, 0) : ColorFromString(node.Attributes["diffuse"].Value);
                    Color specular = node.Attributes["specular"] == null ?
                        new Color(0, 0, 0) : ColorFromString(node.Attributes["specular"].Value);
                    float shine = node.Attributes["shine"] == null ? 0 : Convert.ToSingle(node.Attributes["shine"].Value, nfi);

                    float smooth = node.Attributes["smooth"] == null ? 0 : Convert.ToSingle(node.Attributes["smooth"].Value, nfi);
                    Color n = node.Attributes["n"] == null ?
                        new Color(0, 0, 0) : ColorFromString(node.Attributes["n"].Value);
                    Color kap = node.Attributes["kap"] == null ?
                        new Color(0, 0, 0) : ColorFromString(node.Attributes["kap"].Value);
                    bool isReflective = node.Attributes["isreflective"] == null ? false : Convert.ToBoolean(node.Attributes["isreflective"].Value);
                    bool isRefractive = node.Attributes["isrefractive"] == null ? false : Convert.ToBoolean(node.Attributes["isrefractive"].Value);
                    float blur = node.Attributes["blur"] == null ? 0 : Convert.ToSingle(node.Attributes["blur"].Value, nfi);
                    int blursamples = node.Attributes["blursamples"] == null ? 1 : Convert.ToInt32(node.Attributes["blursamples"].Value);

                    materials.Add(id, new Material(rough, ambient, diffuse, specular, shine, smooth, isReflective, isRefractive, n, kap, blur, blursamples));
                }
                else if (node.Name == "pointlight")
                {
                    scene.AddLight(new PointLight(
                        Vec3FromString(node.Attributes["pos"].Value),
                        ColorFromString(node.Attributes["lum"].Value)));
                }
                else if (node.Name == "arealight")
                {
                    scene.AddLight(new AreaLight(
                        Vec3FromString(node.Attributes["pos"].Value),
                        ColorFromString(node.Attributes["lum"].Value),
                        Convert.ToSingle(node.Attributes["radius"].Value, nfi),
                        Convert.ToInt32(node.Attributes["samples"].Value)));
                }
                else if (node.Name == "plane")
                {
                    scene.AddObject(new Plane(
                        Vec3FromString(node.Attributes["center"].Value),
                        Vec3FromString(node.Attributes["normal"].Value),
                        materials[node.Attributes["material"].Value]));
                }
                else if (node.Name == "checkerboard")
                {
                    scene.AddObject(new CheckerBoard(
                        Vec3FromString(node.Attributes["center"].Value),
                        Vec3FromString(node.Attributes["normal"].Value),
                        Vec3FromString(node.Attributes["matdir"].Value),
                        materials[node.Attributes["mat1"].Value],
                        materials[node.Attributes["mat2"].Value]));
                }
                else if (node.Name == "sphere")
                {
                    scene.AddObject(new Sphere(
                        Vec3FromString(node.Attributes["center"].Value),
                        Convert.ToSingle(node.Attributes["radius"].Value, nfi),
                        materials[node.Attributes["material"].Value]
                        ));
                }
                else if (node.Name == "cube")
                {
                    scene.AddObject(new Cube(
                        Vec3FromString(node.Attributes["center"].Value),
                        Convert.ToSingle(node.Attributes["side"].Value, nfi),
                        materials[node.Attributes["material"].Value]
                        ));
                }
                else if (node.Name == "torus")
                {
                    scene.AddObject(new Torus(
                        Vec3FromString(node.Attributes["center"].Value),
                        Convert.ToSingle(node.Attributes["ro"].Value, nfi),
                        Convert.ToSingle(node.Attributes["ri"].Value, nfi),
                        Convert.ToInt32(node.Attributes["tessu"].Value),
                        Convert.ToInt32(node.Attributes["tessv"].Value),
                        Convert.ToBoolean(node.Attributes["shadingnormals"].Value),
                        materials[node.Attributes["material"].Value]));
                }
                else if (node.Name == "cylinder")
                {
                    scene.AddObject(new Cylinder(
                        Vec3FromString(node.Attributes["cap1center"].Value),
                        Vec3FromString(node.Attributes["cap2center"].Value),
                        Convert.ToSingle(node.Attributes["radius"].Value, nfi),
                        materials[node.Attributes["material"].Value]
                        ));
                }
                else if (node.Name == "cone")
                {
                    scene.AddObject(new Cone(
                        Vec3FromString(node.Attributes["cap1center"].Value),
                        Vec3FromString(node.Attributes["cap2center"].Value),
                        Convert.ToSingle(node.Attributes["cap1radius"].Value, nfi),
                        Convert.ToSingle(node.Attributes["cap2radius"].Value, nfi),
                        materials[node.Attributes["material"].Value]
                        ));
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
                }
                else
                {
                    throw new XmlException("Unknown node: " + node.Name);
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
