using System.Collections;
using TMPro;
using UnityEngine;

public class SaveLoadUI : MonoBehaviour {
    [SerializeField] GameObject saveLoadPanel;
    [SerializeField] FirebaseHandler firebaseHandler;
    [SerializeField] TextMeshProUGUI[] loadButtonLabels;
    [SerializeField] TextMeshProUGUI feedbackText;
    [SerializeField] TMP_InputField mapNameInput;

    [Header("Confirm Delete")]
    [SerializeField] GameObject confirmDeletePanel;
    [SerializeField] TextMeshProUGUI confirmDeleteText;

    [Header("Rename")]
    [SerializeField] GameObject renamePanel;
    [SerializeField] TMP_InputField renameInput;

    private int selectedSlot = 0;
    private int lastUsedSlot = 0;
    private int pendingDeleteSlot = 0;
    private int pendingRenameSlot = 0;

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
        string mapName = mapNameInput != null ? mapNameInput.text?.Trim() : "";
        if (string.IsNullOrEmpty(mapName)) mapName = "Slot " + slot;

        firebaseHandler.SaveEnvironment(slot, mapName);
        lastUsedSlot = slot;

        if (mapNameInput != null) mapNameInput.text = "";
        RefreshSlotNames();
    }

    public void SelectSlot(int slot) => selectedSlot = slot;

    public void Load(int slot) {
        firebaseHandler.LoadEnvironment(slot);
        lastUsedSlot = slot;
    }

    public void DownloadSelected()
    {
        if (selectedSlot == 0)
        {
            if (feedbackText != null)
                StartCoroutine(ShowFeedback("No slot selected"));
            return;
        }
        Download(selectedSlot);
    }

    public void DownloadLastUsed()
    {
        if (lastUsedSlot == 0)
        {
            if (feedbackText != null)
                StartCoroutine(ShowFeedback("No map saved or loaded yet"));
            return;
        }
        Download(lastUsedSlot);
    }

    public void DeleteSelected()
    {
        if (selectedSlot == 0)
        {
            if (feedbackText != null)
                StartCoroutine(ShowFeedback("No slot selected"));
            return;
        }
        Delete(selectedSlot);
    }

    public void ClearAll()
    {
        Debug.Log("[ClearAll] Called");
        firebaseHandler.DestroyObjects();
        Debug.Log("[ClearAll] DestroyObjects done");
        if (feedbackText != null)
            StartCoroutine(ShowFeedback("Scene cleared"));
    }

    public void Delete(int slot)
    {
        if (!firebaseHandler.SlotExists(slot))
        {
            if (feedbackText != null) StartCoroutine(ShowFeedback("Slot " + slot + " is already empty"));
            return;
        }
        pendingDeleteSlot = slot;
        if (confirmDeletePanel != null)
        {
            if (confirmDeleteText != null)
                confirmDeleteText.text = "Delete Slot " + slot + "? This cannot be undone.";
            confirmDeletePanel.SetActive(true);
        }
        else
        {
            ConfirmDelete();
        }
    }

    public void ConfirmDelete()
    {
        confirmDeletePanel?.SetActive(false);
        bool deleted = firebaseHandler.DeleteEnvironment(pendingDeleteSlot);
        if (deleted) firebaseHandler.DestroyObjects();
        RefreshSlotNames();
        if (feedbackText != null)
            StartCoroutine(ShowFeedback("Slot " + pendingDeleteSlot + " deleted"));
        pendingDeleteSlot = 0;
    }

    public void CancelDelete()
    {
        confirmDeletePanel?.SetActive(false);
        pendingDeleteSlot = 0;
    }

    public void BeginRename(int slot)
    {
        if (!firebaseHandler.SlotExists(slot))
        {
            if (feedbackText != null) StartCoroutine(ShowFeedback("Slot " + slot + " is empty"));
            return;
        }
        pendingRenameSlot = slot;
        if (renamePanel != null)
        {
            if (renameInput != null)
                renameInput.text = "";
            renamePanel.SetActive(true);
        }
    }

    public void ConfirmRename()
    {
        renamePanel?.SetActive(false);
        string newName = renameInput != null ? renameInput.text.Trim() : "";
        bool renamed = firebaseHandler.RenameEnvironment(pendingRenameSlot, newName);
        RefreshSlotNames();
        if (feedbackText != null)
            StartCoroutine(ShowFeedback(renamed ? "Renamed to \"" + newName + "\"" : "Slot " + pendingRenameSlot + " is empty"));
        pendingRenameSlot = 0;
    }

    public void CancelRename()
    {
        renamePanel?.SetActive(false);
        pendingRenameSlot = 0;
    }

    public void Download(int slot)
    {
        string msg = firebaseHandler.DownloadMap(slot);
        if (feedbackText != null)
            StartCoroutine(ShowFeedback(msg));
    }

    private IEnumerator ShowFeedback(string msg, float duration = 2f)
    {
        feedbackText.text = msg;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        feedbackText.gameObject.SetActive(false);
    }


    private void RefreshSlotNames() {
        if (loadButtonLabels == null) return;
        for (int i = 0; i < loadButtonLabels.Length; i++) {
            TextMeshProUGUI label = loadButtonLabels[i];
            if (label == null) continue;
            int slot = i + 1;
            firebaseHandler.GetSlotName(slot, name => {
                label.text = string.IsNullOrEmpty(name) ? "Empty" : name;
            });
        }
    }
}
