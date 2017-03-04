using UnityEngine;
using System.Collections;

public class GeneralCanvasController : MonoBehaviour
{
    MenuController menu_controller;
    GameManager game_manager;
    void Awake()
    {
        menu_controller = GameObject.Find("/MenuCanvas/Menu").GetComponent<MenuController>();
        game_manager = GameObject.Find("Main Camera").GetComponent<GameManager>();
    }

    void OnMouseUp()
    {
        if (game_manager.current_state != GameManager.moving_state)
        {
            game_manager.current_state = GameManager.menu_state;
            menu_controller.TogleMenu();
        }

    }
}
