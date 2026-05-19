using TMPro;
using UnityEngine;

public class SaveLoadUI : MonoBehaviour {
    [SerializeField] GameObject saveLoadPanel;
    [SerializeField] FirebaseHandler firebaseHandler;
    // Assign one label per load button in the Inspector (slots 1–4)
    [SerializeField] TextMeshProUGUI[] loadButtonLabels;

    public void SetVisibility(bool state) {
        saveLoadPanel.SetActive(state);
        if (state) RefreshSlotNames();
    }

    public void ToggleVisibility() {
        bool next = !saveLoadPanel.activeSelf;
        saveLoadPanel.SetActive(next);
        if (next) RefreshSlotNames();
    }

    public void Save(int slot) {
        firebaseHandler.SaveEnvironment(slot, "Slot " + slot);
    }

    public void Load(int slot) => firebaseHandler.LoadEnvironment(slot);

    private void RefreshSlotNames() {
        if (loadButtonLabels == null) return;
        for (int i = 0; i < loadButtonLabels.Length; i++) {
            TextMeshProUGUI label = loadButtonLabels[i];
            if (label == null) continue;
            int slot = i + 1;
            firebaseHandler.GetSlotName(slot, name => {
                label.text = string.IsNullOrEmpty(name) ? "Empty" : "Load Map " + slot;
            });
        }
    }
}