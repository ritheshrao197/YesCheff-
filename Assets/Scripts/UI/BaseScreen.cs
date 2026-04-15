// BaseScreen.cs
// Base class for all UI screens. Handles common lifecycle methods.

using UnityEngine;

namespace YesChef.UI
{
    public abstract class BaseScreen : MonoBehaviour
    {
        [Header("Base Screen")]
        [SerializeField] protected CanvasGroup canvasGroup; // optional, for fade/block raycasts

        protected virtual void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        public virtual void ResetScreen()
        {
            // Override in derived classes if needed
        }
    }
}