using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    private const float piece_z_position = 0f;
    public const int up_direction = 0;
    public const int right_direction = 1;
    public const int down_direction = 2;
    public const int left_direction = 3;

    // States
    public const int idle_state = 0;
    public const int moving_state = 1;
    public const int mouse_down_state = 2;
    public const int selection_made_state = 3;
    public const int menu_state = 4;
    public const int level_start_state = 5;
    public const int scene_transition_state = 6;

    private const float ray_start = -10000;
    public static int piece_enter_clip_index;
    public static AudioClip[] piece_enter_clips;
    public GameObject canvas;
    public GameObject generalCanvas;
    private Animation current_animation;
    public int current_state;
    public Vector3 direction;
    public float distance;
    private Vector3 final_position;
    public Vector3 heading;

    private LevelBuilder levelBuilder;
    public static LevelMapSet level_maps_factory;
    private AudioClip level_won_clip;

    // External controllers
    private ScreenFadeInOut screen_fader;
    public MenuController menu_controller;
    private float min_selection_distance;
    public Vector3 mouse_current_position;
    public Vector3 mouse_start_position;
    private AudioClip movement_clip;
    private int movement_direction;
    private float movement_interpolation;
    private float movement_interpolation_speed;
    private bool moving_on_x;
    private Vector3 partial_position;
    private float piece_margin;
    public float piece_size;
    private int piece_started_index;
    private Vector3 scanPos;
    private Vector3 screen_point;
    private GameObject selection;
    private Vector3 selection_initial_position;
    public int current_map;
    private UserData user_data;
    private int created_map_index;
    public GameObject help;


    void Awake()
    {
        screen_fader = GameObject.Find("ScreenFader").GetComponent<ScreenFadeInOut>();
    }
    public void Start()
    {
        levelBuilder = new LevelBuilder();
        level_maps_factory = new LevelMapSet();
        user_data = new UserData();
        movement_interpolation_speed = 5.5f * Screen.dpi;
        min_selection_distance = 5000f;

        generalCanvas = GameObject.Find("GeneralCanvas");

        InitSounds();

        // Cargar ultimo mapa desblokeado por el usuario
        LoadMap(user_data.last_unlocked_map);
        created_map_index = 1;

    }
    public int GetLastUnlockedMap()
    {
        return user_data.last_unlocked_map + 1;
    }

    public void ShowHelp()
    {
        help.SetActive(true);
        help.GetComponentInChildren<Animation>().Play("HelpLevelHand");
    }
    public void HideHelp()
    {
        help.SetActive(false);
    }

    public void LoadMap(int map_index)
    {
        // Si se carga el primer mapa se muestra la ayuda asociada
        if (map_index == 0) ShowHelp();
        else HideHelp();


        canvas.SetActive(false);
        generalCanvas.SetActive(false);
        //Se destruye el mapa anterior
        Destroy(levelBuilder.board);

        current_map = map_index;
        menu_controller.SetLevelNumber(map_index + 1);

        //MAP
        LevelMap m = new LevelMap().Load(GameManager.level_maps_factory.maps[map_index]);

        //int random_level_width = UnityEngine.Random.Range(2, 4);
        //LevelMap m =
        //    LevelMap.GetRandomMap(random_level_width,
        //        UnityEngine.Random.Range(random_level_width, random_level_width * UnityEngine.Random.Range(1, 3)));
        //m.Save("map_" + (level_maps_factory.maps.Length + ++created_map_index) + ".map");
        //SetupCanvasController.CreateMapSet();

        levelBuilder.BuildMap(m);
        int min = Math.Min(levelBuilder.board_resolution.width, levelBuilder.board_resolution.height);
        int max = Math.Max(levelBuilder.board_resolution.width, levelBuilder.board_resolution.height);
        Camera.main.orthographicSize =
            min + min * max / min;

        selection = new GameObject("Selection");
        selection.transform.parent = levelBuilder.board.transform;

        piece_size = levelBuilder.half_piece_size * 2;
        piece_margin = levelBuilder.half_margin * 2;

        piece_started_index = 0;
        screen_fader.MakeTransition();
        current_state = level_start_state;

    }

    private void InitSounds()
    {
        movement_clip = Resources.Load("Sounds/movement_clip", typeof(AudioClip)) as AudioClip;
        level_won_clip = Resources.Load("Sounds/level_won_clip", typeof(AudioClip)) as AudioClip;
        piece_enter_clips = new AudioClip[12];
        for (int i = 0; i < 11; i++)
            piece_enter_clips[i] = Resources.Load("Sounds/combo_sound" + i, typeof(AudioClip)) as AudioClip;
        piece_enter_clip_index = 0;
    }
    private void Update()
    {
        HandleMenuActions();
        HandleLevelStart();

        if (current_state == idle_state)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray vRayStart = Camera.main.ScreenPointToRay(Input.mousePosition);
                mouse_start_position = vRayStart.origin;
                current_state = mouse_down_state;
            }
        }
        else if (current_state == mouse_down_state)
        {
            //Si se levanta el mouse y no se realizo un movimiento se retorna al estado idle
            if (Input.GetMouseButtonUp(0)) { current_state = idle_state; return; }

            mouse_current_position = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
            float delta_position = Math.Abs(mouse_current_position.sqrMagnitude -
                                            mouse_start_position.sqrMagnitude);
            //si se tienen dos puntos se puede describir un vector.
            if (delta_position >= min_selection_distance)
            {
                //Se oculta el menu, en caso de estar abierto
                menu_controller.HideMenu();

                //Vector desde la posicion inicial del mouse hasta la posicion actual
                heading = mouse_current_position - mouse_start_position;
                //Distancia entre los puntos = magnitude del vector
                distance = heading.magnitude;
                //Direccion es igual al vector distancia normalizado
                direction = heading / distance;
                MakeSelection();
                if (selection.transform.childCount > 0)
                    PlayMovementSound();
                current_state = selection_made_state;
            }
        }

        else if (current_state == selection_made_state)
        {
            float delta_position = EdgedDistance();
            if (true)
            {
                partial_position = selection.transform.position;
                current_state = moving_state;
                // Se va al proximo estado
                float next_x = selection_initial_position.x;
                float next_y = selection_initial_position.y;
                float position_offset = (delta_position > 0
                    ? (piece_size + piece_margin)
                    : -(piece_size + piece_margin));
                if (moving_on_x)
                    next_x += position_offset;
                else
                    next_y += position_offset;
                final_position = new Vector3(next_x, next_y, 0);

                //Se crean los reemplazos
                UpdatePiecesReplacement();
            }
        }
        else if (current_state == moving_state)
        {
            AnimateMovement(partial_position, final_position);
        }

        // Salir
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            print("Escape");
            // No trabaja bien(BUSCAR)
            //Application.Quit();
            // Primero se guarda el estado actual de la partida
            OnApplicationQuit();
            // Luego se cierra forzadamente, funciona correctamente en Android
            ForceQuit();
        }

    }

    private void HandleLevelStart()
    {
        if (current_state == level_start_state)
        {
            if (Input.GetMouseButtonDown(0))
            {
                EndLevelStartAnimation();
            }

            if (piece_started_index > levelBuilder.pieces.Count - 1)
            {
                //Se termino la animacion de inicio de nivel
                current_state = idle_state;
                canvas.SetActive(true);
                generalCanvas.SetActive(true);
                levelBuilder.starting_level = false;
                return;
            }
            if (current_animation != null && current_animation.isPlaying)
                return;

            current_animation = (levelBuilder.pieces[piece_started_index++] as GameObject).GetComponent<Animation>();

            current_animation.Play("PieceEnterAnimation");
        }
    }

    public void ForceQuit()
    {
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
    void EndLevelStartAnimation()
    {
        // Terminar animacion de inicio de nivel
        for (int i = piece_started_index; i < levelBuilder.pieces.Count; i++)
            (levelBuilder.pieces[i] as GameObject).transform.localScale = new Vector3(1, 1, 1);

        //Se termino la animacion de inicio de nivel
        current_state = idle_state;
        canvas.SetActive(true);
        generalCanvas.SetActive(true);
        levelBuilder.starting_level = false;
        return;
    }

    bool OverMenu()
    {
        return (EventSystem.current.IsPointerOverGameObject(0) ||
            EventSystem.current.IsPointerOverGameObject());
    }
    private void HandleMenuActions()
    {
        if (current_state != level_start_state && OverMenu())
        {
            // Si no se esta cargando ningun nivel y se dan clicks solo sobre el menu, prevalece el estado de menu
            current_state = menu_state;
        }
        if (current_state == menu_state && !OverMenu())
        {
            // Si estado == menu_activo y se toca algo fuera del menu se cambia el estado a idle
            current_state = idle_state;
        }
        if (Input.GetKeyUp(KeyCode.Menu) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            // Si estado = inicio de nivel se termina la animacion de inicio de nivel y se esconde el menu
            if (current_state == level_start_state) { EndLevelStartAnimation(); current_state = menu_state; menu_controller.ShowMenu(); }

            // Si estado == idle, se intercambia estado de menu
            if (current_state == idle_state) { current_state = menu_state; menu_controller.TogleMenu(); }
        }
        //Para abrir el menu con click en cualkier sitio
        //if (Input.GetMouseButtonUp(0) && current_state == mouse_down_state)
        //{
        //    current_state = idle_state;
        //    menu_controller.TogleMenu();
        //}
    }

    private void PlayMovementSound()
    {
        if (MenuController.sound_enabled)
            Camera.main.GetComponent<AudioSource>().PlayOneShot(movement_clip, .5f);
    }

    private void LevelWonSound()
    {
        if (MenuController.sound_enabled)
            Camera.main.GetComponent<AudioSource>().PlayOneShot(level_won_clip, .5f);
    }

    private void UpdatePiecesReplacement()
    {
        for (int i = selection.transform.childCount - 1; i >= 0; i--)
        {
            Transform piece = selection.transform.GetChild(i);
            var logical_piece = piece.gameObject.GetComponent<Piece>();
            logical_piece.SetInactiveOnMapState();
            if (logical_piece.GoesOut(movement_direction))
            {
                // Si la ficha sale del tablero se genera automaticamente su reemplazo en la 
                // direccion opuesta para realizar la animacion correspondiente y se elimina
                // la ficha ke muere
                int next_x = logical_piece.x, next_y = logical_piece.y;
                if (moving_on_x) next_x = logical_piece.x == 0 ? levelBuilder.level_width : -1;
                else next_y = logical_piece.y == 0 ? levelBuilder.level_height : -1;

                GameObject new_piece = levelBuilder.BuildPieceGameObject(new Vector2(next_x, next_y),
                    LevelBuilder.piece_kind_normal);
                new_piece.transform.parent = selection.transform;
                new_piece.name = piece.name;

                // Manda a eliminar la ficha al terminar la animacion del movimiento actual
                logical_piece.AutoDestroyOnFuture();
            }
        }
    }

    private void AnimateMovement(Vector3 partial_position, Vector3 final_position)
    {
        if (current_state == moving_state)
        {
            movement_interpolation += Time.deltaTime * movement_interpolation_speed;
            selection.transform.position = Vector3.MoveTowards(selection.transform.position, final_position,
                movement_interpolation);
            float abs_sqr_delta_position =
                Math.Abs(selection.transform.position.sqrMagnitude - final_position.sqrMagnitude);

            if (abs_sqr_delta_position <= min_selection_distance)
            {
                // Se fuerza la posicion final para evitar errores
                selection.transform.position = final_position;
                // Se reinicia el manejador de interpolacion
                movement_interpolation = 0;

                // Se actualiza la logica de las fichas movidas

                RemoveUpdateChilds(selection);
            }
        }
    }

    // Distancia entre la posicion actual del mouse y la posicion
    // inicial en el eje actual de movimiento (X|Y)
    private float EdgedDistance()
    {
        if (moving_on_x)
            return mouse_current_position.x - mouse_start_position.x;
        return mouse_current_position.y - mouse_start_position.y;
    }

    private void MakeSelection()
    {
        //Se toman las componentes del vector de direccion para conocer hacia donde desea
        //mover el usuario
        var component_x = new Vector2(heading.x, 0);
        var component_y = new Vector2(0, heading.y);
        double magnitude_x = component_x.sqrMagnitude;
        double magnitude_y = component_y.sqrMagnitude;

        var ray = new Ray();
        if (magnitude_x > magnitude_y)
        {
            ray = new Ray(new Vector3(ray_start, mouse_start_position.y, 0), Vector2.right);

            moving_on_x = true;
            if (direction.x <= 0)
                movement_direction = right_direction;
            else
                movement_direction = left_direction;
        }
        else
        {
            ray = new Ray(new Vector3(mouse_start_position.x, ray_start, 0), Vector2.up);
            moving_on_x = false;
            if (direction.y >= 0)
                movement_direction = up_direction;
            else
                movement_direction = down_direction;
        }
        SelectObjectsInRay(ray);
    }

    private void SelectObjectsInRay(Ray ray)
    {
        // Se captura la posicion inicial de la seleccion para uso posterior
        selection_initial_position = selection.transform.position;

        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits.Length > 0)
            foreach (RaycastHit hit in hits)
            {
                Transform piece = hit.transform;
                piece.parent = selection.transform;
            }
    }

    private void RemoveUpdateChilds(GameObject go)
    {
        for (int i = go.transform.childCount - 1; i >= 0; i--)
        {
            int index = i;
            if (movement_direction == down_direction ||
                movement_direction == right_direction)
                index = go.transform.childCount - i - 1;

            Transform piece = go.transform.GetChild(index);
            var logical_piece = piece.gameObject.GetComponent<Piece>();

            if (logical_piece.auto_destroy)
                logical_piece.Destroy();
            else
            {
                logical_piece.UpdateLogicalPosition(movement_direction);
                logical_piece.SetActiveOnMapState();
                piece.transform.parent = levelBuilder.board.transform;
            }
        }
        if (levelBuilder.map.LevelWon())
        {

            LevelWonSound();
            print("You Win!!!");
            int next_map = (current_map + 1) % level_maps_factory.maps.Length;
            user_data.last_unlocked_map = Mathf.Max(user_data.last_unlocked_map, next_map);
            LoadMap(next_map);
        }
        else
            current_state = idle_state;
    }

    private void CleanSelection()
    {
        for (int i = selection.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = selection.transform.GetChild(i);
            child.transform.parent = levelBuilder.board.transform;
        }
    }
    public void OnApplicationPause()
    {
        print("OnApplicationPause");
        if (user_data != null)
            user_data.Save();
    }
    public void OnApplicationQuit()
    {
        print("OnApplicationQuit");
        if (user_data != null)
            user_data.Save();
    }
}