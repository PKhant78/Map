using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDrag : MonoBehaviour
{
    private Vector3 offset;
    private float yOffset;

    private void Start()
    {
        // Calculate the Y-offset dynamically
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            yOffset = renderer.bounds.extents.y + 0.8f; 
        }
        else
        {
            yOffset = 0.8f; 
        }
    }

    private void OnMouseDown()
    {
        offset = transform.position - BuildingSystem.GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        Vector3 pos = BuildingSystem.GetMouseWorldPosition() + offset;

        pos = BuildingSystem.current.SnapCoordinationToGrid(pos);

        pos.y = yOffset;

        transform.position = pos;
    }
}
