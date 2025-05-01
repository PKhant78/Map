using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDrag : MonoBehaviour
{
    private Vector3 offset;

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

        // Apply the Y-offset to keep the object above the ground
        pos.y += 0.8f;

        // Update the object's position
        transform.position = pos;
    }
}
