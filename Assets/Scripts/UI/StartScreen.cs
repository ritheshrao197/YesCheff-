// StartScreen.cs
// Manages the start screen: shows controls, start button.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YesChef.Core;

namespace YesChef.UI
{
    public class StartScreen : BaseScreen
    {
        [Header("Start Screen")]
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text controlsText;

        private void OnEnable()
        {
            if (startButton)
                startButton.onClick.AddListener(OnStartClicked);
        }

        private void OnDisable()
        {
            if (startButton)
                startButton.onClick.RemoveListener(OnStartClicked);
        }

        private void OnStartClicked()
        {
            UIManager.Instance?.ShowScreen<HUDScreen>();
            GameManager.Instance.StartGame();
        }

        public void SetControlsText(string text)
        {
            if (controlsText) controlsText.text = text;
        }
    }
}
