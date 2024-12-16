using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class SaveHandler : MonoBehaviour
{
    Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();

    [SerializeField] BoundsInt bounds;

    public OpenFile openFile;

    private void Start()
    {

    }

    private void initObjects()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Selectable");

        foreach (var obj in gameObjects)
        {
            objects.Add(obj.name, obj);
        }
    }

    private void DestroyObjects()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Selectable");

        foreach (var obj in gameObjects)
        {
            Destroy(obj);
        }
    }

    public void onSave()
    {
        initObjects();
        openFile.SaveFile();

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

        // load objects
        openFile.LoadFile();
        string filepath = openFile.loadPath();
        if (filepath != null)
        {
            DestroyObjects();
        }

        // save objects
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

}
