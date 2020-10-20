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
        if (game_manager.currentState != GameManager.MovingState)
        {
            game_manager.currentState = GameManager.MenuState;
            menu_controller.TogleMenu();
        }

    }
}
