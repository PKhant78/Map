using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSelect : MonoBehaviour
{
    private GameObject selectedObject;
    private PlaceableObject objPlace;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectObject();
        }
    }

    private void selectObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Selectable"))
            {
                selectedObject = hit.collider.gameObject;
                
                objPlace = selectedObject.GetComponent<PlaceableObject>();
                Debug.Log(selectedObject);
            }
        }
    }
}
