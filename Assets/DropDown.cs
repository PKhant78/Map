using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DropDownScript : MonoBehaviour
{
    [SerializeField] private GameObject hideButton;
    [SerializeField] private GameObject showIcon;
    [SerializeField] private GameObject hideIcon;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        showPos = rectTransform.anchoredPosition;
        hidePos = showPos + new Vector3(0, -rectTransform.sizeDelta.y);

        showImg = showIcon.GetComponent<Image>();
        hideImg = hideIcon.GetComponent<Image>();

        Button button = hideButton.GetComponent<Button>();
        button.onClick.AddListener(() => ShowHide());

        ShowHide();
    }

    Image showImg, hideImg;
    RectTransform rectTransform;
    Vector3 hidePos;
    Vector3 showPos;

    bool isHidden = false;
    void ShowHide()
    {
        isHidden = !isHidden;
        if (isHidden)
        {
            rectTransform.anchoredPosition = showPos;
            showIcon.SetActive(false);
            hideIcon.SetActive(true);
        }
        else
        {
            rectTransform.anchoredPosition = hidePos;
            showIcon.SetActive(true);
            hideIcon.SetActive(false);
        }
    }
}