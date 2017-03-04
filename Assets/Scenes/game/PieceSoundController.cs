using UnityEngine;

public class PieceSoundController : MonoBehaviour
{
    private AudioSource audio_source;

    public PieceSoundController Init()
    {
        audio_source = Camera.main.GetComponent<AudioSource>();
        return this;
    }

    public void PlayAnimationEnterSound()
    {
        if (MenuController.sound_enabled)
        {
            GameManager.piece_enter_clip_index = (GameManager.piece_enter_clip_index + Random.RandomRange(1, 5)) % 11;
            audio_source.PlayOneShot(
                GameManager.piece_enter_clips[GameManager.piece_enter_clip_index]);
        }
    }
}