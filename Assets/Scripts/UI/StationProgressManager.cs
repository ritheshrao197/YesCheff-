using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;
using YesChef.Stations;

namespace YesChef.UI
{
    public class StationProgressManager : MonoBehaviour
    {
        [Header("Chop Progress")]
        [SerializeField] private Slider chopProgressSlider;
        [SerializeField] private GameObject chopProgressPanel;

        [Header("Stove Progress")]
        [SerializeField] private Slider[] stoveSlotSliders;
        [SerializeField] private GameObject[] stoveSlotPanels;

        private void OnEnable()
        {
            GameEvents.ChopStarted += ShowChopProgress;
            GameEvents.ChopProgressChanged += UpdateChopProgress;
            GameEvents.ChopCompleted += HideChopProgress;

            GameEvents.StoveSlotStarted += ShowStoveSlot;
            GameEvents.StoveSlotProgressChanged += UpdateStoveSlot;
            GameEvents.StoveSlotCompleted += HideStoveSlot;
        }

        private void OnDisable()
        {
            GameEvents.ChopStarted -= ShowChopProgress;
            GameEvents.ChopProgressChanged -= UpdateChopProgress;
            GameEvents.ChopCompleted -= HideChopProgress;

            GameEvents.StoveSlotStarted -= ShowStoveSlot;
            GameEvents.StoveSlotProgressChanged -= UpdateStoveSlot;
            GameEvents.StoveSlotCompleted -= HideStoveSlot;
        }

        private void ShowChopProgress(Table table) => SetPanelActive(chopProgressPanel, true);
        private void HideChopProgress(Table table) => SetPanelActive(chopProgressPanel, false);

        private void UpdateChopProgress(Table table, float progress)
        {
            if (chopProgressSlider != null)
            {
                chopProgressSlider.value = progress;
            }
        }

        private void ShowStoveSlot(Stove stove, int slotIndex) => SetPanelActive(stoveSlotPanels, slotIndex, true);
        private void HideStoveSlot(Stove stove, int slotIndex) => SetPanelActive(stoveSlotPanels, slotIndex, false);

        private void UpdateStoveSlot(Stove stove, int slotIndex, float progress)
        {
            if (stoveSlotSliders != null && slotIndex >= 0 && slotIndex < stoveSlotSliders.Length && stoveSlotSliders[slotIndex] != null)
            {
                stoveSlotSliders[slotIndex].value = progress;
            }
        }

        private static void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        private static void SetPanelActive(GameObject[] panels, int index, bool active)
        {
            if (panels != null && index >= 0 && index < panels.Length && panels[index] != null)
            {
                panels[index].SetActive(active);
            }
        }
    }
}
