// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pcx
{
    [ScriptedImporter(1, "xyz")]
    class XyzImporter : ScriptedImporter
    {
        #region ScriptedImporter implementation

        public enum ContainerType { Mesh, ComputeBuffer, Texture  }

        [SerializeField] ContainerType _containerType = ContainerType.Mesh;

        [SerializeField] private int _maxPoints = 10000;

        public override void OnImportAsset(AssetImportContext context)
        {
            if (_containerType == ContainerType.Mesh)
            {
                // Mesh container
                // Create a prefab with MeshFilter/MeshRenderer.
                var gameObject = new GameObject();
                var mesh = ImportAsMesh(context.assetPath);

                var meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;

                var meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = GetDefaultMaterial();

                context.AddObjectToAsset("prefab", gameObject);
                if (mesh != null) context.AddObjectToAsset("mesh", mesh);

                context.SetMainObject(gameObject);
            }
            else if (_containerType == ContainerType.ComputeBuffer)
            {
                // ComputeBuffer container
                // Create a prefab with PointCloudRenderer.
                var gameObject = new GameObject();
                var data = ImportAsPointCloudData(context.assetPath);

                var renderer = gameObject.AddComponent<PointCloudRenderer>();
                renderer.sourceData = data;

                context.AddObjectToAsset("prefab", gameObject);
                if (data != null) context.AddObjectToAsset("data", data);

                context.SetMainObject(gameObject);
            }
            else // _containerType == ContainerType.Texture
            {
                // Texture container
                // No prefab is available for this type.
                var data = ImportAsBakedPointCloud(context.assetPath);
                if (data != null)
                {
                    context.AddObjectToAsset("container", data);
                    context.AddObjectToAsset("position", data.positionMap);
                    context.AddObjectToAsset("color", data.colorMap);
                    context.SetMainObject(data);
                }
            }
        }

        #endregion

        #region Internal utilities

        static Material GetDefaultMaterial()
        {
            // Via package manager
            var path_upm = "Packages/jp.keijiro.pcx/Editor/Default Point.mat";
            // Via project asset database
            var path_prj = "Assets/Pcx/Editor/Default Point.mat";
            return AssetDatabase.LoadAssetAtPath<Material>(path_upm) ??
                   AssetDatabase.LoadAssetAtPath<Material>(path_prj);
        }

        #endregion

        #region Internal data structure


        class XyzPointCloud
        {
            public readonly List<Vector3> points = new List<Vector3>();
            public readonly List<Color32> colors = new List<Color32>();

            public int totalPoints;
        }


        #endregion

        #region Reader implementation

        Mesh ImportAsMesh(string path)
        {
            try
            {
                var pc = ImportFile(path);

                var mesh = new Mesh
                {
                    name = Path.GetFileNameWithoutExtension(path),
                    indexFormat = pc.totalPoints > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
                };
                
                mesh.SetVertices(pc.points);
                mesh.SetColors(pc.colors);

                mesh.SetIndices(
                    Enumerable.Range(0, pc.totalPoints).ToArray(),
                    MeshTopology.Points, 0
                );

                mesh.UploadMeshData(true);
                return mesh;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing " + path + ". " + e.Message);
                return null;
            }
        }

        PointCloudData ImportAsPointCloudData(string path)
        {
            try
            {
                var pc = ImportFile(path);
                var data = ScriptableObject.CreateInstance<PointCloudData>();
                data.Initialize(pc.points, pc.colors);
                data.name = Path.GetFileNameWithoutExtension(path);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing " + path + ". " + e.Message);
                return null;
            }
        }

        BakedPointCloud ImportAsBakedPointCloud(string path)
        {
            try
            {
                var pc = ImportFile(path);
                var data = ScriptableObject.CreateInstance<BakedPointCloud>();
                data.Initialize(pc.points, pc.colors);
                data.name = Path.GetFileNameWithoutExtension(path);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing " + path + ". " + e.Message);
                return null;
            }
        }



        
        private XyzPointCloud ImportFile(string pointCloudPath)
        {
            var pc = new XyzPointCloud();
            var file = new StreamReader(pointCloudPath);
            string line;
            int counter = 0;
            //int maxCount = 1000;
            
            while((line = file.ReadLine()) != null)  
            {  
                //Debug.Log(line);
                var lineParts = line.Split(' ');
                var pos = new Vector3(float.Parse(lineParts[1]), float.Parse(lineParts[2]), float.Parse(lineParts[0]));
                Color32 color = new Color32(byte.Parse(lineParts[3]), byte.Parse(lineParts[4]), byte.Parse(lineParts[5]), 1);
                pc.points.Add(pos);
                pc.colors.Add(color);

                // Debug.Log(color);
                pc.totalPoints = pc.points.Count;
                
                counter++;  
            
                if(counter > _maxPoints) break;
                ;
            }

            //ApplyToParticleSystem();
            return pc;
        }

    }

    #endregion
}
