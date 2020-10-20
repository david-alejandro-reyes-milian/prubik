using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Scenes.game
{
    [System.Serializable]
    public class UserData
    {
        public int lastUnlockedMap;
        [NonSerialized] private BinaryFormatter binary_formatter;
        [NonSerialized] private Stream fStream;

        public UserData()
        {
            binary_formatter = new BinaryFormatter();
            lastUnlockedMap = 0;
        }

        public void Save()
        {
            try
            {
                fStream = new FileStream(Application.persistentDataPath + "/UserData.pru",
                    FileMode.Create, FileAccess.Write, FileShare.None);
                binary_formatter.Serialize(fStream, this);
                fStream.Close();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public void Load()
        {
            try
            {
                fStream = File.OpenRead(Application.persistentDataPath + "/UserData.pru");
                var loadedData = (UserData) binary_formatter.Deserialize(fStream);
                lastUnlockedMap = loadedData.lastUnlockedMap;
                fStream.Close();
            }
            catch (Exception e)
            {
                Debug.Log("No existe el fichero de salvas: " + Application.persistentDataPath + "/UserData.pru");
                Debug.Log(e.Message);
            }
        }
    }
}