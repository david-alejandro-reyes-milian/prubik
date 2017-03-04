using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateLevelCanvasController : MonoBehaviour
{
    public static HashSet<string> final_positions;
    public static HashSet<string> current_positions;
    public int level_width;
    public int level_height;
    private SetupCanvasController setup_canvas_controller;
    public GameObject setup_canvas;
    public GameObject create_level_canvas;
    public GameObject board;
    public void Start()
    {
        Init();
    }
    private void Init()
    {
        final_positions = new HashSet<string>();
        current_positions = new HashSet<string>();
        setup_canvas_controller = gameObject.GetComponent<SetupCanvasController>();
    }

    public void Save()
    {
        print("Saving");
        ArrayList current_positions_array = new ArrayList();
        ArrayList final_positions_array = new ArrayList();
        if (current_positions.Count < final_positions.Count)
        {
            print("Finales: " + final_positions.Count);
            print("Fichas: " + current_positions.Count);
            print("Bad Map, not saving");
            return;
        }
        else if (current_positions.Count > final_positions.Count)
        {
            print("Finales: " + final_positions.Count);
            print("Fichas: " + current_positions.Count);
            print("Bad Map, not saving");
            return;
        }
        foreach (string item in current_positions)
            current_positions_array.Add(new Vector2(int.Parse(item.Substring(0, 2)), int.Parse(item.Substring(2, 2))));

        foreach (string item in final_positions)
            final_positions_array.Add(new Vector2(int.Parse(item.Substring(0, 2)), int.Parse(item.Substring(2, 2))));
        LevelMap m = new LevelMap(level_width, level_height, current_positions_array, final_positions_array);
        if (setup_canvas_controller.current_maps_hash.Contains(m.map_id))
            print("Map exists already, will not be saved");
        else
        {
            m.Save("map_" + (setup_canvas_controller.current_maps_hash.Count + 1) + ".map");
            setup_canvas_controller.current_maps_hash.Add(m.map_id);
            SetupCanvasController.CreateMapSet();
            print("Map saved");
        }
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            Save();
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            print("back");
            Init();
            for (int i = 0; i < board.transform.childCount; i++)
                board.transform.GetChild(i).GetComponent<LevelBuilderButton>().Destroy();
            create_level_canvas.active = false;
            setup_canvas.active = true;
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            print("clean");
            final_positions = new HashSet<string>();
            current_positions = new HashSet<string>();
            foreach (GameObject button in setup_canvas_controller.buttons)
                button.GetComponent<LevelBuilderButton>().Start();
        }
    }
}
