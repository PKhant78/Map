using UnityEngine;
using UnityEngine.UI;

public class DownloadPanel : MonoBehaviour
{
    [SerializeField] FirebaseHandler firebaseHandler;
    [SerializeField] Button[] slotButtons; // assign Slot1-4 buttons

    void OnEnable()
    {
        // Grey out buttons for empty slots when panel opens
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slot = i + 1;
            slotButtons[i].interactable = firebaseHandler.SlotExists(slot);
        }
    }
}
