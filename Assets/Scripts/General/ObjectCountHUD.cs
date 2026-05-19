using TMPro;
using UnityEngine;

public class ObjectCountHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI countText;
    [SerializeField] float refreshInterval = 0.25f;

    float timer;

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer < refreshInterval) return;
        timer = 0f;

        int count = FindObjectsOfType<PlaceableObject>().Length - 1; // Subtract 1 to exclude the object being placed
        countText.text = "Objects: " + count;
    }
}
