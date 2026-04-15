// PauseScreen.cs
// Manages the pause menu controls.

using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;

namespace YesChef.UI
{
    public class PauseScreen : BaseScreen
    {
        [Header("Pause Screen")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;
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
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void OnDisable()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(OnResumeClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitClicked);
            }
        }

        private void OnResumeClicked()
        {
            gameManager?.ResumeGame();
        }

        private void OnQuitClicked()
        {
            gameManager?.QuitGame();
        }
    }
}
