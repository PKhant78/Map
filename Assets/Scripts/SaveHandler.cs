using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SaveHandler : MonoBehaviour
{
    Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();


    [SerializeField] BoundsInt bounds;
    [SerializeField] string filename = "tilemapData.json";

    private void Start()
    {
        
    }

    private void initObjects()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Objects");

        foreach (var obj in gameObjects)
        {
            objects.Add(obj.name, obj);
        }
    }

    public void onSave()
    {
        initObjects();
        openFile.SaveFile();

        // save objects
        List<GameObjectData> objectData = new List<GameObjectData>();

        foreach (var obj in objects)
        {
            objectData.Add(new GameObjectData(obj.Value));
        }

        FileHandler.SaveToJSON<GameObjectData>(objectData, "objectData.json");

    }

    public void onLoad()
    {

        // load objects
        openFile.LoadFile();
        string filepath = openFile.loadPath();
        if (filepath != null)
        {
            DestroyObjects();
        }

        // save objects
        List<GameObjectData> objectData = FileHandler.ReadListFromJSON<GameObjectData>("objectData.json");

        foreach (var objData in objectData)
        {
            GameObject obj = new GameObject(objData.name); 
            obj.transform.position = objData.position; 
            obj.transform.rotation = objData.rotation; 
                                                      
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
    public Vector3 position;
    public Quaternion rotation;

    public GameObjectData(GameObject gameObject)
    {
        name = gameObject.name;
        position = gameObject.transform.position;
        rotation = gameObject.transform.rotation;
    }
}
