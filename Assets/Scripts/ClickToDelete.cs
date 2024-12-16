using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToDelete : MonoBehaviour
{
    [SerializeField] GameObject deleteTool;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    GameObject objectToDelete = hit.collider.gameObject;
                    if (deleteTool.activeSelf && objectToDelete.tag == "Selectable")
                        Destroy(objectToDelete);
                }
            }
        }
    }
}
