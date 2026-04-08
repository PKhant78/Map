using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SearchService;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem current;
    public GridLayout gridLayout;
    public Grid grid;
    [SerializeField] private Tilemap MainTilemap;
    [SerializeField] private TileBase whiteTile;

    // Prefabs & Objects
    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject Selected;

    // Input Settings
    private float doubleClickTime = 0.3f;
    private float lastClickTime = 0f;

    // Scaling Objects
    public enum size { small, medium, large }
    public UnityEngine.UI.Slider scaleSlider;
    size currentSize = size.small;

    private PlaceableObject objectToPlace;
    public static int ObjectCount = 0;

    #region Unity Methods

    // UI References
    [SerializeField] public GameObject content;
    [SerializeField] private GameObject objectPlacement;
    [SerializeField] private GameObject objectScale;
    [SerializeField] private GameObject saveLoad;
    [SerializeField] private GameObject homeBtn;
    [SerializeField] private GameObject trashBtn;
    Color defaultColor;

    // PlacementSystem Fields
    [SerializeField] private GameObject mouseIndicator;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GameObject gridVisualization;
    [SerializeField] private GameObject SelectionBox;

    private void Awake()
    {
        current = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();
    }

    private void OnDestroy()
    {
        if (inputManager != null)
            inputManager.OnExit -= Deselect;
    }

    private void Start()
    {
        Transform transformBtn = content.transform.GetChild(0);
        UnityEngine.UI.Button btn = transformBtn.GetComponent<UnityEngine.UI.Button>();
        defaultColor = btn.GetComponent<UnityEngine.UI.Image>().color;

        if (scaleSlider != null)
            scaleSlider.onValueChanged.AddListener(UpdateScale);

        if (inputManager != null)
            inputManager.OnExit += Deselect;

        unhighlightButtons();

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !inputManager.IsPointerOverUI())
        {
            if (IsDoubleClick())
            {
                if (objectToPlace != null && CanBePlaced(objectToPlace))
                {
                    objectToPlace.Place();
                    Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                    TakeArea(start, objectToPlace.Size);
                    objectToPlace = null;
                    Selected = null;
                    lastClickTime = 0f;
                    unhighlightButtons();
                }
            }
            else
            {
                SelectObject();
            }
        }

        if (scaleSlider != null && objectToPlace != null)
        {
            scaleSlider.value = objectToPlace.transform.localScale.x;
        }

        if (!objectToPlace)
        {
            return;
        }

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        if (mouseIndicator != null)
        {
            mouseIndicator.transform.position = grid.GetCellCenterWorld(gridPosition);
        }
    }

    #region Utils

    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            return raycastHit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 SnapCoordinationToGrid(Vector3 position)
    {
        Vector3Int cellPos = gridLayout.WorldToCell(position);
        position = grid.GetCellCenterWorld(cellPos);
        return position;
    }

    private void UpdateScale(float newScale)
    {
        Vector3 currentScale = objectToPlace.transform.localScale;
        objectToPlace.transform.localScale = new Vector3(newScale, currentScale.y, currentScale.z);
    }

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, z: 0);

            array[counter] = tilemap.GetTile(pos);
            counter++;
        }

        return array;
    }

    private bool IsDoubleClick()
    {
        bool isDouble = Time.time - lastClickTime < doubleClickTime;
        lastClickTime = Time.time;
        return isDouble;
    }

    #endregion // Utils

    #region Building Placement

    public void RotateSelected()
    {
        if (Selected) objectToPlace.Rotate();
    }

    public void DestroySelected()
    {
        if (Selected)
        {
            Destroy(Selected);
            Selected = null;
            objectToPlace = null;
            unhighlightButtons();
        }
    }

    public void PlaceSelected()
    {
        if (Selected)
        {
            if (CanBePlaced(objectToPlace))
            {
                objectToPlace.Place();
                Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                TakeArea(start, objectToPlace.Size);
                Selected = null;
                objectToPlace = null;
                unhighlightButtons();
            }
        }
    }

    private void unhighlightButtons()
    {
        objectPlacement.SetActive(false);
        objectScale.SetActive(false);
        saveLoad.SetActive(true);
        homeBtn.SetActive(true);
        trashBtn.SetActive(true);
        foreach (Transform child in content.transform)
        {
            UnityEngine.UI.Button btn = child.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
                btn.GetComponent<UnityEngine.UI.Image>().color = defaultColor;
        }
    }

    private void highlightButtons()
    {
        objectPlacement.SetActive(true);
        objectScale.SetActive(true);
        saveLoad.SetActive(false);
        homeBtn.SetActive(false);
        trashBtn.SetActive(false);
        foreach (Transform child in content.transform)
        {
            UnityEngine.UI.Button btn = child.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
                btn.GetComponent<UnityEngine.UI.Image>().color = defaultColor;
        }
    }

    public void InitializeWithObject(GameObject prefab)
    {
        Vector3 position = SnapCoordinationToGrid(Vector3.zero);

        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        obj.name = prefab.name + " #" + ObjectCount++;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            position.y += renderer.bounds.extents.y;
        }
        obj.transform.position = position;

        objectToPlace = obj.GetComponent<PlaceableObject>();
        obj.AddComponent<ObjectDrag>();

        Selected = obj;

        if (objectToPlace != null)
            objectToPlace.SetColor(new Color(0, 1, 0, 0.5f)); 

        highlightButtons();
    }

    private bool CanBePlaced(PlaceableObject placeableObject)
    {
        BoundsInt area = new BoundsInt();
        area.position = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
        area.size = placeableObject.Size;

        TileBase[] baseArray = GetTilesBlock(area, MainTilemap);

        foreach (var b in baseArray)
        {
            if (b == whiteTile)
            {
                return false;
            }
        }
        return true;
    }

    private void SelectObject()
    {
        if (ClickToDelete.IsDeleteModeActive) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider != null && hit.collider.gameObject.CompareTag("Selectable"))
        {
            Selected = hit.collider.gameObject;
            objectToPlace = Selected.GetComponent<PlaceableObject>();
            // Reset Placed so double-click doesn't re-trigger Place() immediately,
            // and so the object can be freely moved again before re-confirming placement.
            objectToPlace.UpdateState(false, true);
            // Avoid adding duplicate ObjectDrag components on repeated clicks.
            if (Selected.GetComponent<ObjectDrag>() == null)
            {
                Selected.AddComponent<ObjectDrag>();
            }
            Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
            UnfillArea(start, objectToPlace.Size);
            highlightButtons();
        }
    }

    public void Deselect()
    {
        Selected = null;
        objectToPlace = null;
        unhighlightButtons();
    }

    public void TakeArea(Vector3Int start, Vector3Int size)
    {
        MainTilemap.BoxFill(start, whiteTile, startX: start.x, startY: start.y, endX: start.x + size.x, endY: start.y + size.y);
    }

    public void UnfillArea(Vector3Int start, Vector3Int size)
    {
        MainTilemap.BoxFill(start, null, startX: start.x, startY: start.y, endX: start.x + size.x, endY: start.y + size.y);
    }

    public void changeSize(size s)
    {
        GameObject obj = Selected;
        switch (s)
        {
            case size.small:
                obj.transform.localScale = new Vector3(1f, 1f, 1f);
                break;
            case size.medium:
                obj.transform.localScale = new Vector3(3f, 3f, 3f);
                break;
            case size.large:
                obj.transform.localScale = new Vector3(5f, 5f, 5f);
                break;
            default:
                break;
        }
    }

   
}
#endregion
#endregion 