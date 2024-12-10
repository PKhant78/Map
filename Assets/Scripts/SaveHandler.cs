using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SaveHandler : MonoBehaviour
{
    Dictionary<string, Tilemap> tilemaps = new Dictionary<string, Tilemap>();
    Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();


    [SerializeField] BoundsInt bounds;

    public OpenFile openFile;

    private void Start()
    {
        initTilemaps();
    }

    private void initTilemaps()
    {
        Tilemap[] maps = FindObjectsOfType<Tilemap>();

        foreach (var map in maps)
        {
            tilemaps.Add(map.name, map);
        }
    }

    private void initObjects()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Objects");

        foreach (var obj in gameObjects)
        { 
            objects.Add(obj.GetInstanceID().ToString(), obj);
        }
    }

    public void onSave()
    {
        initObjects();
        openFile.SaveFile();



        /*
        List<TilemapData> data = new List<TilemapData>();

        foreach (var mapObj in tilemaps)
        {
            TilemapData mapData = new TilemapData();
            mapData.key = mapObj.Key;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tile = mapObj.Value.GetTile(pos);

                    if (tile != null)
                    {
                        TileInfo ti = new TileInfo(tile, pos);
                        mapData.tiles.Add(ti);
                    }
                }
            }

            data.Add(mapData);
        }

        FileHandler.SaveToJSON<TilemapData>(data, filename);*/

        // save objects
        List<GameObjectData> objectData = new List<GameObjectData>();
        string filepath = openFile.loadPath();

        foreach (var obj in objects)
        {
            objectData.Add(new GameObjectData(obj.Value));
        }

        FileHandler.SaveToJSON<GameObjectData>(objectData, filepath);

    }

    public void onLoad()
    {
        //List<TilemapData> data = FileHandler.ReadListFromJSON<TilemapData>(openFile.loadPath());

        /*

        foreach(var mapData in data)
        {
            if (!tilemaps.ContainsKey(mapData.key))
            {
                //Debug
            }

            var map = tilemaps[mapData.key];

            map.ClearAllTiles();

            if (mapData.tiles != null && mapData.tiles.Count > 0)
            {
                foreach (TileInfo tile in mapData.tiles)
                {
                    map.SetTile(tile.position, tile.tile);
                }
            }
        }*/

        // load objects
        openFile.LoadFile();
        string filepath = openFile.loadPath();
        if (filepath != null)
        {
            DestroyObjects();
        }

        List<GameObjectData> objectData = FileHandler.ReadListFromJSON<GameObjectData>(filepath);


        foreach (var objData in objectData)
        {
            GameObject prefab = Resources.Load<GameObject>(objData.prefabName);
            GameObject obj = Instantiate(prefab);
            obj.GetComponent<PlaceableObject>();
            obj.AddComponent<ObjectDrag>();
            obj.transform.position = objData.position; 
            obj.transform.rotation = objData.rotation;
            obj.transform.localScale = objData.scale;
            
            obj.transform.SetParent(null);
            obj.SetActive(true);

        }
    }

    public void DestroyObjects()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Objects");

        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
    }

}

[Serializable]
public class TilemapData
{
    public string key;
    public List<TileInfo> tiles = new List<TileInfo>();
}

[Serializable]
public class TileInfo
{
    public TileBase tile;
    public Vector3Int position;

    public TileInfo(TileBase tile, Vector3Int pos)
    {
        this.tile = tile;
        position = pos;
    }
}

[Serializable]
public class GameObjectData
{
    public string name;
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public GameObjectData(GameObject gameObject)
    {
        if (gameObject != null)
        {
            PlaceableObject obj = gameObject.GetComponent<PlaceableObject>();
            if (obj != null)
            {
                name = gameObject.name;
                prefabName = obj.prefabName;
                position = gameObject.transform.position;
                rotation = gameObject.transform.rotation;
                scale = gameObject.transform.localScale;
            }
            else
            {
                Debug.LogWarning("PlaceableObject component not found on " + gameObject.name);
            }
        }
    }
}
