using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CreateRandomMaps : MonoBehaviour
{
    private System.Collections.Generic.HashSet<int> current_maps_hash;
    public Text number_of_maps;
    public Text width;
    public Text height;

    // Use this for initialization
    void Awake()
    {
        current_maps_hash = LevelMap.LoadCurrentMapsHash();
        print("Current maps: " + current_maps_hash.Count);
    }

    public void Create()
    {
        int random_level_width, maps_to_create = number_of_maps.text == "" ? 20 : int.Parse(number_of_maps.text);
        int level_width = int.Parse(width.text);
        int level_height = int.Parse(height.text);
        while (maps_to_create > 0)
        {
            random_level_width = UnityEngine.Random.Range(2, 4);

            LevelMap m =
            LevelMap.GetRandomMap(level_width, level_height);
            if (!current_maps_hash.Contains(m.map_id))
            {
                m.Save("map_" + (current_maps_hash.Count + 1) + ".map");
                print("MAP CREATED: " + m.map_id);
                current_maps_hash.Add(m.map_id);
                maps_to_create--;
            }
        }
        SetupCanvasController.CreateMapSet();
    }
}
