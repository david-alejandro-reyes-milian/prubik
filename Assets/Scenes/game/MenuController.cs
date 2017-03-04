using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static bool sound_enabled;
    private Animation animation;
    private AudioClip menu_clip;
    private float menu_clip_volume = .5f;
    private bool menu_is_showing;
    private Sprite music_off_sprite;
    private Sprite music_on_sprite;
    private Transform sound_button;
    public GameManager game_manager;
    public Text level_number;
    public Text level_number_shadow;


    public void Awake()
    {
        animation = gameObject.GetComponent<Animation>();

        menu_clip = Resources.Load("Sounds/m1", typeof(AudioClip)) as AudioClip;
        music_on_sprite = Resources.Load("Sprites/mute_button_on", typeof(Sprite)) as Sprite;
        music_off_sprite = Resources.Load("Sprites/mute_button_off", typeof(Sprite)) as Sprite;
        sound_button = gameObject.transform.GetChild(2);
        sound_enabled = true;
        menu_is_showing = false;

        //level_number.GetComponentInParent<LevelNumberContainerAutoPosition>().SetPosition();
    }

    public void TogleMenu()
    {
        if (menu_is_showing) HideMenu();
        else ShowMenu();
    }

    public void ShowMenu()
    {
        if (menu_is_showing)
            return;
        PlayMenuClick();
        animation["MenuAnimation"].time = 0;
        animation["MenuAnimation"].speed = 1;
        animation.Play("MenuAnimation");
        menu_is_showing = true;
    }

    public void HideMenu()
    {
        if (!menu_is_showing)
            return;
        animation["MenuAnimation"].time = animation["MenuAnimation"].length / 15;
        animation["MenuAnimation"].speed = -1;
        animation.Play("MenuAnimation");
        menu_is_showing = false;
    }

    public void PlayMenuClick()
    {
        if (sound_enabled)
            Camera.main.GetComponent<AudioSource>().PlayOneShot(menu_clip, menu_clip_volume);
    }


    public void ToggleSound()
    {
        sound_enabled = !sound_enabled;
        sound_button.GetComponent<Image>().sprite = sound_enabled ? music_on_sprite : music_off_sprite;
        PlayMenuClick();
    }

    public void LoadNextMap()
    {
        // Para ver todos los niveles existentes
        //int next_map = (game_manager.current_map + 1) % (GameManager.level_maps_factory.maps.Length);
        int next_map = (game_manager.current_map + 1) % (game_manager.GetLastUnlockedMap());
        game_manager.LoadMap(next_map);
    }
    public void LoadPrevMap()
    {
        int prev_map = game_manager.current_map == 0 ? game_manager.GetLastUnlockedMap() - 1 : (game_manager.current_map - 1) % game_manager.GetLastUnlockedMap();
        game_manager.LoadMap(prev_map);
    }
    public void Retry()
    {
        game_manager.LoadMap(game_manager.current_map);
    }

    public void SetLevelNumber(int level)
    {
        level_number.text = Fill(level);
        level_number_shadow.text = Fill(level);
    }
    private string Fill(int number)
    {
        int expected_length = 3;
        int length = (number + "").Length;
        string res = "";
        for (int i = length; i < expected_length; i++)
            res += "0";
        res += number;
        return res;
    }
}