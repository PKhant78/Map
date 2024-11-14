using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem current;

    public GridLayout gridLayout;
    private Grid grid;
    [SerializeField] private Tilemap MainTilemap;
    [SerializeField] private TileBase whiteTile;

    public GameObject prefab1;
    public GameObject prefab2;
    private GameObject selectedObject;
    private PlaceableObject objectToPlace;

    private float doubleClickTime = 0.3f;  
    private float lastClickTime = 0f;

    #region Unity Methods

    private void Awake()
    {
         {
            current = this;
            grid = gridLayout.gameObject.GetComponent<Grid>();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastClickTime < doubleClickTime)
            {
                SelectObject();
            }
            lastClickTime = Time.time;
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
        else if (Input.GetKeyDown(KeyCode.Backspace))
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

    public void InitializeWithObject(GameObject prefab)
    {
        Vector3 position = SnapCoordinationToGrid(Vector3.zero);
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        objectToPlace = obj.GetComponent<PlaceableObject>();
        obj.AddComponent<ObjectDrag>();
    }

    private void SelectObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider != null && hit.collider.gameObject.CompareTag("Selectable"))
        {
            selectedObject = hit.collider.gameObject;
            objectToPlace = selectedObject.GetComponent<PlaceableObject>();
            Debug.Log(selectedObject);
            selectedObject.AddComponent<ObjectDrag>();
            Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
            UnfillArea(start, objectToPlace.Size);
        }

        
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

    public void rotateObject()
    {
        objectToPlace.Rotate();
    }

    public void placeObject()
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

    public void deleteObject()
    {
        Destroy(objectToPlace.gameObject);
    }

    public void TakeArea(Vector3Int start, Vector3Int size)
    {
        MainTilemap.BoxFill(start, whiteTile, startX: start.x, startY: start.y, endX: start.x + size.x, endY: start.y + size.y);
    }

    public void UnfillArea(Vector3Int start, Vector3Int size)
    {
        MainTilemap.BoxFill(start, null, startX: start.x, startY: start.y, endX: start.x + size.x, endY: start.y + size.y);
    }

    #endregion

}
