using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

[System.Serializable]
public class UserData
{
    public int last_unlocked_map;
    [System.NonSerialized]
    private BinaryFormatter binary_formatter;
    [System.NonSerialized]
    private Stream fStream;
    private string USER_DATA_PATH = Application.persistentDataPath + "/UserData.pru";
    public UserData()
    {
        binary_formatter = new BinaryFormatter();
        last_unlocked_map = 0;
        Load();
    }

    public void Save()
    {
        try
        {
            fStream = new FileStream(USER_DATA_PATH,
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
            fStream = File.OpenRead(USER_DATA_PATH);
            UserData loaded_data = (UserData)binary_formatter.Deserialize(fStream);
            last_unlocked_map = loaded_data.last_unlocked_map;
            fStream.Close();
        }
        catch (Exception e)
        {
            Debug.Log("No existe el fichero de salvas: " + USER_DATA_PATH);
            Debug.Log(e.Message);
        }
    }
}
