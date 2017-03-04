using System;
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Object = UnityEngine.Object;

//------------------------------------------------
public class TexturePacker : ScriptableWizard
{
    //Name of Atlas Texture
    public string AtlasName = "Atlas_Texture";
    //Amount of padding in atlas
    public int Padding = 4;
    //Reference to list of textures
    public Texture2D[] Textures;

    public string AtlasPath = "Assets";
    //------------------------------------------------//Called when dev selects window from main menu
    [MenuItem("JUMIT Tools/Create Atlas Texture")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard("Create Atlas", typeof(TexturePacker));
    }

    //Sorts the textures, needed for animation textures! by DARM
    private class Comparador : IComparer
    {
        public int Compare(object obj1, object obj2)
        {
            Texture emp1 = (Texture)obj1;
            Texture emp2 = (Texture)obj2;
            return (String.Compare(emp1.name, emp2.name));
        }
    }
    //------------------------------------------------//Called when window is first created or shown
    void OnEnable()
    {

        //Search through selected objects for textures
        //Create new texture list
        List<Texture2D> TextureList = new List<Texture2D>();
        //Loop through objects selected in editor
        if (Selection.objects != null && Selection.objects.Length > 0)
        {
            Object[] objects = EditorUtility.CollectDependencies(Selection.objects);
            Array.Sort(objects, new Comparador());

            foreach (Object o in objects)
            {
                //Get selected object as texture
                Texture2D tex = o as Texture2D;
                //Is texture asset?
                if (tex != null)
                {
                    //Add to list
                    TextureList.Add(tex);
                }
            }
        }



        //Check count. If >0, then create array
        if (TextureList.Count > 0)
        {
            //Obtaining selected textures Folder for atlases by DARM!
            string tmp = AssetDatabase.GetAssetPath(TextureList[0]);
            tmp = tmp.Split('.')[0];
            AtlasPath = tmp.Replace(TextureList[0].name, "") + "Atlas/";
            MonoBehaviour.print("Selected Textures path: " + AtlasPath);
            if (!Directory.Exists(AtlasPath))
                Directory.CreateDirectory(AtlasPath);

            Textures = new Texture2D[TextureList.Count];
            for (int i = 0; i < TextureList.Count; i++)
            {
                Textures[i] = TextureList[i];
            }
        }
    }
    void OnWizardCreate()
    {
        GenerateAtlas();
    }
    public void GenerateAtlas()
    {
        //Generate Atlas Object
        GameObject AtlasObject = new GameObject("obj_" + AtlasName);
        AtlasData AtlasComp = AtlasObject.AddComponent<AtlasData>();
        //Initialize string array
        AtlasComp.TextureNames = new string[Textures.Length];
        AtlasComp.TexturePaths = new string[Textures.Length];
        //Cycle through textures and configure for atlasing
        for (int i = 0; i < Textures.Length; i++)
        {
            //Get asset path
            string TexturePath = AssetDatabase.GetAssetPath(Textures[i]);
            //Configure texture
            ConfigureForAtlas(TexturePath);
            //Add file name to atlas texture name list
            AtlasComp.TexturePaths[i] = TexturePath;
            AtlasComp.TextureNames[i] = Textures[i].name;

        }
        //Generate Atlas
        Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        AtlasComp.UVs = tex.PackTextures(Textures, Padding, 4096);
        //Generate Unique Asset Path
        string AssetPath = AssetDatabase.GenerateUniqueAssetPath(AtlasPath + AtlasName + ".png");
        //Write texture to file
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(AssetPath, bytes);
        bytes = null;
        //Delete generated texture
        UnityEngine.Object.DestroyImmediate(tex);
        //Import Asset
        AssetDatabase.ImportAsset(AssetPath);
        //Get Imported Texture
        AtlasComp.AtlasTexture = AssetDatabase.LoadAssetAtPath(AssetPath, typeof(Texture2D)) as
        Texture2D;
        //Configure texture as atlas
        ConfigureForAtlas(AssetDatabase.GetAssetPath(AtlasComp.AtlasTexture));
        //Now create prefab from atlas object
        AssetPath = AssetDatabase.GenerateUniqueAssetPath(AtlasPath + "atlasdata_" + AtlasName + ".prefab");
        //Create prefab object
        Object prefab = PrefabUtility.CreateEmptyPrefab(AssetPath);
        //Update prefab and save
        PrefabUtility.ReplacePrefab(AtlasObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        //Destroy original object
        DestroyImmediate(AtlasObject);
    }
    //------------------------------------------------//Function to configure texture for atlasing
    public void ConfigureForAtlas(string TexturePath)
    {
        TextureImporter TexImport = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
        TextureImporterSettings tiSettings = new TextureImporterSettings();
        TexImport.textureType = TextureImporterType.Advanced;
        TexImport.ReadTextureSettings(tiSettings);
        tiSettings.mipmapEnabled = false;
        tiSettings.readable = true;
        tiSettings.maxTextureSize = 4096;
        tiSettings.textureFormat = TextureImporterFormat.ARGB32;
        tiSettings.filterMode = FilterMode.Point;
        tiSettings.wrapMode = TextureWrapMode.Clamp;
        tiSettings.npotScale = TextureImporterNPOTScale.None;
        TexImport.SetTextureSettings(tiSettings);
        //Save changes
        AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
    }
    //------------------------------------------------

}