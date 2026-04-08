using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;

public class FirebaseHandler : MonoBehaviour
{
    private DatabaseReference root;
    Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();

    void Start() {
        root = FirebaseDatabase.DefaultInstance.RootReference;
    }

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

        BuildingSystem.ObjectCount = 0;
    }
    
    private string Serialize(GameObjectData[] items, bool prettyPrint = false) {
        Environment env = new(items);
        return JsonUtility.ToJson(env, prettyPrint);
    }

    private Environment Deserialize(string json) {
        return JsonUtility.FromJson<Environment>(json);
    }

    public void SaveEnvironment(int slot)
    {
        InitObjects();
        List<GameObjectData> objectData = new List<GameObjectData>();
        foreach (var obj in objects) objectData.Add(new GameObjectData(obj.Value));
        
        // Serialize Environment data to a single JSON
        string json = Serialize(objectData.ToArray());

        // Save JSON to Database; "authorname" is a placeholder
        root.Child("Environments").Child("authorname-SLOT" + slot).SetRawJsonValueAsync(json);
    }

    private IEnumerator QueryEnvironmentByName(string name, Action<DataSnapshot> onResult) {
        var query = root
            .Child("Environments")
            .Child(name)
            .GetValueAsync();
        
        yield return new WaitUntil(() => query.IsCompleted);

        if (query.Exception == null && query.Result != null && query.Result.HasChildren) onResult?.Invoke(query.Result);
        else onResult?.Invoke(null);
    }

    // slot = 0 -> load nothing
    public void LoadEnvironment(int slot) {
        // "authorname" is a placeholder
        string envName = "authorname-SLOT" + slot;

        // Reset environment builder
        DestroyObjects();
        BuildingSystem.ObjectCount = 0;

        if (slot == 0) return;
        
        else
        StartCoroutine(QueryEnvironmentByName(envName, (DataSnapshot snapshot) => {
            if (snapshot != null) {
                // Get the data from Firebase and parse it into an Environment object
                Environment env = Deserialize(snapshot.GetRawJsonValue());
                
                // Load all PlaceableObjects
                GameObjectData[] objectData = env.Items;
                
                foreach (var data in objectData) {
                    GameObject prefab = Resources.Load<GameObject>("Prefabs/" + data.prefabName);
                    if (prefab != null) {
                        GameObject obj = Instantiate(prefab);

                        obj.GetComponent<PlaceableObject>();
                        obj.name = data.prefabName + "#" + BuildingSystem.ObjectCount++;
                        obj.transform.SetPositionAndRotation(data.position, data.rotation);
                        obj.transform.localScale = data.scale;
                        obj.tag = "Selectable";
                        obj.transform.SetParent(null);
                        obj.SetActive(true);
                    }
                    else Debug.LogError("Prefab not found: " + data.prefabName);
                }
            }
            else Debug.LogError("Environment data not found: " + envName);
        }));
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
                name = obj.name;
                prefabName = obj.prefabName;
                position = gameObject.transform.position;
                rotation = gameObject.transform.rotation;
                scale = gameObject.transform.localScale;
            }
            else
            {
                Debug.LogWarning("Has no PlaceableObject component: " + gameObject.name);
            }
        }
    }
}

[Serializable]
public class Environment {

    public string Name;
    public int ObjectCount;
    public GameObjectData[] Items;

    public Environment(GameObjectData[] items) {
        Name = "untitled";
        Items = items;
        ObjectCount = BuildingSystem.ObjectCount;
    }
}