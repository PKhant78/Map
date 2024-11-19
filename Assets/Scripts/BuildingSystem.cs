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


    private PlaceableObject objectToPlace;

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

    private void unhighlightButtons()
    {
        GameObject content = GameObject.Find("Content");
        GameObject objectSelection = GameObject.Find("ObjectPlacement");
        GameObject objectScale = GameObject.Find("ObjectScale");
        objectSelection.transform.localPosition = new Vector3(1060, 0, 0);
        objectScale.transform.localPosition = new Vector3(-1360, -350, 0);
        foreach (Transform child in content.transform)
        {
            UnityEngine.UI.Button btn = child.GetComponent<UnityEngine.UI.Button>();
            btn.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 64f / 255f, 128f / 255f);
        }
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

    #endregion
}
