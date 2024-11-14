using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDrag : MonoBehaviour
{
    private Vector3 offset;

    private void OnMouseDown()
    {
        offset = transform.position - BuildingSystem.GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        Vector3 pos = BuildingSystem.GetMouseWorldPosition() + offset;
        transform.position = BuildingSystem.current.SnapCoordinationToGrid(pos);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))  
        {
            Vector3 pos = BuildingSystem.GetMouseWorldPosition();
            transform.position = BuildingSystem.current.SnapCoordinationToGrid(pos);
        }
    }
}
