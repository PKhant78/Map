using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighlightShowHide : MonoBehaviour
{
    void highlightBtn(Button button)
    {
        button.GetComponent<Image>().color = new Color(1, 32f / 255f, 152f / 255f);
        objectPlacement.transform.localPosition = new Vector3(960, 0, 0);
        objectScale.transform.localPosition = new Vector3(-900, -350, 0);
        saveLoad.transform.localPosition = new Vector3(960, -730, 0);
    }
    [SerializeField] private GameObject saveLoad;
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject objectPlacement;
    [SerializeField] private GameObject objectScale;
    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform child in content.transform)
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
