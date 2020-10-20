using UnityEngine;

namespace Scenes.game
{
    public class Piece : MonoBehaviour
    {
        private AudioSource audio_source;
        public bool autoDestroy;
        private LevelMap map;
        public int x;
        public int y;

        public void Init(int pX, int pY, LevelMap pMap)
        {
            x = pX;
            y = pY;
            map = pMap;
            autoDestroy = false;
            audio_source = Camera.main.GetComponent<AudioSource>();
        }

        public void AutoDestroyOnFuture()
        {
            autoDestroy = true;
        }

        public void SetInactiveOnMapState()
        {
            map.AddPendingPosition(x, y);
        }

        public void SetActiveOnMapState()
        {
            map.RemovePendingPosition(x, y);
        }

        public void UpdateLogicalPosition(int movementDirection)
        {
            switch (movementDirection)
            {
                case GameManager.UpDirection:
                    y += 1;
                    break;
                case GameManager.RightDirection:
                    x += 1;
                    break;
                case GameManager.DownDirection:
                    y -= 1;
                    break;
                case GameManager.LeftDirection:
                    x -= 1;
                    break;
            }
        }

        public bool GoesOut(int direction)
        {
            var goesOut = false;
            switch (direction)
            {
                case GameManager.UpDirection:
                    goesOut = y >= map.level_height - 1;
                    break;
                case GameManager.RightDirection:
                    goesOut = x >= map.level_width - 1;
                    break;
                case GameManager.DownDirection:
                    goesOut = y <= 0;
                    break;
                case GameManager.LeftDirection:
                    goesOut = x <= 0;
                    break;
            }

            return goesOut;
        }

        public void Destroy()
        {
            gameObject.transform.parent = null;
            Destroy(gameObject);
        }
    }
}