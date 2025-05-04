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
    private Grid grid;
    [SerializeField] private Tilemap MainTilemap;
    [SerializeField] private TileBase whiteTile;

    // Prefabs & Objects
    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject Selected; // Line added by Bryan

    // Input Settings
    private float doubleClickTime = 0.3f;
    private float lastClickTime = 0f;


    // Scaling Objects 
    public enum size { small, medium, large}
    public UnityEngine.UI.Slider scaleSlider;
    size currentSize = size.small;


    private PlaceableObject objectToPlace;

    // UI References
    [SerializeField] public GameObject content;
    [SerializeField] private GameObject objectPlacement;
    [SerializeField] private GameObject objectScale;
    [SerializeField] private GameObject saveLoad;
    [SerializeField] private GameObject homeBtn;
    [SerializeField] private GameObject trashBtn;
    Color defaultColor;


    #region Unity Methods
    private void Awake()
    {
        current = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();
    }

    private void Update()
    {
        // Checks if left mouse clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Checks for double-click to place object
            if (IsDoubleClick())
            {
                if(objectToPlace != null)
                {   
                    // Checks if valid placement
                    if(CanBePlaced(objectToPlace))
                    {
                        objectToPlace.Place();
                        Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                        TakeArea(start, objectToPlace.Size);

                    }
                }
            }
            else
            {
                // Single click selects the object
                SelectObject();
            }
        }


        if (scaleSlider != null && objectToPlace != null)
        {
            scaleSlider.value = objectToPlace.transform.localScale.x;
            scaleSlider.onValueChanged.AddListener(UpdateScale);
        }

        if (!objectToPlace)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            currentSize = (size)(((int)currentSize + 1) % Enum.GetValues(typeof(size)).Length);
            changeSize(currentSize);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (CanBePlaced(objectToPlace))
            {
                objectToPlace.Place();
                Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                TakeArea(start, objectToPlace.Size);
            }
            else
            {
                Destroy(objectToPlace.gameObject);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(objectToPlace.gameObject);
        }
    }
        #endregion


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

    public Vector3 SnapCoordinationToGrid (Vector3 position)
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

        foreach (var v in area.allPositionsWithin) {
            Vector3Int pos = new Vector3Int(v.x, v.y, z:0);

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

    #endregion

    #region Building Placement
    // Lines added by Bryan
    public void RotateSelected()
    {
        if (Selected) objectToPlace.Rotate();
    }

    public void DestroySelected()
    {
        if (Selected)
        {
            Destroy(objectToPlace.gameObject);
            Selected = null;
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
                Selected = null;
                unhighlightButtons();
                Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                TakeArea(start, objectToPlace.Size);
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

    private void Start()
    {
        Transform transformBtn = content.transform.GetChild(0);
        UnityEngine.UI.Button btn = transformBtn.GetComponent<UnityEngine.UI.Button>();
        defaultColor = btn.GetComponent<UnityEngine.UI.Image>().color;

        unhighlightButtons();
    }


    public void InitializeWithObject(GameObject prefab)
    {
        Vector3 position = SnapCoordinationToGrid(Vector3.zero);

        GameObject obj = Instantiate(prefab, position, Quaternion.identity);

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            position.y += renderer.bounds.extents.y + 0.8f;
        }
        else
        {
            position.y += 0.8f; 
        }

        obj.transform.position = position;

        objectToPlace = obj.GetComponent<PlaceableObject>();
        obj.AddComponent<ObjectDrag>();

        Selected = obj;

        unhighlightButtons();
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider != null && hit.collider.gameObject.CompareTag("Selectable"))
        {
            Selected = hit.collider.gameObject;
            objectToPlace = Selected.GetComponent<PlaceableObject>();
            Debug.Log(Selected);
            Selected.AddComponent<ObjectDrag>();
            Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
            UnfillArea(start, objectToPlace.Size);
            highlightButtons();
        }


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

    #endregion
}
