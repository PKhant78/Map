using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDrag : MonoBehaviour
{
    private Vector3 offset;
    private float yOffset;
    private PlaceableObject placeableObject;
    private BoxCollider objectCollider;
    
    private void Start()
    {
        // Get the PlaceableObject component
        placeableObject = GetComponent<PlaceableObject>();
        
        // Get collider for size calculations
        objectCollider = GetComponent<BoxCollider>();
        
        // Calculate proper height positioning
        CalculateYOffset();
    }
    
    private void CalculateYOffset()
    {
        if (objectCollider != null)
        {
            // Use the bottom of the collider for consistent placement
            yOffset = objectCollider.bounds.extents.y;
        }
        else
        {
            // Fall back to renderer bounds if no collider
            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null)
            {
                renderer = GetComponentInChildren<Renderer>();
            }
            
            if (renderer != null)
            {
                yOffset = renderer.bounds.extents.y;
            }
            else
            {
                // Default value if no renderer found
                yOffset = 0.5f;
                Debug.LogWarning("No renderer found for " + gameObject.name + ". Using default height.");
            }
        }
    }

    private void OnMouseDown()
    {
        // Calculate offset from object position to mouse position
        Vector3 mousePos = BuildingSystem.GetMouseWorldPosition();
        offset = transform.position - mousePos;
        
        // Make sure this object is selected in the building system
        if (BuildingSystem.current != null)
        {
            BuildingSystem.current.Selected = gameObject;
        }
    }

    private void OnMouseDrag()
    {
        // Get current mouse position
        Vector3 mousePos = BuildingSystem.GetMouseWorldPosition();
        
        // Apply offset to get target position
        Vector3 targetPos = mousePos + offset;
        
        // First, snap to grid
        if (BuildingSystem.current != null)
        {
            // Get current grid cell and convert back to world position
            Vector3Int cellPos = BuildingSystem.current.gridLayout.WorldToCell(targetPos);
            Vector3 snappedPos = BuildingSystem.current.grid.GetCellCenterWorld(cellPos);
            
            // Only keep the x and z coordinates from the grid, and preserve the proper y height
            targetPos = new Vector3(snappedPos.x, targetPos.y, snappedPos.z);
        }
        
        // Ensure proper height
        // The y position should be exactly the height of the object from the ground
        targetPos.y = yOffset;
        
        // Apply the position
        transform.position = targetPos;
        
        // Force update visuals
        if (placeableObject != null)
        {
            placeableObject.ObjectColors();
        }
    }
}