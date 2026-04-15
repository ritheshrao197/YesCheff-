// FloatingScoreManager.cs
// Manages pooled floating score notifications.

using System.Collections.Generic;
using UnityEngine;
using YesChef.Core;

namespace YesChef.UI
{
    public class FloatingScoreManager : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private GameObject floatingScorePrefab;
        [SerializeField] private Transform popupContainer;
        [SerializeField] private int poolSize = 10;

        private Queue<FloatingScorePopup> _popupPool;

        private void Awake()
        {
            BuildPool();
        }

        private void OnEnable()
        {
            GameEvents.DeliveryScored += ShowFloatingScore;
        }

        private void OnDisable()
        {
            GameEvents.DeliveryScored -= ShowFloatingScore;
        }

        public void ShowFloatingScore(int windowIndex, int score)
        {
            if (_popupPool == null || _popupPool.Count == 0)
            {
                return;
            }

            FloatingScorePopup popup = _popupPool.Dequeue();
            popup.gameObject.SetActive(true);
            popup.Show(
                text: score >= 0 ? $"+{score}" : score.ToString(),
                color: score >= 0 ? Color.green : Color.red,
                onComplete: () => ReturnToPool(popup));
        }

        private void BuildPool()
        {
            _popupPool = new Queue<FloatingScorePopup>();
            if (floatingScorePrefab == null || popupContainer == null)
            {
                return;
            }

            for (int i = 0; i < poolSize; i++)
            {
                GameObject popupObject = Instantiate(floatingScorePrefab, popupContainer);
                FloatingScorePopup popup = popupObject.GetComponent<FloatingScorePopup>();
                popupObject.SetActive(false);
                _popupPool.Enqueue(popup);
            }
        }

        private void ReturnToPool(FloatingScorePopup popup)
        {
            if (popup == null)
            {
                return;
            }

            popup.gameObject.SetActive(false);
            _popupPool.Enqueue(popup);
        }
    }
}
