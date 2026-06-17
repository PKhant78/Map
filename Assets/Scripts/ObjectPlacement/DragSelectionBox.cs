using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragSelectionBox : MonoBehaviour
{
    [SerializeField] private RectTransform selectionBoxUI; // UI element for the selection box
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject trashCan; // Reference to the trash can GameObject
    [SerializeField] private Color selectionColor = new Color(0, 1, 0, 0.5f); // Green for normal selection
    [SerializeField] private Color deletionColor = new Color(1, 0, 0, 0.5f); // Red for trash selection

    private Vector2 startMousePosition;
    private Vector2 endMousePosition;
    private bool isDragging;
    private bool isSelectionMode = false;

    private void Start()
    {
        selectionBoxUI.GetComponent<Image>().color = selectionColor; 
        if (selectionBoxUI != null)
        {
            selectionBoxUI.gameObject.SetActive(false); // Hide the selection box initially
        }
    }

    private void Update()
    {
        if (isSelectionMode) // Prevent dragging in selection mode
        {
            return;
        }

        if (Input.GetMouseButtonDown(0)) // Start dragging
        {
            startMousePosition = Input.mousePosition;
            isDragging = true;

            if (selectionBoxUI != null)
            {
                selectionBoxUI.gameObject.SetActive(true);

                // Change the color of the selection box based on the trash can state
                if (trashCan != null)
                {
                    if (trashCan.activeSelf)
                    {
                        selectionBoxUI.GetComponent<Image>().color = deletionColor; // Red for trash selection
                    }
                    else
                    {
                        selectionBoxUI.GetComponent<Image>().color = selectionColor; // Green for normal selection
                    }
                }
                else
                {
                    selectionBoxUI.GetComponent<Image>().color = selectionColor; // Default to green
                }
            }
        }

        if (Input.GetMouseButton(0) && isDragging) // Update the selection box
        {
            endMousePosition = Input.mousePosition;
            UpdateSelectionBox();
        }

        if (Input.GetMouseButtonUp(0)) // Finish dragging
        {
            isDragging = false;

            if (selectionBoxUI != null)
            {
                selectionBoxUI.gameObject.SetActive(false);
            }

            if (trashCan != null && trashCan.activeSelf) // Check if the trash can is active
            {
                SelectAndDeleteObjectsInBox();
            }
            else
            {
                SelectObjectsInBox(); // Normal selection logic
            }
        }
    }

    private void UpdateSelectionBox()
    {
        if (selectionBoxUI == null) return;

        Vector2 boxStart = startMousePosition;
        Vector2 boxEnd = Input.mousePosition;

        Vector2 boxCenter = (boxStart + boxEnd) / 2;
        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));

        selectionBoxUI.position = boxCenter;
        selectionBoxUI.sizeDelta = boxSize;
    }

    private void SelectObjectsInBox()
    {
        Vector2 min = mainCamera.ScreenToWorldPoint(startMousePosition);
        Vector2 max = mainCamera.ScreenToWorldPoint(endMousePosition);

        Collider[] colliders = Physics.OverlapBox(
            (min + max) / 2, 
            new Vector3(Mathf.Abs(max.x - min.x) / 2, Mathf.Abs(max.y - min.y) / 2, 1), 
            Quaternion.identity, 
            LayerMask.GetMask("PlaceableObject")
        );

        List<GameObject> selectedObjects = new List<GameObject>();
        foreach (Collider collider in colliders)
        {
            PlaceableObject placeableObject = collider.GetComponent<PlaceableObject>();
            if (placeableObject != null)
            {
                selectedObjects.Add(collider.gameObject);
            }
        }
    }

    private void SelectAndDeleteObjectsInBox()
    {
        Vector2 min = mainCamera.ScreenToWorldPoint(startMousePosition);
        Vector2 max = mainCamera.ScreenToWorldPoint(endMousePosition);

        Collider[] colliders = Physics.OverlapBox(
            (min + max) / 2, 
            new Vector3(Mathf.Abs(max.x - min.x) / 2, Mathf.Abs(max.y - min.y) / 2, 1), 
            Quaternion.identity, 
            LayerMask.GetMask("PlaceableObject")
        );

        List<GameObject> selectedObjects = new List<GameObject>();
        foreach (Collider collider in colliders)
        {
            PlaceableObject placeableObject = collider.GetComponent<PlaceableObject>();
            if (placeableObject != null)
            {
                selectedObjects.Add(collider.gameObject);
            }
        }

        DeleteSelectedObjects(selectedObjects);
    }

    private void DeleteSelectedObjects(List<GameObject> selectedObjects)
    {
        foreach (GameObject obj in selectedObjects)
        {
            Destroy(obj);
        }
    }

    // Public method to toggle selection mode
    public void SetSelectionMode(bool isEnabled)
    {
        isSelectionMode = isEnabled;
    }
}