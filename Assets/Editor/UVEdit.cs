using UnityEngine;
using UnityEditor;
//------------------------------------------------
public class UVEdit : EditorWindow
{
    //Reference to atlas data game object
    public GameObject AtlasDataObject = null;
    //Reference to atlas data
    public AtlasData AtlasDataComponent = null;
    //Popup Index
    public int PopupIndex = 0;
    //Popup strings for sprite selection mode: sprites or custom (sprites = select sprites from atlas, custom = manually set UVs)
    public string[] Modes = { "Select By Sprites", "Select By UVs" };
    //Sprite Select Index - selection in the drop down box
    public int ModeIndex = 0;
    //Rect for manually setting UVs in Custom mode
    public Rect CustomRect = new Rect(0, 0, 0, 0);
    //-----------------------------------------------
    [MenuItem("JUMIT Tools/Atlas Texture Editor")]
    static void Init()
    {
        //Show window
        GetWindow(typeof(UVEdit), false, "Texture Atlas", true);
    }
    //------------------------------------------------
    void OnGUI()
    {
        //Draw Atlas Object Selector
        GUILayout.Label("Atlas Generation", EditorStyles.boldLabel);
        AtlasDataObject = (GameObject)EditorGUILayout.ObjectField("Atlas Object", AtlasDataObject,
        typeof(GameObject), true);
        //If no valid atlas object selected, then cancel
        if (AtlasDataObject == null)
            return;//Get atlas data component attached to selected prefab
        AtlasDataComponent = AtlasDataObject.GetComponent<AtlasData>();
        //If no valid data object, then cancel
        if (!AtlasDataComponent)
            return;
        //Choose sprite selection mode: sprites or UVs
        ModeIndex = EditorGUILayout.Popup(ModeIndex, Modes);
        //If selecting by sprites
        if (ModeIndex != 1)
        {
            //Show popup selector for valid textures
            PopupIndex = EditorGUILayout.Popup(PopupIndex, AtlasDataComponent.TextureNames);
            //When clicked, set UVs on selected objects
            if (GUILayout.Button("Select Sprite From Atlas"))
            {
                //Update UVs for selected meshes
                if (Selection.gameObjects.Length > 0)
                {
                    foreach (GameObject Obj in Selection.gameObjects)
                    {
                        //Is this is a mesh object?
                        if (Obj.GetComponent<MeshFilter>())
                            UpdateUVs(Obj, AtlasDataComponent.UVs[PopupIndex]);
                    }
                }
            }
        }
        else
        {
            //Selecting manually
            GUILayout.Label("X");
            CustomRect.x = EditorGUILayout.FloatField(CustomRect.x);
            GUILayout.Label("Y");
            CustomRect.y = EditorGUILayout.FloatField(CustomRect.y);
            GUILayout.Label("Width");
            CustomRect.width = EditorGUILayout.FloatField(CustomRect.width);
            GUILayout.Label("Height");
            CustomRect.height = EditorGUILayout.FloatField(CustomRect.height);
            //When clicked, set UVs on selected objects
            if (GUILayout.Button("Select Sprite From Atlas"))
            {
                //Update UVs for selected meshes
                if (Selection.gameObjects.Length > 0)
                {
                    foreach (GameObject Obj in Selection.gameObjects)
                    {//Is this is a mesh object?
                        if (Obj.GetComponent<MeshFilter>())
                            UpdateUVs(Obj, CustomRect);
                    }
                }
            }
        }
    }
    //------------------------------------------------//Function to update UVs of selected mesh object
    void UpdateUVs(GameObject MeshOject, Rect AtlasUVs, bool Reset = false)
    {
        //Get Mesh Filter Component
        MeshFilter MFilter = MeshOject.GetComponent<MeshFilter>();
        Mesh MeshObject = MFilter.sharedMesh;
        //Vertices
        Vector3[] Vertices = MeshObject.vertices;
        Vector2[] UVs = new Vector2[Vertices.Length];
        //Bottom-left
        UVs[0].x = (Reset) ? 0.0f : AtlasUVs.x;
        UVs[0].y = (Reset) ? 0.0f : AtlasUVs.y;
        //Bottom-right
        UVs[1].x = (Reset) ? 1.0f : AtlasUVs.x + AtlasUVs.width;
        UVs[1].y = (Reset) ? 0.0f : AtlasUVs.y;
        //Top-left
        UVs[2].x = (Reset) ? 0.0f : AtlasUVs.x;
        UVs[2].y = (Reset) ? 1.0f : AtlasUVs.y + AtlasUVs.height;
        //Top-right
        UVs[3].x = (Reset) ? 1.0f : AtlasUVs.x + AtlasUVs.width;
        UVs[3].y = (Reset) ? 1.0f : AtlasUVs.y + AtlasUVs.height;
        MeshObject.uv = UVs;
        MeshObject.vertices = Vertices;
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }
    //------------------------------------------------
}