// FloatingScoreManager.cs
// Manages simple notification popups (e.g., score notifications) at a fixed screen location.

using UnityEngine;
using System.Collections.Generic;

namespace YesChef.UI
{
    public class FloatingScoreManager : MonoBehaviour
    {
        public static FloatingScoreManager Instance { get; private set; }

        [Header("Pool Settings")]
        [SerializeField] private GameObject floatingScorePrefab;   // UI element on a Screen Space - Overlay canvas
        [SerializeField] private Transform popupContainer;        // Parent transform (e.g., the Canvas)
        [SerializeField] private int poolSize = 10;

        private Queue<FloatingScorePopup> _popupPool;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            BuildPool();
        }

        private void BuildPool()
        {
            _popupPool = new Queue<FloatingScorePopup>();
            if (floatingScorePrefab == null || popupContainer == null) return;

            for (int i = 0; i < poolSize; i++)
            {
                var go = Instantiate(floatingScorePrefab, popupContainer);
                var popup = go.GetComponent<FloatingScorePopup>();
                go.SetActive(false);
                _popupPool.Enqueue(popup);
            }
        }

        /// <summary>
        /// Shows a score notification (ignores windowIndex, uses fixed position).
        /// </summary>
        public void ShowFloatingScore(int windowIndex, int score)
        {
            if (_popupPool == null || _popupPool.Count == 0) return;

            var popup = _popupPool.Dequeue();
            popup.gameObject.SetActive(true);
            popup.Show(
                text: score >= 0 ? $"+{score}" : score.ToString(),
                color: score >= 0 ? Color.green : Color.red,
                onComplete: () => ReturnToPool(popup)
            );
        }

        private void ReturnToPool(FloatingScorePopup popup)
        {
            if (popup == null) return;
            popup.gameObject.SetActive(false);
            _popupPool.Enqueue(popup);
        }
    }
}