using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UnityAssimp {

    public static class ImportAssimp {
        public static bool saveAssets = true;

        [MenuItem("Assets/Djoker Tools/Assimp/ImportStatic")]
        static void init() {

            string filename = Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject));
            string rootPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));

            readMesh(rootPath, filename, "");
        }

        private static void trace(string msg) {
        }

        private class AssimpMesh {
            public Mesh geometry;
            public string name;
            public Material material;
            public List<Vector3> vertices;
            public List<Vector3> normals;
            public List<Vector4> tangents;
            public List<Vector2> uvcoords;
            public List<int> faces;
            public GameObject meshContainer;
            public MeshFilter meshFilter;
            public MeshRenderer meshRenderer;
            public GameObject Root;

            public AssimpMesh(GameObject parent, GameObject root, string name) {
                this.name = name;
                meshContainer = new GameObject(name);

                this.Root = root;
                meshContainer.transform.parent = parent.transform;

                meshContainer.AddComponent<MeshFilter>();
                meshContainer.AddComponent<MeshRenderer>();
                meshFilter = meshContainer.GetComponent<MeshFilter>();
                meshRenderer = meshContainer.GetComponent<MeshRenderer>();

                meshFilter.sharedMesh = new Mesh();
                geometry = meshFilter.sharedMesh;
                vertices = new List<Vector3>();
                normals = new List<Vector3>();
                tangents = new List<Vector4>();
                uvcoords = new List<Vector2>();
                faces = new List<int>();
            }

            public void addVertex(Vector3 pos, Vector3 normal, Vector2 uv, Vector4 tan) {
                vertices.Add(pos);
                normals.Add(normal);
                uvcoords.Add(uv);
                tangents.Add(tan);
            }

            public void setmaterial(Material mat) {
                meshRenderer.sharedMaterial = mat;
            }

            public void addFace(int a, int b, int c) {
                faces.Add(a);
                faces.Add(b);
                faces.Add(c);
            }

            public void build() {
                geometry.vertices = vertices.ToArray();
                geometry.normals = normals.ToArray();
                geometry.uv = uvcoords.ToArray();
                //geometry.u = uvcoords.ToArray();
                geometry.triangles = faces.ToArray();
                geometry.tangents = tangents.ToArray();
                Unwrapping.GenerateSecondaryUVSet(geometry);
                //geometry.RecalculateNormals();
                geometry.RecalculateBounds();
                //TangentSolver(geometry);
                ;
            }

            public void dispose() {
                vertices.Clear();
                normals.Clear();
                faces.Clear();
                uvcoords.Clear();
                tangents.Clear();
            }            
        }

        public static void readMesh(string path, string filename, string texturepath) {
            string importingAssetsDir;

            if (File.Exists(path + "/" + filename)) {
                Assimp.PostProcessSteps flags = (
                    //        Assimp.PostProcessSteps.MakeLeftHanded |

                    Assimp.PostProcessSteps.OptimizeMeshes |
                    Assimp.PostProcessSteps.OptimizeGraph |
                    Assimp.PostProcessSteps.RemoveRedundantMaterials |
                    Assimp.PostProcessSteps.SortByPrimitiveType |
                    Assimp.PostProcessSteps.SplitLargeMeshes |
                    Assimp.PostProcessSteps.Triangulate |
                    Assimp.PostProcessSteps.CalculateTangentSpace |
                    Assimp.PostProcessSteps.GenerateUVCoords |
                    Assimp.PostProcessSteps.GenerateSmoothNormals |
                    Assimp.PostProcessSteps.RemoveComponent |
                    Assimp.PostProcessSteps.JoinIdenticalVertices
                );

                IntPtr config = Assimp.aiCreatePropertyStore();

                Assimp.aiSetImportPropertyFloat(config, Assimp.AI_CONFIG_PP_CT_MAX_SMOOTHING_ANGLE, 60.0f);

                // IntPtr scene = Assimp.aiImportFile(path + "/" + filename, (uint)flags);
                IntPtr scene = Assimp.aiImportFileWithProperties(path + "/" + filename, (uint)flags, config);
                Assimp.aiReleasePropertyStore(config);
                if (scene == null) {
                    Debug.LogWarning("failed to read file: " + path + "/" + filename);
                    return;
                } else {
                    string nm = Path.GetFileNameWithoutExtension(filename);

                    importingAssetsDir = "Assets/Prefabs/" + nm + "/";

                    if (saveAssets) {
                        if (!Directory.Exists(importingAssetsDir)) {
                            Directory.CreateDirectory(importingAssetsDir);
                        }
                        AssetDatabase.Refresh();
                    }

                    GameObject ObjectRoot = new GameObject(nm);
                    GameObject meshContainer = new GameObject(nm + "_Mesh");
                    meshContainer.transform.parent = ObjectRoot.transform;

                    List<Material> materials = new List<Material>();
                    List<AssimpMesh> MeshList = new List<AssimpMesh>();

                    for (int i = 0; i < Assimp.aiScene_GetNumMaterials(scene); i++) {
                        string matName = Assimp.aiMaterial_GetName(scene, i);
                        matName = nm + "_mat" + i;

                        //  string fname = Path.GetFileNameWithoutExtension(Assimp.aiMaterial_GetTexture(scene, i, (int)Assimp.TextureType.Diffuse));
                        string fname = Path.GetFileName(Assimp.aiMaterial_GetTexture(scene, i, (int)Assimp.TextureType.Diffuse));
                        Debug.Log("texture " + fname + "Material :" + matName);

                        Color ambient = Assimp.aiMaterial_GetAmbient(scene, i);
                        Color diffuse = Assimp.aiMaterial_GetDiffuse(scene, i);
                        Color specular = Assimp.aiMaterial_GetSpecular(scene, i);
                        Color emissive = Assimp.aiMaterial_GetEmissive(scene, i);

                        Material mat = new Material(Shader.Find("Diffuse"));
                        mat.name = matName;

                        string texturename = path + "/" + fname;

                        Texture2D tex = Utils.loadTex(texturename);
                        
                        //Texture2D tex = Resources.Load(texturename) as Texture2D;
                        //Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texturename, typeof(Texture2D));
                        if (tex != null) {
                            Debug.Log("LOAD (" + texturename + ") texture");
                            mat.SetTexture("_MainTex", tex);
                        } else {
                            Debug.LogError("Fail LOAD (" + texturename + ") error");
                        }

                        if (saveAssets) {
                            string materialAssetPath = AssetDatabase.GenerateUniqueAssetPath(importingAssetsDir + mat.name + ".asset");
                            AssetDatabase.CreateAsset(mat, materialAssetPath);
                        }
                        materials.Add(mat);
                    }

                    AssetDatabase.Refresh();

                    if (Assimp.aiScene_HasMeshes(scene)) {
                        for (int i = 0; i < Assimp.aiScene_GetNumMeshes(scene); i++) {
                            string name = "Mesh_";
                            name += i.ToString();

                            bool HasNormals = Assimp.aiMesh_HasNormals(scene, i);
                            bool HasTexCoord = Assimp.aiMesh_HasTextureCoords(scene, i, 0);
                            bool HasFaces = Assimp.aiMesh_HasFaces(scene, i);

                            AssimpMesh mesh = new AssimpMesh(meshContainer, ObjectRoot, name);
                            mesh.setmaterial(materials[Assimp.aiMesh_GetMaterialIndex(scene, i)]);
                            MeshList.Add(mesh);

                            for (int v = 0; v < Assimp.aiMesh_GetNumVertices(scene, i); v++) {
                                Vector3 vertex = Assimp.aiMesh_Vertex(scene, i, v);
                                Vector3 n = Assimp.aiMesh_Normal(scene, i, v);
                                float x = Assimp.aiMesh_TextureCoordX(scene, i, v, 0);
                                float y = Assimp.aiMesh_TextureCoordY(scene, i, v, 0);

                                Vector3 binormalf = Assimp.aiMesh_Bitangent(scene, i, v);
                                Vector3 tangentf = Assimp.aiMesh_Tangent(scene, i, v);
                                Vector4 outputTangent = new Vector4(tangentf.x, tangentf.y, tangentf.z, 0.0F);

                                float dp = Vector3.Dot(Vector3.Cross(n, tangentf), binormalf);
                                if (dp > 0.0F) {
                                    outputTangent.w = 1.0F;
                                } else {
                                    outputTangent.w = -1.0F;
                                }

                                mesh.addVertex(vertex, n, new Vector2(x, y), outputTangent);
                                //mesh.addVertex(vertex, new Vector3(1 * -n.x, n.y, n.z), new Vector2(x, y), outputTangent);
                            }

                            for (int f = 0; f < Assimp.aiMesh_GetNumFaces(scene, i); f++) {
                                int a = Assimp.aiMesh_Indice(scene, i, f, 0);
                                int b = Assimp.aiMesh_Indice(scene, i, f, 1);
                                int c = Assimp.aiMesh_Indice(scene, i, f, 2);
                                mesh.addFace(a, b, c);
                            }

                            //**********
                            mesh.build();
                            if (saveAssets) {
                                string meshAssetPath = AssetDatabase.GenerateUniqueAssetPath(importingAssetsDir + mesh.name + ".asset");
                                AssetDatabase.CreateAsset(mesh.geometry, meshAssetPath);
                            }

                            mesh.dispose();
                        }
                    }

                    if (saveAssets) {

                        string prefabPath = AssetDatabase.GenerateUniqueAssetPath(importingAssetsDir + filename + ".prefab");
                        var prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
                        PrefabUtility.ReplacePrefab(ObjectRoot, prefab, ReplacePrefabOptions.ConnectToPrefab);
                        AssetDatabase.Refresh();
                    }

                    MeshList.Clear();
                }
                Assimp.aiReleaseImport(scene);
                Debug.LogWarning(path + "/" + filename + " Imported ;) ");
            }
        }
    }
}