using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Sirenix.OdinInspector;

namespace ESGI.WFC.Exporter
{
    [CreateAssetMenu(menuName = "WFC/2D/Module Exporter")]
    public class ModuleExporter : ScriptableObject
    {
        public List<ModuleGAN> modules;
        [FolderPath(AbsolutePath = true)]
        public string exportPath;
        
        const string PrefixNormal = "N";
        const string PrefixColor = "C";
        const string PrefixTextureCoordinate = "ST";
        const string PrefixNDimension = "n";
        const string PrefixHomogeneousCoordinate = "4";

        [Button]
        private void GenerateFileModel(string folderName)
        {
            const string meshesFolderName = "meshes";
            var parentFolder = Path.Combine(exportPath, folderName);
            var meshesFolder = Path.Combine(parentFolder, meshesFolderName);
            
            if (!Directory.Exists(parentFolder))
            {
                Directory.CreateDirectory(parentFolder);
            }
            
            if (!Directory.Exists(meshesFolder))
            {
                Directory.CreateDirectory(meshesFolder);
            }

            var sb = new StringBuilder();

            foreach (var module in modules)
            {
                // Write in CSV
                var fileName = module.mesh.name + (module.rotation != 0 ? "_" + module.rotation.ToString(CultureInfo.InvariantCulture) : "") + ".off";
                sb.AppendLine(
                    $"{meshesFolderName}/{fileName};{module.sockets.bottom.id};{module.sockets.right.id};{module.sockets.top.id};{module.sockets.left.id}");

                CreateMeshFile(meshesFolder, module, fileName);
            }
            
            File.WriteAllText(Path.Combine(parentFolder, $"{folderName}.csv"), sb.ToString());
        }

        private void CreateMeshFile(string meshesFolder, ModuleGAN module, string fileName)
        {/*
            var sb = new StringBuilder();

            foreach (var triangle in module.mesh.triangles)
            {
                var vertex = module.mesh.vertices[triangle];
                sb.AppendLine($"{vertex.x};{vertex.y};{vertex.z}");
            }
            
            File.WriteAllText(Path.Combine(meshesFolder, $"{fileName}"), sb.ToString());*/

            Debug.Log($"{fileName}");
            Debug.Log($"POS 0 BEFORE : {module.mesh.vertices[0].ToString("F3")}");
            var mesh = CloneMesh(module);
            Debug.Log($"POS 0 AFTER : {mesh.vertices[0].ToString("F3")}");
            
            MeshToOff(mesh, new StreamWriter(Path.Combine(meshesFolder, $"{fileName}")));
            Debug.Log("------------------------------");
        }

        private Mesh CloneMesh(ModuleGAN module)
        {
            var center =  Vector3.zero;//any V3 you want as the pivot point.
            var newRotation = new Quaternion();
            newRotation.eulerAngles = new Vector3(0,module.rotation,0);//the degrees the vertices are to be rotated, for example (0,90,0) 

            var mesh = Instantiate(module.mesh);
            
            var newmesh = new Mesh();
            var vertices = (Vector3[]) mesh.vertices.Clone();

            for(int i = 0; i < vertices.Length; i++) {//vertices being the array of vertices of your mesh
                vertices[i] = newRotation * (vertices[i] - center) + center;
            }

            newmesh.SetVertices(vertices);
            newmesh.triangles = (int[])mesh.triangles.Clone();
            newmesh.uv = (Vector2[])mesh.uv.Clone();
            newmesh.normals = (Vector3[])mesh.normals.Clone();
            newmesh.colors = mesh.colors;
            newmesh.tangents = mesh.tangents;
            newmesh.RecalculateBounds();
            newmesh.RecalculateNormals();
            

            return newmesh;
        }

        public static void MeshToOff(Mesh mesh, TextWriter off)
        {
            if (mesh.uv.Length != 0)
                off.Write(PrefixTextureCoordinate);
            if (mesh.colors.Length != 0)
                off.Write(PrefixColor);
            if (mesh.normals.Length != 0)
                off.Write(PrefixNormal);
            off.WriteLine("OFF");

            var verts = mesh.vertices;
            var norms = mesh.normals;
            var colors = mesh.colors;
            var uvs = mesh.uv;
            uvs = new Vector2[0];
            colors = new Color[0];
            var tris = mesh.triangles;
            var faceCount = tris.Length / 3;

            off.WriteLine(string.Format("{0} {1} {2}", verts.Length, faceCount, 0));

            for (int i = 0; i < verts.Length; i++)
            {
                off.Write(verts[i].x);
                off.Write(" ");
                off.Write(verts[i].y);
                off.Write(" ");
                off.Write(verts[i].z);
                if (norms.Length != 0)
                {
                    off.Write(" ");
                    off.Write(norms[i].x);
                    off.Write(" ");
                    off.Write(norms[i].y);
                    off.Write(" ");
                    off.Write(norms[i].z);
                }
                if (colors.Length != 0)
                {
                    off.Write(" ");
                    off.Write(colors[i].r);
                    off.Write(" ");
                    off.Write(colors[i].g);
                    off.Write(" ");
                    off.Write(colors[i].b);
                    off.Write(" ");
                    off.Write(colors[i].a);
                }
                if (uvs.Length != 0)
                {
                    off.Write(" ");
                    off.Write(uvs[i].x);
                    off.Write(" ");
                    off.Write(uvs[i].y);
                }
                off.WriteLine();
            }

            for(int i = 0; i < faceCount; i++)
            {
                off.Write("3 ");
                off.Write(tris[i * 3]);
                off.Write(" ");
                off.Write(tris[i * 3 + 1]);
                off.Write(" ");
                off.WriteLine(tris[i * 3 + 2]);
            }
            off.Close();
        }
    }
}
