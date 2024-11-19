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
        showImg = showIcon.GetComponent<Image>();
        hideImg = hideIcon.GetComponent<Image>();

        Button button = hideButton.GetComponent<Button>();
        button.onClick.AddListener(() => ShowHide());
        ShowHide();
    }

    // Update is called once per frame
    void Update()
    {

    }

    Image showImg, hideImg;
    RectTransform rectTransform;
    Vector3 hidePos = new Vector3(0, -150, 0);
    Vector3 showPos = new Vector3(0, 0, 0);

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