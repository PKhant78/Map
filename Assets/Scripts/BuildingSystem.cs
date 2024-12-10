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

    public GameObject prefab1;
    public GameObject prefab2;

    public GameObject Selected; // Line added by Bryan

    public enum size { small, medium, large}

    private PlaceableObject objectToPlace;
    public UnityEngine.UI.Slider scaleSlider;

    size currentSize = size.small;

    #region Unity Methods

    private void Awake()
    {
        current = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();
    }

    private void Start()
    {

    }

    private void UpdateScale(float newScale)
    {
        Vector3 currentScale = objectToPlace.transform.localScale;
        objectToPlace.transform.localScale = new Vector3(newScale, currentScale.y, currentScale.z);
    }

    private void Update()
    {
        if (scaleSlider != null && objectToPlace != null)
        {
            scaleSlider.value = objectToPlace.transform.localScale.x;
            scaleSlider.onValueChanged.AddListener(UpdateScale);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            InitializeWithObject(prefab1);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            InitializeWithObject(prefab2);
        }

        if (!objectToPlace)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            objectToPlace.Rotate();
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

    [SerializeField] public GameObject content;
    [SerializeField] private GameObject objectPlacement;
    [SerializeField] private GameObject objectScale;
    [SerializeField] private GameObject saveLoad;
    [SerializeField] private GameObject homeBtn;
    Color defaultColor;

    private void unhighlightButtons()
    {
        objectPlacement.SetActive(false);
        objectScale.SetActive(false);
        saveLoad.SetActive(true);
        homeBtn.SetActive(true);
        foreach (Transform child in content.transform)
        {
            UnityEngine.UI.Button btn = child.GetComponent<UnityEngine.UI.Button>();
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
        objectToPlace = obj.GetComponent<PlaceableObject>();
        obj.AddComponent<ObjectDrag>();

        // Lines added by Bryan
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

    public void TakeArea(Vector3Int start, Vector3Int size)
    {
        MainTilemap.BoxFill(start, whiteTile, startX: start.x, startY: start.y, endX: start.x + size.x, endY: start.y + size.y);
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
