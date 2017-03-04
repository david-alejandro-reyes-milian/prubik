
using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Assets
{
    public class CreateQuad : ScriptableWizard
    {
        public int QuadsCount = 1;
        public float Width = 10f, Height = 10f;
        public string MeshName = "Quad";
        public string GameObjectName = "Quad";
        public string AssetsFolder = "Assets";




        public enum AnchorPoint
        {
            TopLeft,
            TopMiddle,
            TopRight,
            RightMiddle,
            BottomRight,
            BottomMiddle,
            BottomLeft,
            LeftMiddle,
            Center,
            Custom
        }
        public AnchorPoint Anchor = AnchorPoint.Center;
        //Horz Position of Anchor on Plane
        public float AnchorX = 0.5f;
        //Vert Position of Anchor on Plane
        public float AnchorY = 0.5f;




        [MenuItem("GameObject/2D Object/Quad")]
        private static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard("Create Quad", typeof(CreateQuad));


        }

        private void GetFolderSelection()
        {
            if (Selection.objects != null && Selection.objects.Length == 1)
                AssetsFolder = AssetDatabase.GetAssetPath(Selection.objects[0]);
        }

        private void OnInspectorUpdate()
        {
            switch (Anchor)
            {
                case AnchorPoint.TopLeft:
                    AnchorX = 0.0f * Width;
                    AnchorY = 1.0f * Height;
                    break;
                //Anchor is set to top-middle
                case AnchorPoint.TopMiddle:
                    AnchorX = 0.5f * Width;
                    AnchorY = 1.0f * Height;
                    break;
                //Anchor is set to top-right
                case AnchorPoint.TopRight:
                    AnchorX = 1.0f * Width;
                    AnchorY = 1.0f * Height;
                    break;
                //Anchor is set to right-middle
                case AnchorPoint.RightMiddle:
                    AnchorX = 1.0f * Width;
                    AnchorY = 0.5f * Height;
                    break;
                //Anchor is set to Bottom-Right
                case AnchorPoint.BottomRight:
                    AnchorX = 1.0f * Width;
                    AnchorY = 0.0f * Height;
                    break;
                //Anchor is set to Bottom-Middle
                case AnchorPoint.BottomMiddle:
                    AnchorX = 0.5f * Width;
                    AnchorY = 0.0f * Height;
                    break;
                //Anchor is set to Bottom-Left
                case AnchorPoint.BottomLeft:
                    AnchorX = 0.0f * Width;
                    AnchorY = 0.0f * Height;
                    break;
                //Anchor is set to Left-Middle
                case AnchorPoint.LeftMiddle:
                    AnchorX = 0.0f * Width;
                    AnchorY = 0.5f * Height;
                    break;
                //Anchor is set to center
                case AnchorPoint.Center:
                    AnchorX = 0.5f * Width;
                    AnchorY = 0.5f * Height;
                    break;
                case AnchorPoint.Custom:
                default:
                    break;
            }
        }

        void OnWizardCreate()
        {
            Vector3[] Vertices = new Vector3[4];
            Vector2[] UVs = new Vector2[4];
            int[] Triangles = new int[6];
            for (int i = 0; i < QuadsCount; i++)
            {

                Vertices[0].x = -AnchorX;
                Vertices[0].y = -AnchorY;
                //Bottom-right
                Vertices[1].x = Vertices[0].x + Width;
                Vertices[1].y = Vertices[0].y;
                //Top-left
                Vertices[2].x = Vertices[0].x;
                Vertices[2].y = Vertices[0].y + Height;
                //Top-right
                Vertices[3].x = Vertices[0].x + Width;
                Vertices[3].y = Vertices[0].y + Height;

                //Assign UVs
                //Bottom-left
                UVs[0].x = 0.0f;
                UVs[0].y = 0.0f;
                //Bottom-right
                UVs[1].x = 1.0f;
                UVs[1].y = 0.0f;
                //Top-left
                UVs[2].x = 0.0f;
                UVs[2].y = 1.0f;
                //Top-right
                UVs[3].x = 1.0f;
                UVs[3].y = 1.0f;
                //Assign triangles
                Triangles[0] = 3;
                Triangles[1] = 1;
                Triangles[2] = 2;
                Triangles[3] = 2;
                Triangles[4] = 1;
                Triangles[5] = 0;

                //Generate mesh
                Mesh mesh = new Mesh();
                mesh.name = MeshName + (i + 1);
                mesh.vertices = Vertices;
                mesh.uv = UVs;
                mesh.triangles = Triangles;
                mesh.RecalculateNormals();

                //Por ke guardar cada quad como un assett???????????????????? Dont know!!!!!!!!!!
                //Create asset in database
//                AssetDatabase.CreateAsset(mesh, AssetDatabase.GenerateUniqueAssetPath(AssetsFolder + "/" + MeshName + (i + 1))
//                + ".asset");
//                AssetDatabase.SaveAssets();

                //Create plane game object
                GameObject plane = new GameObject(GameObjectName);
                MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
                plane.AddComponent(typeof(MeshRenderer));
                //Assign mesh to mesh filter
                meshFilter.sharedMesh = mesh;
                mesh.RecalculateBounds();
                //Add a box collider component
                plane.AddComponent(typeof(BoxCollider));
            }

        }

        private void OnEnable()
        {
            GetFolderSelection();
        }


        private void OnSelectionChange()
        {
        }

    }


}
