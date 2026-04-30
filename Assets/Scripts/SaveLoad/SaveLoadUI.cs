using UnityEngine;

public class SaveLoadUI : MonoBehaviour {
    [SerializeField]
    GameObject saveLoadPanel;

    public void SetVisibility(bool state) {
        saveLoadPanel.SetActive(state);
    }

    public void ToggleVisibility() {
        saveLoadPanel.SetActive(!saveLoadPanel.activeSelf);
    }
}