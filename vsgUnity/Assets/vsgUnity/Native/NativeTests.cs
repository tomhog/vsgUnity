
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace vsgUnity.Native
{
    public static class OrderedDictionaryExtensions
    {
        public static int IndexOfKey(this OrderedDictionary dictionary, object key)
        {
            IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
            int index = 0;
            while (enumerator.MoveNext())
            {
                if (enumerator.Key == key)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
    }

    public static class NativeTests
    {
        [DllImport("unity2vsgd", EntryPoint = "unity2vsg_Tests_GetXValues")]
        private static extern NativeFloatArray unity2vsg_Tests_GetXValues(Vec2Array points);

        public static float[] GetXValues(UnityEngine.Vector2[] points)
        {
            NativeFloatArray nativefloats = unity2vsg_Tests_GetXValues(Convert.FromLocal(points));
            FloatArray floatarray = Convert.FromNative(nativefloats);
            Memory.DeleteNativeObject(nativefloats.ptr, true);
            return floatarray.data;
        }


        //
        // Convert a mesh

        [DllImport("unity2vsgd", EntryPoint = "unity2vsg_ConvertMesh")]
        private static extern void unity2vsg_ConvertMesh(Mesh mesh);

        public static void ConvertMesh(UnityEngine.Mesh umesh)
        {
            Mesh mesh = new Mesh();
            mesh.verticies = new Vec3Array();
            mesh.verticies.data = umesh.vertices;
            mesh.verticies.length = (uint)umesh.vertexCount;

            mesh.triangles = new IntArray();
            mesh.triangles.data = umesh.triangles;
            mesh.triangles.length = (uint)mesh.triangles.data.Length;

            mesh.normals = new Vec3Array();
            mesh.normals.data = umesh.normals;
            mesh.normals.length = (uint)umesh.vertexCount;

            unity2vsg_ConvertMesh(mesh);
        }

        //
        // Convert a mesh

        [DllImport("unity2vsgd", EntryPoint = "unity2vsg_ExportScene")]
        private static extern void unity2vsg_ExportScene(ExportScene exportScene);

        public class ExportSceneBuilder
        {
            public ExportSceneBuilder(string exportPath, UnityEngine.GameObject gameObject = null)
            {
                //_exportScene.exportPath = exportPath;

                // create the root node
                _exportScene.root = new SceneNode();
                _exportScene.root.type = (int)NodeType.GROUP;

                // gather root gameobjects from scene or passed to builder
                List<UnityEngine.GameObject> rootObjects = new List<UnityEngine.GameObject>();
                if(gameObject != null)
                {
                    rootObjects.Add(gameObject);
                }
                else
                {
                    rootObjects.AddRange(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());
                }

                _exportScene.root.children.length = (uint)rootObjects.Count;
                _exportScene.root.children.data = new SceneNode[_exportScene.root.children.length];

                for(int i = 0; i < rootObjects.Count; i++)
                {
                    SceneNode childnode = new SceneNode();
                    processGameObject(rootObjects[i], ref childnode);
                    _exportScene.root.children.data[i] = childnode;
                }

                // set the meshes list now all nodes are processed
                _exportScene.meshes.length = (uint)_meshes.Count;
                _exportScene.meshes.data = new Mesh[_exportScene.meshes.length];

                IDictionaryEnumerator enumerator = _meshes.GetEnumerator();
                int index = 0;
                while (enumerator.MoveNext())
                {
                    _exportScene.meshes.data[index] = (Mesh)enumerator.Value;
                    index++;
                }
            }

            public void processGameObject(UnityEngine.GameObject gameObject, ref SceneNode node)
            {
                UnityEngine.Transform goTransform = gameObject.transform;

                //node.name = gameObject.name;

                // figure out the node type and split if needed, any split nodes become a child of this node
                List<SceneNode> splitNodes = new List<SceneNode>();

                // does it have a mesh
                UnityEngine.MeshFilter meshFilter = gameObject.GetComponent<UnityEngine.MeshFilter>();
                UnityEngine.MeshRenderer meshRenderer = gameObject.GetComponent<UnityEngine.MeshRenderer>();
                if(meshFilter && meshRenderer)
                {
                    // create an additional mesh node
                    SceneNode meshNode = new SceneNode();
                    //meshNode.name = node.name + "-Mesh";
                    meshNode.type = (int)NodeType.MESH;
                    meshNode.meshID = GetOrCreateMeshID(meshFilter.mesh);
                    splitNodes.Add(meshNode);
                }

                // is it just a group or does it have a non identity local matrix (a transform node)
                node.type = (int)NodeType.GROUP;

                UnityEngine.Vector3 localpos = goTransform.localPosition;
                UnityEngine.Quaternion localrot = goTransform.localRotation;
                UnityEngine.Vector3 localscale = goTransform.localScale;
                if(localpos != UnityEngine.Vector3.zero || localrot != UnityEngine.Quaternion.identity || localscale != UnityEngine.Vector3.one)
                {
                    node.type = (int)NodeType.TRANSFORM;
                    UnityEngine.Matrix4x4 matrix = UnityEngine.Matrix4x4.TRS(localpos, localrot, localscale);
                    node.matrix.data = new float[]
                    {
                        matrix.m00, matrix.m01, matrix.m02, matrix.m03,
                        matrix.m10, matrix.m11, matrix.m12, matrix.m13,
                        matrix.m20, matrix.m21, matrix.m22, matrix.m23,
                        matrix.m30, matrix.m31, matrix.m32, matrix.m33
                    };
                    node.matrix.length = (uint)node.matrix.data.Length;
                }

                // add children
                node.children.length = (uint)goTransform.childCount;// + splitNodes.Count;
                node.children.data = new SceneNode[node.children.length];

                for(int i = 0; i < goTransform.childCount; i++)
                {
                    SceneNode childnode = new SceneNode();
                    processGameObject(goTransform.GetChild(i).gameObject, ref childnode);
                    node.children.data[i] = childnode;
                }

                // add any split nodes
                for(int i = 0; i < splitNodes.Count; i++)
                {
                    //node.children.data[i + goTransform.childCount] = splitNodes[i];
                }
                UnityEngine.Debug.Log("processGameObject " + gameObject.name + " complete");
            }

            public uint GetOrCreateMeshID(UnityEngine.Mesh umesh)
            {
                int existingIndex = _meshes.IndexOfKey(umesh.GetInstanceID());
                if (existingIndex != -1) return (uint)existingIndex;

                // create a new mesh
                Mesh mesh = new Mesh();
                mesh.verticies = new Vec3Array();
                mesh.verticies.data = umesh.vertices;
                mesh.verticies.length = (uint)umesh.vertexCount;

                mesh.triangles = new IntArray();
                mesh.triangles.data = umesh.triangles;
                mesh.triangles.length = (uint)mesh.triangles.data.Length;

                mesh.normals = new Vec3Array();
                mesh.normals.data = umesh.normals;
                mesh.normals.length = (uint)umesh.vertexCount;

                int index = _meshes.Count;
                _meshes.Add(umesh.GetInstanceID(), mesh);
                return (uint)index;
            }

            // export scene we're building to pass to native vsg exporter
            public ExportScene _exportScene = new ExportScene();

            // dynamic list to collect all meshes in export graph
            public OrderedDictionary _meshes = new OrderedDictionary();
        }

        public static void ExportScene(string exportPath, UnityEngine.GameObject gameObject = null)
        {
            ExportSceneBuilder sceneBuilder = new ExportSceneBuilder(exportPath, gameObject);
            string asjson = UnityEngine.JsonUtility.ToJson(sceneBuilder._exportScene, true);
            System.IO.File.WriteAllText("C:\\Work\\VSG\\exportscene.json", asjson);
            //UnityEngine.Debug.Log(asjson);
            UnityEngine.Debug.Log("Size of c# scenenode: " + Marshal.SizeOf(sceneBuilder._exportScene.root));
            unity2vsg_ExportScene(sceneBuilder._exportScene);
            sceneBuilder = null;
        }
    }
}