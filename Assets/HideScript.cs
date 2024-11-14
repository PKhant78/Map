using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideScript : MonoBehaviour
{
    GameObject gameObjectToHide;
    // Start is called before the first frame update
    void Start()
    {
        gameObjectToHide = GameObject.Find("ObjectSelection");
        gameObjectToHide.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void HideGameObject()
    {

    }
}
