using UnityEngine;
using System;
using System.Collections.Generic;

public class ScreenOverlayController : MonoBehaviour
{
    private bool isOverlayActive = false;
    private List<GameObject> camLst = new List<GameObject>();

    void Start()
    {
        InitCameras();
        SetOverlay(false);

    }

    void InitCameras()
    {
        camLst.Add(GameObject.Find("Main Camera"));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            isOverlayActive = !isOverlayActive;
            SetOverlay(isOverlayActive);
        }
    }

    public void SetOverlay(bool active)
    {
        foreach(GameObject obj in camLst)
        {
            Camera cam = obj.transform.GetComponent<Camera>();
            int layer = active ? 0 : -1;
            cam.cullingMask = layer;
            Debug.LogError("============ layer  " + layer + "   " + cam.cullingMask);
        }
    }
}

