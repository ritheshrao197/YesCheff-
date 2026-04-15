using UnityEngine;
using UnityEngine.UI;
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
            Table.OnChopStarted += ShowChopProgress;
            Table.OnChopProgress += UpdateChopProgress;
            Table.OnChopComplete += HideChopProgress;

            Stove.OnSlotStarted += ShowStoveSlot;
            Stove.OnSlotProgress += UpdateStoveSlot;
            Stove.OnSlotComplete += HideStoveSlot;
        }

        private void OnDisable()
        {
            Table.OnChopStarted -= ShowChopProgress;
            Table.OnChopProgress -= UpdateChopProgress;
            Table.OnChopComplete -= HideChopProgress;

            Stove.OnSlotStarted -= ShowStoveSlot;
            Stove.OnSlotProgress -= UpdateStoveSlot;
            Stove.OnSlotComplete -= HideStoveSlot;
        }

        private void ShowChopProgress() => SetPanelActive(chopProgressPanel, true);
        private void HideChopProgress() => SetPanelActive(chopProgressPanel, false);
        private void UpdateChopProgress(float t)
        {
            if (chopProgressSlider) chopProgressSlider.value = t;
        }

        private void ShowStoveSlot(int slot) => SetPanelActive(stoveSlotPanels, slot, true);
        private void HideStoveSlot(int slot) => SetPanelActive(stoveSlotPanels, slot, false);
        private void UpdateStoveSlot(int slot, float t)
        {
            if (stoveSlotSliders != null && slot >= 0 && slot < stoveSlotSliders.Length && stoveSlotSliders[slot])
                stoveSlotSliders[slot].value = t;
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel) panel.SetActive(active);
        }

        private void SetPanelActive(GameObject[] panels, int index, bool active)
        {
            if (panels != null && index >= 0 && index < panels.Length && panels[index])
                panels[index].SetActive(active);
        }
    }
}