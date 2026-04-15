// PauseScreen.cs
// Manages the pause menu: resume and quit buttons.

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

        private void OnEnable()
        {
            if (resumeButton) resumeButton.onClick.AddListener(OnResumeClicked);
            if (quitButton) quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnDisable()
        {
            if (resumeButton) resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (quitButton) quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnResumeClicked()
        {
            GameManager.Instance.ResumeGame();
            UIManager.Instance?.ShowScreen<HUDScreen>();
        }

        private void OnQuitClicked()
        {
            GameManager.Instance.QuitGame();
        }
    }
}