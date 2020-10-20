using System;
using JetBrains.Annotations;
using Scenes.game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private const float piece_z_position = 0f;
    public const int UpDirection = 0;
    public const int RightDirection = 1;
    public const int DownDirection = 2;
    public const int LeftDirection = 3;

    // States
    private const int idleState = 0;
    public const int MovingState = 1;
    private const int mouseDownState = 2;
    private const int selectionMadeState = 3;
    public const int MenuState = 4;
    private const int levelStartState = 5;
    private const int sceneTransitionState = 6;

    private const float ray_start = -10000;
    public static int piece_enter_clip_index;
    public static AudioClip[] piece_enter_clips;
    public GameObject canvas;
    public GameObject generalCanvas;
    private Animation current_animation;
    public int currentState;
    public Vector3 direction;
    public float distance;
    private Vector3 final_position;
    public Vector3 heading;

    private LevelBuilder levelBuilder;
    private static LevelMapSet _levelMapSet;
    private static LevelMap _levelMap;
    private AudioClip level_won_clip;

    // External controllers
    private ScreenFadeInOut screenFader;
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
    public Camera mainCamera;


    void Awake()
    {
        screenFader = GameObject.Find("ScreenFader").GetComponent<ScreenFadeInOut>();
        mainCamera = GetComponentInParent<Camera>();
        user_data = new UserData();
        levelBuilder = new LevelBuilder();
        _levelMap = new LevelMap();
        _levelMapSet = gameObject.AddComponent<LevelMapSet>();
        movement_interpolation_speed = 5.5f * Screen.dpi;
        min_selection_distance = 5000f;

        generalCanvas = GameObject.Find("GeneralCanvas");

        InitSounds();

        LoadMap(user_data.lastUnlockedMap);
        created_map_index = 1;
    }

    public int GetLastUnlockedMap()
    {
        return user_data.lastUnlockedMap + 1;
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

    public void LoadMap(int mapIndex)
    {
        // Si se carga el primer mapa se muestra la ayuda asociada
        if (mapIndex == 0) ShowHelp();
        else HideHelp();

        canvas.SetActive(false);
        generalCanvas.SetActive(false);
        //Se destruye el mapa anterior
        Destroy(levelBuilder.board);

        current_map = mapIndex;
        menu_controller.SetLevelNumber(mapIndex + 1);

        //MAP
        _levelMap = _levelMap.Load(_levelMapSet.maps[mapIndex]);

        //int random_level_width = UnityEngine.Random.Range(2, 4);
        //LevelMap m =
        //    LevelMap.GetRandomMap(random_level_width,
        //        UnityEngine.Random.Range(random_level_width, random_level_width * UnityEngine.Random.Range(1, 3)));
        //m.Save("map_" + (level_maps_factory.maps.Length + ++created_map_index) + ".map");
        //SetupCanvasController.CreateMapSet();

        levelBuilder.BuildMap(_levelMap);
        var min = Math.Min(levelBuilder.board_resolution.width, levelBuilder.board_resolution.height);
        var max = Math.Max(levelBuilder.board_resolution.width, levelBuilder.board_resolution.height);
        mainCamera.orthographicSize = min + min * max / min;

        selection = new GameObject("Selection");
        selection.transform.parent = levelBuilder.board.transform;

        piece_size = levelBuilder.half_piece_size * 2;
        piece_margin = levelBuilder.half_margin * 2;

        piece_started_index = 0;
        screenFader.MakeTransition();
        currentState = levelStartState;
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
        switch (currentState)
        {
            case levelStartState:
            {
                OnLevelStart();
                break;
            }
            case idleState:
            {
                OnIdle();
                break;
            }
            case mouseDownState:
            {
                OnMouseDown();
                break;
            }
            case selectionMadeState:
            {
                OnSelectionMade();
                break;
            }
            case MovingState:
                AnimateMovement();
                break;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            print("Escape");
            OnApplicationQuit();
            ForceQuit();
        }
    }

    private void OnSelectionMade()
    {
        float deltaPosition = EdgedDistance();
        partial_position = selection.transform.position;
        currentState = MovingState;
        // Se va al proximo estado
        float nextX = selection_initial_position.x;
        float nextY = selection_initial_position.y;
        float positionOffset = (deltaPosition > 0
            ? (piece_size + piece_margin)
            : -(piece_size + piece_margin));

        if (moving_on_x) nextX += positionOffset;
        else nextY += positionOffset;
        final_position = new Vector3(nextX, nextY, 0);

        UpdatePiecesReplacement();
    }

    private void OnMouseDown()
    {
        Debug.Log("terryx down");
        mouse_current_position = mainCamera.ScreenPointToRay(Input.mousePosition).origin;
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
            currentState = selectionMadeState;
        }
    }

    private void OnIdle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("terry");
            var vRayStart = mainCamera.ScreenPointToRay(Input.mousePosition);
            mouse_start_position = vRayStart.origin;
            currentState = mouseDownState;
        }
    }

    private void OnLevelStart()
    {
        if (currentState != levelStartState) return;

        if (Input.GetMouseButtonDown(0)) EndLevelStartAnimation();

        if (piece_started_index > levelBuilder.pieces.Count - 1)
        {
            ConcludeLevelStart();
            return;
        }

        if (current_animation != null && current_animation.isPlaying) return;

        current_animation = ((GameObject) levelBuilder.pieces[piece_started_index++]).GetComponent<Animation>();
        current_animation.Play("PieceEnterAnimation");
    }

    private void ConcludeLevelStart()
    {
        currentState = idleState;
        canvas.SetActive(true);
        generalCanvas.SetActive(true);
        levelBuilder.starting_level = false;
    }

    public void ForceQuit()
    {
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    private void EndLevelStartAnimation()
    {
        for (var i = piece_started_index; i < levelBuilder.pieces.Count; i++)
            ((GameObject) levelBuilder.pieces[i]).transform.localScale = new Vector3(1, 1, 1);

        ConcludeLevelStart();
    }

    private static bool OverMenu()
    {
        return EventSystem.current.IsPointerOverGameObject(0) || EventSystem.current.IsPointerOverGameObject();
    }

    private void HandleMenuActions()
    {
        if (currentState != levelStartState && OverMenu())
        {
            // Si no se esta cargando ningun nivel y se dan clicks solo sobre el menu, prevalece el estado de menu
            currentState = MenuState;
        }

        if (currentState == MenuState && !OverMenu())
        {
            // Si estado == menu_activo y se toca algo fuera del menu se cambia el estado a idle
            currentState = idleState;
        }

        if (Input.GetKeyUp(KeyCode.Menu) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            // Si estado = inicio de nivel se termina la animacion de inicio de nivel y se esconde el menu
            if (currentState == levelStartState)
            {
                EndLevelStartAnimation();
                currentState = MenuState;
                menu_controller.ShowMenu();
            }

            // Si estado == idle, se intercambia estado de menu
            if (currentState == idleState)
            {
                currentState = MenuState;
                menu_controller.TogleMenu();
            }
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
            mainCamera.GetComponent<AudioSource>().PlayOneShot(movement_clip, .5f);
    }

    private void LevelWonSound()
    {
        if (MenuController.sound_enabled)
            mainCamera.GetComponent<AudioSource>().PlayOneShot(level_won_clip, .5f);
    }

    private void UpdatePiecesReplacement()
    {
        for (var i = selection.transform.childCount - 1; i >= 0; i--)
        {
            var pieceTransform = selection.transform.GetChild(i);
            var piece = pieceTransform.gameObject.GetComponent<Piece>();
            piece.SetInactiveOnMapState();
            if (!piece.GoesOut(movement_direction)) continue;

            // Si la ficha sale del tablero se genera automaticamente su reemplazo en la 
            // direccion opuesta para realizar la animacion correspondiente y se elimina
            // la ficha ke muere
            int nextX = piece.x, nextY = piece.y;
            if (moving_on_x) nextX = piece.x == 0 ? levelBuilder.level_width : -1;
            else nextY = piece.y == 0 ? levelBuilder.level_height : -1;

            var newPiece = levelBuilder.BuildPieceGameObject(new Vector2(nextX, nextY),
                LevelBuilder.piece_kind_normal);
            newPiece.transform.parent = selection.transform;
            newPiece.name = pieceTransform.name;

            // Manda a eliminar la ficha al terminar la animacion del movimiento actual
            piece.AutoDestroyOnFuture();
        }
    }

    private void AnimateMovement()
    {
        if (currentState == MovingState)
        {
            movement_interpolation += Time.deltaTime * movement_interpolation_speed;
            selection.transform.position = Vector3.MoveTowards(selection.transform.position, final_position,
                movement_interpolation);
            var absSqrDeltaPosition =
                Math.Abs(selection.transform.position.sqrMagnitude - final_position.sqrMagnitude);
            print("move!!!" + absSqrDeltaPosition);
            // if (absSqrDeltaPosition <= min_selection_distance)
            // {
            // Se fuerza la posicion final para evitar errores
            selection.transform.position = final_position;
            // Se reinicia el manejador de interpolacion
            movement_interpolation = 0;
            // Se actualiza la logica de las fichas movidas
            RemoveUpdateChildren(selection);
            currentState = idleState;
            // }
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
        Debug.Log("make sel");
        //Se toman las componentes del vector de direccion para conocer hacia donde desea
        //mover el usuario
        var componentX = new Vector2(heading.x, 0);
        var componentY = new Vector2(0, heading.y);
        double magnitudeX = componentX.sqrMagnitude;
        double magnitudeY = componentY.sqrMagnitude;

        Ray ray;
        if (magnitudeX > magnitudeY)
        {
            ray = new Ray(new Vector3(ray_start, mouse_start_position.y, 0), Vector2.right);
            moving_on_x = true;
            if (direction.x <= 0)
                movement_direction = RightDirection;
            else
                movement_direction = LeftDirection;
        }
        else
        {
            ray = new Ray(new Vector3(mouse_start_position.x, ray_start, 0), Vector2.up);
            moving_on_x = false;
            if (direction.y >= 0)
                movement_direction = UpDirection;
            else
                movement_direction = DownDirection;
        }

        SelectObjectsInRay(ray);
    }

    private void SelectObjectsInRay(Ray ray)
    {
        // Se captura la posicion inicial de la seleccion para uso posterior
        selection_initial_position = selection.transform.position;

        var hits = Physics.RaycastAll(ray);

        if (hits.Length <= 0) return;
        foreach (var hit in hits)
        {
            var piece = hit.transform;
            piece.parent = selection.transform;
        }
    }

    private void RemoveUpdateChildren(GameObject go)
    {
        for (var i = go.transform.childCount - 1; i >= 0; i--)
        {
            var index = i;
            if (movement_direction == DownDirection ||
                movement_direction == RightDirection)
                index = go.transform.childCount - i - 1;

            var pieceTransform = go.transform.GetChild(index);
            var piece = pieceTransform.gameObject.GetComponent<Piece>();

            if (piece.autoDestroy)
                piece.Destroy();
            else
            {
                piece.UpdateLogicalPosition(movement_direction);
                piece.SetActiveOnMapState();
                pieceTransform.transform.parent = levelBuilder.board.transform;
            }
        }

        if (levelBuilder.map.LevelWon())
        {
            LevelWonSound();
            print("You Win!!!");
            int nextMap = (current_map + 1) % _levelMapSet.maps.Length;
            user_data.lastUnlockedMap = Mathf.Max(user_data.lastUnlockedMap, nextMap);
            LoadMap(nextMap);
        }
        else
            currentState = idleState;
    }

    private void CleanSelection()
    {
        for (var i = selection.transform.childCount - 1; i >= 0; i--)
        {
            var child = selection.transform.GetChild(i);
            child.transform.parent = levelBuilder.board.transform;
        }
    }

    public void OnApplicationPause(bool pauseStatus)
    {
        print("OnApplicationPause");
        user_data?.Save();
    }

    public void OnApplicationQuit()
    {
        print("OnApplicationQuit");
        user_data?.Save();
    }
}