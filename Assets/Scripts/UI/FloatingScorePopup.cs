using TMPro;
using UnityEngine;

public class FloatingScorePopup : MonoBehaviour
{
    private const float PopupTravelDistance = 30f;
    private const float FullyOpaqueAlpha = 1f;

    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private AnimationCurve moveCurve; // optional

    public void Show(string text, Color color, System.Action onComplete = null)
    {
        textComponent.text = text;
        textComponent.color = color;
        gameObject.SetActive(true);
        StartCoroutine(AnimateAndFade(onComplete));
    }

    private System.Collections.IEnumerator AnimateAndFade(System.Action onComplete)
    {
        // Simple fade out and scale, or move up slightly
        float elapsed = 0f;
        Color originalColor = textComponent.color;
        Vector3 startPos = transform.localPosition;
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;
            transform.localPosition = startPos + Vector3.up * (t * PopupTravelDistance);
            textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, FullyOpaqueAlpha - t);
            yield return null;
        }
        onComplete?.Invoke();
    }
}
