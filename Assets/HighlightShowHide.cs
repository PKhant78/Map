using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighlightShowHide : MonoBehaviour
{
    [SerializeField] private GameObject objectPlacement;
    [SerializeField] private GameObject objectScale;
    RectTransform objectScaleRect;
    Vector3 objectScalePos;
    [SerializeField] private GameObject saveLoad;
    [SerializeField] private GameObject homeBtn;
    Color selectedColor = new Color(1, 32f / 255f, 152f / 255f);

    void highlightBtn(Button button)
    {
        button.GetComponent<Image>().color = selectedColor;
        objectPlacement.SetActive(true);
        objectScaleRect.anchoredPosition = objectScalePos;
        objectScale.SetActive(true);
        saveLoad.SetActive(false);
        homeBtn.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        objectScaleRect = objectScale.GetComponent<RectTransform>();
        objectScalePos = objectScaleRect.anchoredPosition;
        foreach (Transform child in transform)
        {
            Button btn = child.GetComponent<Button>();
            btn.onClick.AddListener(() => highlightBtn(btn));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
