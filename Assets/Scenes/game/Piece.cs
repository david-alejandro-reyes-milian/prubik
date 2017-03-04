using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool active;
    private AudioSource audio_source;
    public bool auto_destroy;
    private LevelMap map;
    public int x;
    public int y;

    public Piece Init(int x, int y, LevelMap map)
    {
        this.x = x;
        this.y = y;
        active = true;
        this.map = map;
        auto_destroy = false;
        audio_source = Camera.main.GetComponent<AudioSource>();
        return this;
    }

    public void AutoDestroyOnFuture()
    {
        auto_destroy = true;
    }

    public void SetInactiveOnMapState()
    {
        map.AddPendingPosition(x, y);
    }

    public void SetActiveOnMapState()
    {
        map.RemovePendingPosition(x, y);
    }

    public void UpdateLogicalPosition(int movement_direction)
    {
        switch (movement_direction)
        {
            case GameManager.up_direction:
                y += 1;
                break;
            case GameManager.right_direction:
                x += 1;
                break;
            case GameManager.down_direction:
                y -= 1;
                break;
            case GameManager.left_direction:
                x -= 1;
                break;
        }
    }

    public bool GoesOut(int direction)
    {
        bool goes_out = false;
        switch (direction)
        {
            case GameManager.up_direction:
                goes_out = y >= map.level_height - 1;
                break;
            case GameManager.right_direction:
                goes_out = x >= map.level_width - 1;
                break;
            case GameManager.down_direction:
                goes_out = y <= 0;
                break;
            case GameManager.left_direction:
                goes_out = x <= 0;
                break;
        }
        return goes_out;
    }

    public void Destroy()
    {
        gameObject.transform.parent = null;
        Destroy(gameObject);
    }
}