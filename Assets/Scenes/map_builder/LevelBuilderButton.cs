using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelBuilderButton : MonoBehaviour
{

    // Use this for initialization
    Material final_position_sprite;
    Material current_position_sprite;
    Material normal_position_sprite;
    int current_state;
    public int x, y;

    public void Start()
    {
        current_state = 0;
        final_position_sprite = Resources.Load("Materials/FinalPositionMat", typeof(Material)) as Material;
        current_position_sprite = Resources.Load("Materials/CubeMat", typeof(Material)) as Material;
        normal_position_sprite = Resources.Load("Materials/GameBoardMat", typeof(Material)) as Material;
        Toggle();
    }

    public void ChangeToFinal()
    {
        gameObject.GetComponent<Image>().material = final_position_sprite;
        CreateLevelCanvasController.final_positions.Add(FillDigits(x) + "" + FillDigits(y));
        CreateLevelCanvasController.current_positions.Remove(FillDigits(x) + "" + FillDigits(y));

    }
    public void ChangeToNormal()
    {
        gameObject.GetComponent<Image>().material = current_position_sprite;
        CreateLevelCanvasController.final_positions.Remove(FillDigits(x) + "" + FillDigits(y));
        CreateLevelCanvasController.current_positions.Add(FillDigits(x) + "" + FillDigits(y));
    }
    public void ChangeToNothing()
    {
        gameObject.GetComponent<Image>().material = normal_position_sprite;
        CreateLevelCanvasController.final_positions.Remove(FillDigits(x) + "" + FillDigits(y));
        CreateLevelCanvasController.current_positions.Remove(FillDigits(x) + "" + FillDigits(y));
    }
    public void Toggle()
    {
        switch (current_state)
        {
            case 0:
                ChangeToNothing();
                break;
            case 1:
                ChangeToNormal();
                break;
            case 2:
                ChangeToFinal();
                break;
        }

        current_state = (current_state + 1) % 3;
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
    public void Destroy() {
        Destroy(gameObject);
    }
}
