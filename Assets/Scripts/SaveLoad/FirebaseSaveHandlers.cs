using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseSaveHandler : MonoBehaviour
{
    Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
    
    [SerializeField] BoundsInt bounds;
    
    public FirebaseFileSelector fileSelector;
    
    private void InitObjects()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Selectable");
        
        objects.Clear();
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
    
    public async void OnSave()
    {
        InitObjects();
        
        if (fileSelector != null)
        {
            // Prompt for save location
            fileSelector.OnNewFile();
            
            string path = fileSelector.GetCurrentPath();
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("No save path specified");
                return;
            }
            
            // Gather object data
            List<GameObjectData> objectData = new List<GameObjectData>();
            
            foreach (var obj in objects)
            {
                objectData.Add(new GameObjectData(obj.Value));
            }
            
            // Save to Firebase
            await FirebaseHandler.SaveToDatabase(objectData, "saves/" + path);
            Debug.Log("Saved to Firebase: saves/" + path);
        }
        else
        {
            Debug.LogError("FileSelector reference is not set");
        }
    }
    
    public async void OnLoad()
    {
        if (fileSelector != null)
        {
            // Refresh file list and prompt for file selection
            await fileSelector.RefreshFileList();
            fileSelector.OnSelectFile();
            
            string path = fileSelector.GetCurrentPath();
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("No file selected for loading");
                return;
            }
            
            // Clean up existing objects
            DestroyObjects();
            
            // Load from Firebase
            List<GameObjectData> objectData = await FirebaseHandler.ReadListFromDatabase<GameObjectData>("saves/" + path);
            
            foreach (var objData in objectData)
            {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/" + objData.prefabName);
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab);
                    
                    obj.GetComponent<PlaceableObject>();
                    obj.name = objData.name;
                    obj.transform.SetPositionAndRotation(objData.position, objData.rotation);
                    obj.transform.localScale = objData.scale;
                    obj.tag = "Selectable";
                    obj.transform.SetParent(null);
                    obj.SetActive(true);
                }
                else
                {
                    Debug.LogError("Prefab not found: " + objData.prefabName);
                }
            }
            
            Debug.Log("Loaded from Firebase: saves/" + path);
        }
        else
        {
            Debug.LogError("FileSelector reference is not set");
        }
    }
}

// Keep the GameObjectData class as it was in the original SaveHandler.cs
[Serializable]
public class GameObjectData
{
    public int ID;
    public string name;
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    
    public GameObjectData() { } // Empty constructor needed for Firebase deserialization
    
    public GameObjectData(GameObject gameObject)
    {
        if (gameObject != null)
        {
            PlaceableObject obj = gameObject.GetComponent<PlaceableObject>();
            if (obj != null)
            {
                name = gameObject.name;
                ID = int.Parse(name[(name.IndexOf('#') + 1)..]);
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