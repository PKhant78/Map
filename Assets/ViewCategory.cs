using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ViewCategory : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] BuildingSystem grid;
    [SerializeField] GameObject scrollView;
    ScrollRect scrollRect;
    [SerializeField] GameObject viewPort;
    [SerializeField] GameObject targetContent;
    RectTransform targetRect;
    void Start()
    {
        scrollRect = scrollView.GetComponent<ScrollRect>();
        targetRect = targetContent.GetComponent<RectTransform>();
        GetComponent<Button>().onClick.AddListener(focusTarget);
    }

    public void focusTarget()
    {
        grid.content = targetContent;
        scrollRect.content = targetRect;
        for (int i = 0; i < viewPort.transform.childCount; i++)
            viewPort.transform.GetChild(i).gameObject.SetActive(false);
        targetContent.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
