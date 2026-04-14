// FloatingScorePopup.cs
// Pooled UI element that floats upward and fades out.
// Called by UIManager; returns itself to the pool via callback.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace YesChef.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class FloatingScorePopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private float    floatSpeed  = 60f;
        [SerializeField] private float    lifetime    = 2f;

        private RectTransform _rt;
        private Action _onComplete;

        private void Awake() => _rt = GetComponent<RectTransform>();

        public void Show(string text, Color color, Vector2 anchoredPos, Action onComplete)
        {
            _onComplete = onComplete;

            if (label) { label.text = text; label.color = color; }
            _rt.anchoredPosition = anchoredPos;

            StopAllCoroutines();
            StartCoroutine(AnimateRoutine());
        }

        private IEnumerator AnimateRoutine()
        {
            float elapsed = 0f;
            Color startColor = label ? label.color : Color.white;
            Vector2 startPos = _rt.anchoredPosition;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;

                // Float upward
                _rt.anchoredPosition = startPos + Vector2.up * floatSpeed * elapsed;

                // Fade out in second half
                if (label)
                {
                    float alpha = t < 0.5f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f);
                    label.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                }

                yield return null;
            }

            _onComplete?.Invoke();
        }
    }
}
