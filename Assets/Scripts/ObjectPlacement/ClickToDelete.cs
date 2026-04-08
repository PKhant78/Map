using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToDelete : MonoBehaviour
{
    [SerializeField] GameObject deleteTool;

    public static bool IsDeleteModeActive { get; private set; }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        IsDeleteModeActive = deleteTool != null && deleteTool.activeSelf;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject objectToDelete = hit.collider.gameObject;
                if (deleteTool.activeSelf && objectToDelete.tag == "Selectable")
                    Destroy(objectToDelete);
            }
        }
    }
}
