using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowHideScript : MonoBehaviour
{
    GameObject hideBtn;
    GameObject shIcon;
    GameObject gameObjectToHide;
    // Start is called before the first frame update
    void Start()
    {
        gameObjectToHide = GameObject.Find("ObjectSelection");
        rectTransform = gameObjectToHide.GetComponent<RectTransform>();
        shIcon = GameObject.Find("Image_shIcon");

        hideBtn = GameObject.Find("Button_HideBtn");
        Button button = hideBtn.GetComponent<Button>();
        button.onClick.AddListener(() => ShowHide());
    }

    // Update is called once per frame
    void Update()
    {

    }

    bool isHidden = false;
    RectTransform rectTransform;
    Vector3 hidePos = new Vector3(0, -150, 0);
    Vector3 showPos = new Vector3(0, 0, 0);
    Image shImage;
    void ShowHide()
    {
        isHidden = !isHidden;
        if (isHidden)
        {
            rectTransform.anchoredPosition = showPos;
        }
        else
        {
            rectTransform.anchoredPosition = hidePos;
        }
    }
}