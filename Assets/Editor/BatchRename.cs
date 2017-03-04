using UnityEditor;
using UnityEngine;

namespace Assets
{
    public class BatchRename : ScriptableWizard
    {
        public string BaseName = "MyObject_";
        public int StartNumber = 0, Increment = 1;

        [MenuItem("JUMIT Tools/Batch Rename")]
        private static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard("Batch Rename", typeof(BatchRename), "Rename");
        }

        void OnWizardCreate()
        {
            int ObjectsCount = Selection.objects.Length;
            for (int i = StartNumber; i < ObjectsCount; i += Increment)
                Selection.objects[i].name = BaseName + i;
        }

        private void OnEnable()
        {
            UpdateSelectionHelper();
        }

        private void UpdateSelectionHelper()
        {
            helpString = "";
            if (Selection.objects != null)
            {
                helpString = "Number of selected items: " + Selection.objects.Length;
            }

        }

        private void OnSelectionChange()
        {
            UpdateSelectionHelper();
        }

    }

}
