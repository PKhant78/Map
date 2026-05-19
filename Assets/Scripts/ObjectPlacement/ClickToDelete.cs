// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ClickToDelete : MonoBehaviour
// {
//     [SerializeField] GameObject deleteTool;
//     // Start is called before the first frame update
//     void Start()
//     {

//     }
    
//     // Update is called once per frame
//    void Update()
// {
//     if (Input.GetMouseButtonDown(0))
//     {
//         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//         RaycastHit hit;
//         if (Physics.Raycast(ray, out hit))
//         {
//             GameObject objectToDelete = hit.collider.gameObject;
//             if (deleteTool.activeSelf && objectToDelete.tag == "PlaceableObject")
//             {
//                 if (BuildingSystem.current.Selected == objectToDelete)
//                 {
//                     BuildingSystem.current.Selected = null;
//                 }
//                 Destroy(objectToDelete);
//                 BuildingSystem.current.ClearObjectToPlace();
//             }
//         }
//     }
// }
// }
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
                GameObject objectToDelete = hit.collider.gameObject;
                if (deleteTool.activeSelf && objectToDelete.CompareTag("Selectable"))
                {
                    // Clear tilemap so space isn't permanently blocked
                    PlaceableObject po = objectToDelete.GetComponent<PlaceableObject>();
                    if (po != null && po.Placed)
                    {
                        Vector3Int start = BuildingSystem.current.gridLayout.WorldToCell(po.GetStartPosition());
                        BuildingSystem.current.UnfillArea(start, po.Size);
                    }
                    // Clear state so you can add new objects after deleting
                    if (BuildingSystem.current.Selected == objectToDelete)
                        BuildingSystem.current.ClearObjectToPlace();
                    Destroy(objectToDelete);
                }
            }
        }
    }
}
