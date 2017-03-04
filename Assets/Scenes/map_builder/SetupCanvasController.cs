using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class SetupCanvasController : MonoBehaviour
{
    public GameObject selected_width;
    public GameObject selected_height;
    public Slider slider_width;
    public Slider slider_height;
    public GameObject cell_button_prefab;
    public GameObject setup_canvas;
    public GameObject create_level_canvas;
    public GameObject board;
    public HashSet<int> current_maps_hash;
    public ArrayList buttons;
    private const int MIN_SLIDER_VALUE = 2;

    void Start()
    {
        slider_width.minValue = MIN_SLIDER_VALUE;
        slider_height.minValue = slider_width.minValue;
        selected_width.GetComponent<Text>().text = slider_width.minValue + "";
        selected_height.GetComponent<Text>().text = slider_height.minValue + "";
        current_maps_hash = LevelMap.LoadCurrentMapsHash();
    }

    public void StartMapBuilding()
    {
        buttons = new ArrayList();
        int map_width = (int)slider_width.value;
        int map_height = (int)slider_height.value;


        gameObject.GetComponent<CreateLevelCanvasController>().level_width = map_width;
        gameObject.GetComponent<CreateLevelCanvasController>().level_height = map_height;

        int button_size = Screen.width / map_width;
        create_level_canvas.SetActive(true);

        float canvas_x_size = setup_canvas.GetComponent<RectTransform>().sizeDelta.x;
        float canvas_y_size = setup_canvas.GetComponent<RectTransform>().sizeDelta.y;


        float size = canvas_x_size / map_width / 2;
        size = Mathf.Min(size, canvas_y_size / map_height / 2);
        int margin = (int)(size * 2 / 100);
        size -= margin;


        board.GetComponent<RectTransform>().sizeDelta = new Vector2((size + margin) * map_width * 2, (size + margin) * map_height * 2);
        float x_start = -(size + margin) * map_width;
        float y_start = -(size + margin) * map_height;
        for (int i = 0; i < map_width; i++)
        {
            for (int j = 0; j < map_height; j++)
            {
                Vector3 pos =
                    new Vector3(x_start + (size + margin) + i * (2 * (size + margin)), y_start + (size + margin) + j * (2 * (size + margin)), 1);
                GameObject btn = GameObject.Instantiate(cell_button_prefab) as GameObject;
                btn.transform.parent = board.transform;
                btn.GetComponent<RectTransform>().sizeDelta = new Vector2(size / 2, size / 2);
                LevelBuilderButton logical_button = btn.GetComponent<LevelBuilderButton>();
                logical_button.x = i;
                logical_button.y = j;
                btn.transform.localPosition = pos;
                buttons.Add(btn);
            }
        }
        setup_canvas.SetActive(false);
    }

    public void ChangeMapWidth()
    {
        selected_width.GetComponent<Text>().text = slider_width.value + "";
        slider_height.minValue = slider_width.value;
        if (slider_width.value > slider_height.minValue)
        {
            slider_height.value = slider_width.value;
        }

    }
    public void ChangeMapHeight()
    {
        selected_height.GetComponent<Text>().text = slider_height.value + "";
    }
    public static void CreateMapSet()
    {
        int maps_count = LevelMap.LoadCurrentMapsHash().Count;
        string map_set = "using System;using UnityEngine;public class LevelMapSet:MonoBehaviour{ public string[] maps = new string[]{";
        for (int i = 0; i < maps_count; i++)
        {
            LevelMap m =
           new LevelMap(LevelMap.MAPS_FOLDER + "map_" + (i + 1) + ".map");
            map_set += "\"" + m.GetMapString() + "\",";
        }
        map_set += "};}";
        FileStream file_stream =
                    new FileStream(LevelMap.MAPS_SET_FILE, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter writer = new StreamWriter(file_stream);
        writer.Write(map_set);
        writer.Close();
    }

}
