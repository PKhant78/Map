using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadedMapBanner : MonoBehaviour
{
    [SerializeField] FirebaseHandler firebaseHandler;
    [SerializeField] TextMeshProUGUI bannerText;
    [SerializeField] string emptyLabel = "Untitled Map";
    [SerializeField] string prefix = "Loaded: ";
    [SerializeField] float refreshInterval = 0.25f;

    [Header("Rename In-Scene")]
    [SerializeField] GameObject editButton;
    [SerializeField] GameObject renameRow;
    [SerializeField] TMP_InputField renameInput;
    [SerializeField] Button confirmButton;

    float timer;
    string lastShown = null;

    void Start()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmRename);
        SetEditMode(false);
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer < refreshInterval) return;
        timer = 0f;

        string name = firebaseHandler.CurrentMapName;
        string display = string.IsNullOrEmpty(name) ? emptyLabel : prefix + name;
        if (display == lastShown) return;

        bannerText.text = display;
        lastShown = display;
    }

    public void BeginRename()
    {
        if (firebaseHandler.CurrentSlot == 0)
        {
            bannerText.text = "Load a map first";
            lastShown = null;
            return;
        }
        renameInput.text = firebaseHandler.CurrentMapName;
        SetEditMode(true);
        renameInput.Select();
        renameInput.ActivateInputField();
    }

    public void ConfirmRename()
    {
        string newName = renameInput.text.Trim();
        if (!string.IsNullOrEmpty(newName))
            firebaseHandler.RenameCurrentMap(newName);
        SetEditMode(false);
        lastShown = null;
    }

    public void CancelRename()
    {
        SetEditMode(false);
    }

    private void SetEditMode(bool editing)
    {
        if (bannerText != null) bannerText.gameObject.SetActive(!editing);
        if (editButton != null) editButton.SetActive(!editing);
        if (renameRow != null) renameRow.SetActive(editing);
    }
}
