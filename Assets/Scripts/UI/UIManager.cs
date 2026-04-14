// UIManager.cs
// Manages all in-game UI: HUD (score, timer), order windows, station progress bars,
// floating score popups (pooled), start screen, pause screen, game over screen.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YesChef.Core;
using YesChef.Ingredients;
using YesChef.Orders;
using YesChef.Player;
using YesChef.Stations;
using YesChef.Systems;

namespace YesChef.UI
{
    public class UIManager : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════════════
        // Inspector refs
        // ═══════════════════════════════════════════════════════════════════

        [Header("Screens")]
        [SerializeField] private GameObject startScreen;
        [SerializeField] private GameObject hudScreen;
        [SerializeField] private GameObject pauseScreen;
        [SerializeField] private GameObject gameOverScreen;

        [Header("HUD")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text interactionPromptText;

        [Header("Game Over")]
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text newHighScoreText;

        [Header("Order Windows (4 panels, assign in order)")]
        [SerializeField] private OrderWindowUI[] orderWindowUIs;

        [Header("Station Progress")]
        [SerializeField] private Slider chopProgressSlider;
        [SerializeField] private GameObject chopProgressPanel;
        [SerializeField] private Slider[] stoveSlotSliders;   // length 2
        [SerializeField] private GameObject[] stoveSlotPanels;

        [Header("Floating Score Pool")]
        [SerializeField] private GameObject floatingScorePrefab;
        [SerializeField] private Transform  popupCanvas;
        [SerializeField] private int poolSize = 10;

        [Header("Held Item")]
        [SerializeField] private TMP_Text heldItemText;

        // ═══════════════════════════════════════════════════════════════════
        // Private state
        // ═══════════════════════════════════════════════════════════════════
        private Queue<FloatingScorePopup> _popupPool;
        private Camera _mainCamera;

        // ═══════════════════════════════════════════════════════════════════
        // Unity lifecycle
        // ═══════════════════════════════════════════════════════════════════
        private void Awake()
        {
            _mainCamera = Camera.main;
            BuildPopupPool();
        }

        private void OnEnable()
        {
            // Timer
            TimerSystem.OnTimerTick      += UpdateTimer;

            // Score
            ScoreSystem.OnScoreChanged         += UpdateScore;
            ScoreSystem.OnHighScoreChanged      += UpdateHighScore;
            ScoreSystem.OnGameEndedWithHighScore += ShowGameOver;

            // Player
            PlayerController.OnNearInteractable += ShowPrompt;
            PlayerController.OnLeftInteractable  += HidePrompt;
            PlayerController.OnPickedUp          += ShowHeldItem;
            PlayerController.OnDropped           += HideHeldItem;

            // Orders
            OrderManager.OnOrderAssigned  += OnOrderAssigned;
            OrderManager.OnWindowCleared  += OnWindowCleared;
            CustomerWindow.OnDeliveryScored += ShowFloatingScore;

            // Stations
            Table.OnChopStarted   += ShowChopProgress;
            Table.OnChopProgress  += UpdateChopProgress;
            Table.OnChopComplete  += HideChopProgress;

            Stove.OnSlotStarted   += ShowStoveSlot;
            Stove.OnSlotProgress  += UpdateStoveSlot;
            Stove.OnSlotComplete  += OnStoveDone;
        }

        private void OnDisable()
        {
            TimerSystem.OnTimerTick      -= UpdateTimer;
            ScoreSystem.OnScoreChanged         -= UpdateScore;
            ScoreSystem.OnHighScoreChanged      -= UpdateHighScore;
            ScoreSystem.OnGameEndedWithHighScore -= ShowGameOver;
            PlayerController.OnNearInteractable -= ShowPrompt;
            PlayerController.OnLeftInteractable  -= HidePrompt;
            PlayerController.OnPickedUp          -= ShowHeldItem;
            PlayerController.OnDropped           -= HideHeldItem;
            OrderManager.OnOrderAssigned  -= OnOrderAssigned;
            OrderManager.OnWindowCleared  -= OnWindowCleared;
            CustomerWindow.OnDeliveryScored -= ShowFloatingScore;
            Table.OnChopStarted   -= ShowChopProgress;
            Table.OnChopProgress  -= UpdateChopProgress;
            Table.OnChopComplete  -= HideChopProgress;
            Stove.OnSlotStarted   -= ShowStoveSlot;
            Stove.OnSlotProgress  -= UpdateStoveSlot;
            Stove.OnSlotComplete  -= OnStoveDone;
        }

        private void Start()
        {
            ShowStartScreen();
        }

        // ═══════════════════════════════════════════════════════════════════
        // Screen transitions
        // ═══════════════════════════════════════════════════════════════════
        public void ShowStartScreen()
        {
            SetScreens(start: true);
        }

        public void OnStartButtonClicked()
        {
            SetScreens(hud: true);
            GameManager.Instance.StartGame();
        }

        public void OnPauseButtonClicked()
        {
            bool isPaused = GameManager.Instance.CurrentState == GameState.Paused;
            if (isPaused)
            {
                SetScreens(hud: true);
                GameManager.Instance.ResumeGame();
            }
            else
            {
                SetScreens(hud: true, pause: true);
                GameManager.Instance.PauseGame();
            }
        }

        public void OnResumeButtonClicked()
        {
            SetScreens(hud: true);
            GameManager.Instance.ResumeGame();
        }

        public void OnRestartButtonClicked()
        {
            SetScreens(hud: true);
            GameManager.Instance.RestartGame();
        }

        public void OnQuitButtonClicked()
        {
            GameManager.Instance.QuitGame();
        }

        private void ShowGameOver(bool isNewHighScore)
        {
            SetScreens(gameOver: true);
            finalScoreText.text    = $"Final Score: {FindObjectOfType<ScoreSystem>().CurrentScore}";
            newHighScoreText.gameObject.SetActive(isNewHighScore);
        }

        private void SetScreens(bool start = false, bool hud = false, bool pause = false, bool gameOver = false)
        {
            if (startScreen)   startScreen.SetActive(start);
            if (hudScreen)     hudScreen.SetActive(hud);
            if (pauseScreen)   pauseScreen.SetActive(pause);
            if (gameOverScreen) gameOverScreen.SetActive(gameOver);
        }

        // ═══════════════════════════════════════════════════════════════════
        // HUD updates
        // ═══════════════════════════════════════════════════════════════════
        private void UpdateTimer(float remaining)
        {
            int mins = Mathf.FloorToInt(remaining / 60f);
            int secs = Mathf.FloorToInt(remaining % 60f);
            timerText.text = $"{mins}:{secs:D2}";

            // Turn red in final 30 seconds
            timerText.color = remaining <= 30f ? Color.red : Color.white;
        }

        private void UpdateScore(int score)
        {
            if (scoreText) scoreText.text = $"Score: {score}";
        }

        private void UpdateHighScore(int hs)
        {
            if (highScoreText) highScoreText.text = $"Best: {hs}";
        }

        private void ShowPrompt(string prompt)
        {
            if (interactionPromptText) { interactionPromptText.text = prompt; interactionPromptText.gameObject.SetActive(true); }
        }

        private void HidePrompt()
        {
            if (interactionPromptText) interactionPromptText.gameObject.SetActive(false);
        }

        private void ShowHeldItem(Ingredient ing)
        {
            if (heldItemText) { heldItemText.text = $"Holding: {ing.Data.displayName} ({ing.State})"; heldItemText.gameObject.SetActive(true); }
        }

        private void HideHeldItem()
        {
            if (heldItemText) heldItemText.gameObject.SetActive(false);
        }

        // ═══════════════════════════════════════════════════════════════════
        // Order window callbacks
        // ═══════════════════════════════════════════════════════════════════
        private void OnOrderAssigned(Order order, int windowIndex)
        {
            if (windowIndex >= 0 && windowIndex < orderWindowUIs.Length)
                orderWindowUIs[windowIndex].SetOrder(order);
        }

        private void OnWindowCleared(int windowIndex)
        {
            if (windowIndex >= 0 && windowIndex < orderWindowUIs.Length)
                orderWindowUIs[windowIndex].ClearOrder();
        }

        // ═══════════════════════════════════════════════════════════════════
        // Station progress
        // ═══════════════════════════════════════════════════════════════════
        private void ShowChopProgress()   { if (chopProgressPanel) chopProgressPanel.SetActive(true); }
        private void HideChopProgress()   { if (chopProgressPanel) chopProgressPanel.SetActive(false); }
        private void UpdateChopProgress(float t) { if (chopProgressSlider) chopProgressSlider.value = t; }

        private void ShowStoveSlot(int slot)  { if (slot < stoveSlotPanels.Length) stoveSlotPanels[slot].SetActive(true); }
        private void OnStoveDone(int slot)    { if (slot < stoveSlotPanels.Length) stoveSlotPanels[slot].SetActive(false); }
        private void UpdateStoveSlot(int slot, float t) { if (slot < stoveSlotSliders.Length) stoveSlotSliders[slot].value = t; }

        // ═══════════════════════════════════════════════════════════════════
        // Floating score popup (object pool)
        // ═══════════════════════════════════════════════════════════════════
        private void BuildPopupPool()
        {
            _popupPool = new Queue<FloatingScorePopup>();
            if (floatingScorePrefab == null || popupCanvas == null) return;

            for (int i = 0; i < poolSize; i++)
            {
                var go = Instantiate(floatingScorePrefab, popupCanvas);
                var popup = go.GetComponent<FloatingScorePopup>();
                go.SetActive(false);
                _popupPool.Enqueue(popup);
            }
        }

        private void ShowFloatingScore(int windowIndex, int score)
        {
            if (_popupPool == null || _popupPool.Count == 0) return;

            // World-space position of the window
            Vector3 worldPos = Vector3.zero;
            if (windowIndex < orderWindowUIs.Length)
                worldPos = orderWindowUIs[windowIndex].transform.position + Vector3.up * 1.5f;

            // Convert to screen space
            Vector2 screenPos = _mainCamera.WorldToScreenPoint(worldPos);

            var popup = _popupPool.Dequeue();
            popup.gameObject.SetActive(true);
            popup.Show(score >= 0 ? $"+{score}" : score.ToString(),
                       score >= 0 ? Color.green : Color.red,
                       screenPos,
                       () => ReturnToPool(popup));
        }

        private void ReturnToPool(FloatingScorePopup popup)
        {
            popup.gameObject.SetActive(false);
            _popupPool.Enqueue(popup);
        }
    }
}
