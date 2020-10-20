using System.Collections;
using Scenes.game;
using UnityEngine;

public class LevelBuilder
{
    private const string MATERIALS_FOLDER = "Materials/";
    private const string ANIMATIONS_FOLDER = "Animations/";
    private const int layer_background = 0;
    private const int layer_final_positions = 1;
    private const int layer_pieces = 2;

    public const int piece_kind_normal = 0;
    public const int piece_kind_final = 1;
    private readonly AnimationClip piece_animation_clip;
    private readonly int screen_height;
    private readonly int screen_width;
    public GameObject board;
    private Mesh board_mesh;
    public Resolution board_resolution;
    private float firt_piece_x;
    private float firt_piece_y;
    public float half_margin;
    public float half_piece_size;
    public int level_height = 3;
    public int level_width = 2;
    public LevelMap map;
    private float margin_percent = 8;
    private int piece_count;
    public ArrayList pieces;
    private Mesh quad_mesh;
    public bool starting_level;
    private Material cube_material, final_position_material;

    public LevelBuilder()
    {
        screen_width = Screen.width;
        screen_height = Screen.height;
        piece_animation_clip =
            Resources.Load(ANIMATIONS_FOLDER + "PieceEnterAnimation",
                typeof(AnimationClip)) as AnimationClip;

    }

    public void BuildMap(LevelMap map)
    {
        pieces = new ArrayList();
        starting_level = true;
        this.map = map;
        level_height = map.level_height;
        level_width = map.level_width;

        half_piece_size = MaximunHalfPieceSize();
        board_resolution = BoardResolution();

        quad_mesh = CreateMesh(half_piece_size, half_piece_size);
        board_mesh = CreateMesh(board_resolution.width, board_resolution.height);

        board =
            CreateCustomQuad(board_mesh, "GameBoardMat");
        board.name = "GameBoard";
        firt_piece_x = board_resolution.width - half_piece_size;
        firt_piece_y = -board_resolution.height + half_piece_size;
        piece_count = 0;

        foreach (Vector2 position in map.current_positions)
            pieces.Add(BuildPieceGameObject(position, piece_kind_normal));
        foreach (Vector2 position in map.final_positions)
            pieces.Add(BuildPieceGameObject(position, piece_kind_final));
    }

    public GameObject BuildPieceGameObject(Vector2 position, int piece_kind)
    {
        GameObject piece = CreateCustomQuad(quad_mesh,
            piece_kind == piece_kind_normal
                ? "CubeMat"
                : "FinalPositionMat");
        piece.name = piece_kind == piece_kind_normal ? "Piece " + piece_count++ : "Final position";
        piece.AddComponent<Animation>().AddClip(piece_animation_clip, "PieceEnterAnimation");
        piece.AddComponent<PieceSoundController>().Init();
        MeshRenderer mesh_renderer = piece.GetComponent<MeshRenderer>();
        mesh_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mesh_renderer.receiveShadows = false;
        mesh_renderer.useLightProbes = false;
        mesh_renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        if (starting_level)
        {
            piece.transform.localScale = new Vector3(0, 0);
        }

        int layer = layer_final_positions;
        if (piece_kind == piece_kind_normal)
        {
            var collider = piece.AddComponent<BoxCollider>();
            collider.center = new Vector3(collider.center.x, collider.center.y, 0);
            collider.size = new Vector3(2 * half_piece_size, 2 * half_piece_size, 80f);
            collider.isTrigger = true;
            piece.AddComponent<Piece>().Init((int)position.x, (int)position.y, map);
            layer = layer_pieces;
        }

        piece.transform.parent = board.transform;
        piece.transform.localPosition =
            new Vector3((firt_piece_x - half_margin) - position.x * (2 * (half_piece_size + half_margin)),
                (firt_piece_y + half_margin) + position.y * (2 * (half_piece_size + half_margin)), layer);
        return piece;
    }

    private Resolution BoardResolution()
    {
        var r = new Resolution();
        r.height = (int)(half_piece_size + half_margin) * level_height;
        r.width = (int)(half_piece_size + half_margin) * level_width;
        return r;
    }

    private float MaximunHalfPieceSize()
    {
        float piece_size = 0f;
        float piece_by_width = screen_width / level_width;
        float piece_by_heigth = screen_height / level_height;

        piece_size = Mathf.Min(piece_by_width, piece_by_heigth);
        // El margen usando porcientos es inexacto por lo que se redondea
        half_margin = Mathf.RoundToInt(piece_size * margin_percent / 100);
        //half_margin = 5;
        piece_size -= half_margin;
        return piece_size;
    }

    private GameObject CreateCustomQuad(Mesh mesh, string material_name)
    {
        var go = new GameObject("CustomQuad");
        var meshFilter = (MeshFilter)go.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = mesh;
        var renderer = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = Resources.Load(MATERIALS_FOLDER + material_name, typeof(Material)) as Material;
        //renderer.material.shader = Shader.Find("UI/Default");
        //renderer.material.color = color;
        //Texture2D tex = new Texture2D(1, 1);
        //tex.SetPixel(0, 0, Color.green);
        //tex.Apply();
        //renderer.material.mainTexture = tex;

        return go;
    }
    private GameObject CreateCustomQuad(Mesh mesh, Material material)
    {
        var go = new GameObject("CustomQuad");
        var meshFilter = (MeshFilter)go.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = mesh;
        var renderer = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = material;
        return go;
    }

    private Mesh CreateMesh(float width, float height)
    {
        var m = new Mesh();
        m.name = "ScriptedMesh";
        m.vertices = new[]
        {
            new Vector3(-width, -height, 0.01f),
            new Vector3(width, -height, 0.01f),
            new Vector3(width, height, 0.01f),
            new Vector3(-width, height, 0.01f)
        };
        m.uv = new[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0)
        };
        m.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        m.RecalculateNormals();
        return m;
    }
}