using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDrag : MonoBehaviour
{
    private Vector3 offset;
    private float yOffset;

    private void Start()
    {
        // Calculate the Y-offset dynamically based on the object's bounds
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            yOffset = renderer.bounds.extents.y + 0.8f; // Half the height of the object + 0.8f
        }
        else
        {
            yOffset = 0.8f; // Default offset if no renderer is found
        }
    }

    private void OnMouseDown()
    {
        // Calculate the offset between the object's position and the mouse position
        offset = transform.position - BuildingSystem.GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        // Get the mouse position and apply the offset
        Vector3 pos = BuildingSystem.GetMouseWorldPosition() + offset;

        // Snap the position to the grid
        pos = BuildingSystem.current.SnapCoordinationToGrid(pos);

        // Apply the dynamically calculated Y-offset
        pos.y = yOffset;

        // Update the object's position
        transform.position = pos;
    }
}
