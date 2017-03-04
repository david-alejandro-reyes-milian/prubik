using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelMap : MonoBehaviour
{
    private HashSet<string> final_positions_hash;
    private HashSet<string> pending_positions_hash;
    public int level_height;
    public int level_width;
    public ArrayList current_positions;
    public ArrayList final_positions;
    public int map_id;
    public static string MAPS_FOLDER =
        Application.dataPath +
        Path.DirectorySeparatorChar + "Resources" +
        Path.DirectorySeparatorChar + "Maps" +
        Path.DirectorySeparatorChar;
    public static string MAPS_SET_FILE = Application.dataPath +
        Path.DirectorySeparatorChar + "Resources" +
        Path.DirectorySeparatorChar + "Maps" +
        Path.DirectorySeparatorChar +
        "LevelMapSet.cs";

    public LevelMap() { }
    public LevelMap(int level_width, int level_height,
        ArrayList current_positions, ArrayList final_positions)
    {
        this.level_height = level_height;
        this.level_width = level_width;
        this.current_positions = current_positions;
        this.final_positions = final_positions;
        map_id = GetMapId();

        pending_positions_hash = new HashSet<string>();
        final_positions_hash = new HashSet<string>();

        Init();
    }


    public LevelMap(string map_path)
    {

        current_positions = new ArrayList();
        final_positions = new ArrayList();
        pending_positions_hash = new HashSet<string>();
        final_positions_hash = new HashSet<string>();

        FileStream file_stream =
            new FileStream(map_path, FileMode.Open, FileAccess.Read);
        StreamReader reader = new StreamReader(file_stream);
        string[] tmp = reader.ReadLine().Split('|');
        map_id = int.Parse(tmp[0]);
        level_width = int.Parse(tmp[1].Split(' ')[0]);
        level_height = int.Parse(tmp[1].Split(' ')[1]);

        string[] positions_string = tmp[2].Split(' ');
        int x, y;
        foreach (var coord in positions_string)
        {
            if (coord == null || coord == "") continue;
            x = int.Parse(coord.Substring(0, 2));
            y = int.Parse(coord.Substring(2, 2));
            final_positions.Add(new Vector2(x, y));
        }
        positions_string = tmp[3].Split(' ');
        foreach (var coord in positions_string)
        {
            if (coord == null || coord == "") continue;
            x = int.Parse(coord.Substring(0, 2));
            y = int.Parse(coord.Substring(2, 2));
            current_positions.Add(new Vector2(x, y));
        }
        reader.Close();
        map_id = GetMapId();
        Init();
    }

    public LevelMap Load(string map_string)
    {
        current_positions = new ArrayList();
        final_positions = new ArrayList();
        pending_positions_hash = new HashSet<string>();
        final_positions_hash = new HashSet<string>();

        string[] tmp = map_string.Split('|');
        map_id = int.Parse(tmp[0]);
        level_width = int.Parse(tmp[1].Split(' ')[0]);
        level_height = int.Parse(tmp[1].Split(' ')[1]);

        string[] positions_string = tmp[2].Split(' ');
        int x, y;
        foreach (var coord in positions_string)
        {
            if (coord == null || coord == "") continue;
            x = int.Parse(coord.Substring(0, 2));
            y = int.Parse(coord.Substring(2, 2));
            final_positions.Add(new Vector2(x, y));
        }
        positions_string = tmp[3].Split(' ');
        foreach (var coord in positions_string)
        {
            if (coord == null || coord == "") continue;
            x = int.Parse(coord.Substring(0, 2));
            y = int.Parse(coord.Substring(2, 2));
            current_positions.Add(new Vector2(x, y));
        }
        map_id = GetMapId();
        Init();
        return this;
    }
    public void Save(string map_name)
    {
        FileStream file_stream =
            new FileStream(MAPS_FOLDER + map_name, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter writer = new StreamWriter(file_stream);
        writer.Write(GetMapString());
        writer.Close();
    }

    public class VectorComparer : IComparer
    {
        public int Compare(object a, object b)
        {
            Vector2 aa = (Vector2)a;
            Vector2 bb = (Vector2)b;
            if (aa.x > bb.x)
                return 1;
            else if (aa.x < bb.x)
                return -1;
            else
            {
                if (aa.y > bb.y)
                    return 1;
                else if (aa.y < bb.y)
                    return -1;
            }
            return 0;
        }
    };
    public string GetMapString()
    {
        string map_string = "";
        map_string += map_id + "|";
        map_string += level_width + " " + level_height + "|";
        string tmp = "";
        int x, y;

        foreach (Vector2 position in final_positions)
        {
            x = (int)position.x;
            y = (int)position.y;
            tmp += FillDigits(x) + "" + FillDigits(y) + " ";
        }
        map_string += tmp + "|";
        tmp = "";
        foreach (Vector2 position in current_positions)
        {
            x = (int)position.x;
            y = (int)position.y;
            tmp += FillDigits(x) + "" + FillDigits(y) + " ";
        }
        map_string += tmp + "|";
        return map_string;
    }
    private string FillDigits(int value)
    {
        int digits_to_fill = 2;
        string res = "";
        int current_count = (value + "").Length;
        while (current_count++ < digits_to_fill)
            res += "0";
        res += value;
        return res;
    }

    public void Init()
    {
        foreach (Vector2 position in final_positions)
        {
            pending_positions_hash.Add((int)position.x + "" + (int)position.y);
            final_positions_hash.Add((int)position.x + "" + (int)position.y);
        }
        foreach (Vector2 position in current_positions)
            pending_positions_hash.Remove((int)position.x + "" + (int)position.y);
    }

    public void AddPendingPosition(int x, int y)
    {
        if (final_positions_hash.Contains(x + "" + y))
        {
            //DisplaySet(pending_positions);
            pending_positions_hash.Add(x + "" + y);
        }
    }

    public void RemovePendingPosition(int x, int y)
    {
        //DisplaySet(pending_positions);
        pending_positions_hash.Remove(x + "" + y);
    }

    private void DisplaySet(HashSet<string> set)
    {
        string tmp = "";
        foreach (string item in set)
        {
            tmp += item + ",";
        }
        Debug.Log(tmp);
        Debug.Log("Count " + pending_positions_hash.Count);
    }

    public bool LevelWon()
    {
        return pending_positions_hash.Count == 0;
    }

    public static LevelMap GetRandomMap(int level_width, int level_height)
    {
        var current_positions = new ArrayList();
        var final_positions = new ArrayList();

        while (current_positions.Count <= 0)
            for (int i = 0; i < level_width && current_positions.Count < level_width * level_height / 2; i++)
                for (int j = 0; j < level_height && current_positions.Count < level_width * level_height / 2; j++)
                    if (Random.Range(0, 10) >= 5)
                        current_positions.Add(new Vector2(i, j));

        while (final_positions.Count < current_positions.Count)
        {
            int i = Random.Range(0, level_width);
            int j = Random.Range(0, level_height);
            var tmp = new Vector2(i, j);
            if (!current_positions.Contains(tmp) &&
                !final_positions.Contains(tmp))
                final_positions.Add(tmp);
        }
        return new LevelMap(level_width, level_height, current_positions, final_positions);
    }

    public static LevelMap GetFullMap(int level_width, int level_height)
    {
        var current_positions = new ArrayList();
        var final_positions = new ArrayList();

        for (int i = 0; i < level_width && current_positions.Count < level_width * level_height / 2; i++)
            for (int j = 0; j < level_height && current_positions.Count < level_width * level_height / 2; j++)
                current_positions.Add(new Vector2(i, j));

        while (final_positions.Count < current_positions.Count)
        {
            int i = Random.Range(0, level_width);
            int j = Random.Range(0, level_height);
            var tmp = new Vector2(i, j);
            if (!current_positions.Contains(tmp) &&
                !final_positions.Contains(tmp))
                final_positions.Add(tmp);
        }
        return new LevelMap(level_width, level_height, current_positions, final_positions);
    }
    private int GetMapId()
    {
        string map_id = level_width + "" + level_height;
        foreach (Vector2 item in final_positions)
            map_id += (int)item.x + "" + (int)item.y;
        foreach (Vector2 item in current_positions)
            map_id += (int)item.x + "" + (int)item.y;
        int map_id_code = map_id.GetHashCode();
        return map_id_code;
    }
    public static HashSet<int> LoadCurrentMapsHash()
    {
        if (!Directory.Exists(MAPS_FOLDER))
            Directory.CreateDirectory(MAPS_FOLDER);
        HashSet<int> current_maps_hash = new HashSet<int>();
        string[] file_names = Directory.GetFiles(MAPS_FOLDER, "*.map");
        FileStream file_stream;
        StreamReader reader;
        foreach (string map_path in file_names)
        {
            LevelMap m = new LevelMap(map_path);
            current_maps_hash.Add(m.map_id);
        }
        return current_maps_hash;
    }
}