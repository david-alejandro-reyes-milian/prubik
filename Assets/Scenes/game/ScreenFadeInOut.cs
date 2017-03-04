using UnityEngine;
using System.Collections;

public class ScreenFadeInOut : MonoBehaviour
{

    public float fade_speed = 25f;
    public bool scene_starting = false;
    public bool scene_ending = false;
    private GUITexture guiTexture;
    void Awake()
    {
        guiTexture = GetComponent<GUITexture>();
        guiTexture.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
    }

    void Update()
    {
        if (scene_starting && !scene_ending)
        {
            StartScene();
        }
        if (scene_ending)
        {
            EndScene();
        }
    }
    public void MakeTransition()
    {
        scene_ending = true;
    }

    public void StartScene()
    {
        FadeToClear();
        if (guiTexture.color.a <= 0.05f)
        {
            guiTexture.color = Color.clear;
            scene_starting = false;
        }
    }
    public void EndScene()
    {
        FadeToBlack();
        if (guiTexture.color.a >= 0.95f)
        {
            guiTexture.enabled = true;
            guiTexture.color = Color.black;
            scene_ending = false;
            scene_starting = true;
        }
    }
    void FadeToClear()
    {
        guiTexture.color = Color.Lerp(guiTexture.color, Color.clear, fade_speed * Time.deltaTime);
    }
    void FadeToBlack()
    {
        guiTexture.color = Color.Lerp(guiTexture.color, Color.black, fade_speed * Time.deltaTime);
    }
}
