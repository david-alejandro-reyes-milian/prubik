using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets
{
    public class CreateSceneLayout : ScriptableWizard
    {
        public string BasePath = "Assets/Scenes/";
        public string SceneName = "";
        public string[] Folders = new string[] { "Scripts", "Textures", "Animations" };

        [MenuItem("JUMIT Tools/Create Scene Layout")]
        private static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard("Create Scene Layout", typeof(CreateSceneLayout), "Create");
        }

        void OnWizardCreate()
        {
            if (!Directory.Exists(BasePath + SceneName))
            {
                Directory.CreateDirectory(BasePath + SceneName);
                foreach (string folder in Folders)
                {
                    Directory.CreateDirectory(BasePath + SceneName + "/" + folder);
                }
            }
        }




    }

}
