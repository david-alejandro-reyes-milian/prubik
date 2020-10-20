using UnityEngine;
using UnityEngine.UI;

namespace Scenes.game
{
    public class ScreenFadeInOut : MonoBehaviour
    {
        public float fadeSpeed = 25f;
        public bool sceneStarting;
        public bool sceneEnding;
        private Image image;

        private void Awake()
        {
            image = gameObject.AddComponent<Image>();
            image.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        }

        private void Update()
        {
            if (sceneStarting) StartScene();
            else if (sceneEnding) EndScene();
        }

        public void MakeTransition() => sceneEnding = true;

        private void StartScene()
        {
            FadeToClear();
            if (image.color.a <= 0.05f)
            {
                image.color = Color.clear;
                sceneStarting = false;
            }
        }

        private void EndScene()
        {
            image.color = Color.Lerp(image.color, Color.black, fadeSpeed * Time.deltaTime);
            var imageVisible = image.color.a >= 0.95f;

            if (!imageVisible) return;
            image.enabled = true;
            image.color = Color.black;
            sceneEnding = false;
            sceneStarting = true;
        }

        private void FadeToClear() =>
            image.color = Color.Lerp(image.color, Color.clear, fadeSpeed * Time.deltaTime);
    }
}