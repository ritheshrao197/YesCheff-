// StartScreen.cs
// Manages the start screen controls.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;

namespace YesChef.UI
{
    public class StartScreen : BaseScreen
    {
        [Header("Start Screen")]
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text controlsText;
        [SerializeField] private GameManager gameManager;

        protected override void Awake()
        {
            base.Awake();

            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
        }

        private void OnEnable()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartClicked);
            }
        }

        private void OnDisable()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartClicked);
            }
        }

        public void SetControlsText(string text)
        {
            if (controlsText != null)
            {
                controlsText.text = text;
            }
        }

        private void OnStartClicked()
        {
            gameManager?.StartGame();
        }
    }
}
