// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Firebase.Database;
// using UnityEngine;

// public class FirebaseHandler : MonoBehaviour
// {
//     private DatabaseReference root;
//     Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();

//     void Start() {
//         root = FirebaseDatabase.DefaultInstance.RootReference;
//     }

//     private void InitObjects()
//     {
//         objects.Clear();
//         GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("PlaceableObject");
        
//         foreach (var obj in gameObjects)
//         {
//             objects.Add(obj.name, obj);
//         }
//     }
    
//     private void DestroyObjects()
//     {
//         GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("PlaceableObject");
        
//         foreach (var obj in gameObjects)
//         {
//             Destroy(obj);
//         }

//         BuildingSystem.ObjectCount = 0;
//     }
    
//     private string Serialize(GameObjectData[] items, bool prettyPrint = false) {
//         Environment env = new(items);
//         return JsonUtility.ToJson(env, prettyPrint);
//     }

//     private Environment Deserialize(string json) {
//         return JsonUtility.FromJson<Environment>(json);
//     }

//     public void SaveEnvironment(int slot)
//     {
//         InitObjects();
//         List<GameObjectData> objectData = new List<GameObjectData>();
//         foreach (var obj in objects) objectData.Add(new GameObjectData(obj.Value));
        
//         // Serialize Environment data to a single JSON
//         string json = Serialize(objectData.ToArray());

//         // Save JSON to Database; "authorname" is a placeholder
//         root.Child("Environments").Child("authorname-SLOT" + slot).SetRawJsonValueAsync(json);
//     }

    

//     private IEnumerator QueryEnvironmentByName(string name, Action<DataSnapshot> onResult) {
//         var query = root
//             .Child("Environments")
//             .Child(name)
//             .GetValueAsync();
        
//         yield return new WaitUntil(() => query.IsCompleted);

//         if (query.Exception == null && query.Result != null && query.Result.HasChildren) onResult?.Invoke(query.Result);
//         else onResult?.Invoke(null);
//     }

//     // slot = 0 -> load nothing
//     public void LoadEnvironment(int slot) {
//         // "authorname" is a placeholder
//         string envName = "authorname-SLOT" + slot;

//         // Reset environment builder
//         DestroyObjects();
//         BuildingSystem.ObjectCount = 0;

//         if (slot == 0) return;
        
//         else
//         StartCoroutine(QueryEnvironmentByName(envName, (DataSnapshot snapshot) => {
//             if (snapshot != null) {
//                 // Get the data from Firebase and parse it into an Environment object
//                 Environment env = Deserialize(snapshot.GetRawJsonValue());
                
//                 // Load all PlaceableObjects
//                 GameObjectData[] objectData = env.Items;
                
//                 foreach (var data in objectData) {
//                     GameObject prefab = Resources.Load<GameObject>("Prefabs/" + data.prefabName);
//                     if (prefab != null) {
//                         GameObject obj = Instantiate(prefab);

//                         obj.GetComponent<PlaceableObject>();
//                         obj.name = data.prefabName + "#" + BuildingSystem.ObjectCount++;
//                         obj.transform.SetPositionAndRotation(data.position, data.rotation);
//                         obj.transform.localScale = data.scale;
//                         obj.tag = "Selectable";
//                         obj.transform.SetParent(null);
//                         obj.SetActive(true);
//                     }
//                     else Debug.LogError("Prefab not found: " + data.prefabName);
//                 }
//             }
//             else Debug.LogError("Environment data not found: " + envName);
//         }));
//     }
// }

// [Serializable]
// public class GameObjectData
// {
//     public string name;
//     public string prefabName;
//     public Vector3 position;
//     public Quaternion rotation;
//     public Vector3 scale;
    
//     public GameObjectData(GameObject gameObject)
//     {
//         if (gameObject != null)
//         {
//             PlaceableObject obj = gameObject.GetComponent<PlaceableObject>();
//             if (obj != null)
//             {
//                 name = obj.name;
//                 prefabName = obj.prefabName;
//                 position = gameObject.transform.position;
//                 rotation = gameObject.transform.rotation;
//                 scale = gameObject.transform.localScale;
//             }
//             else
//             {
//                 Debug.LogWarning("Has no PlaceableObject component: " + gameObject.name);
//             }
//         }
//     }
// }

// [Serializable]
// public class Environment {

//     public string Name;
//     public int ObjectCount;
//     public GameObjectData[] Items;

//     public Environment(GameObjectData[] items) {
//         Name = "untitled";
//         Items = items;
//         ObjectCount = BuildingSystem.ObjectCount;
//     }
// }
using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;

public class FirebaseHandler : MonoBehaviour
{
    public string CurrentMapName { get; private set; } = "";
    public int CurrentSlot { get; private set; } = 0;

    private static string GetSlotPath(int slot) =>
        Path.Combine(Application.persistentDataPath, UserSession.Username + "-SLOT" + slot + ".json");

    private static GameObject LoadPrefab(string prefabName)
    {
        string[] folders = { "Prefabs/PlaceableObjects/", "Prefabs/DepercatedObjects/", "Prefabs/" };
        foreach (string folder in folders)
        {
            GameObject prefab = Resources.Load<GameObject>(folder + prefabName);
            if (prefab != null) return prefab;
        }
        return null;
    }

    public void DestroyObjects()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Selectable"))
            Destroy(obj);
        BuildingSystem.ObjectCount = 0;
        CurrentMapName = "";
    }

    private string Serialize(GameObjectData[] items, string mapName) =>
        JsonUtility.ToJson(new Environment(items, mapName), true);

    private Environment Deserialize(string json) =>
        JsonUtility.FromJson<Environment>(json);

    public void SaveEnvironment(int slot) => SaveEnvironment(slot, "Slot " + slot);

    public void SaveEnvironment(int slot, string mapName)
    {
        List<GameObjectData> objectData = new List<GameObjectData>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Selectable"))
        {
            GameObjectData data = new(obj);
            if (data.IsValid) objectData.Add(data);
        }

        string json = Serialize(objectData.ToArray(), mapName);
        File.WriteAllText(GetSlotPath(slot), json);
        CurrentMapName = mapName;
        CurrentSlot = slot;
        Debug.Log("Saved to: " + GetSlotPath(slot));
    }

    public void LoadEnvironment(int slot)
    {
        DestroyObjects();

        if (slot == 0) return;

        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save found at: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        Environment env = Deserialize(json);
        CurrentMapName = env.Name;
        CurrentSlot = slot;

        foreach (GameObjectData data in env.Items)
        {
            GameObject prefab = LoadPrefab(data.prefabName);
            if (prefab == null)
            {
                Debug.LogError("Prefab not found: " + data.prefabName);
                continue;
            }

            GameObject obj = Instantiate(prefab);

            PlaceableObject placeableObj = obj.GetComponent<PlaceableObject>();
            if (placeableObj != null)
            {
                placeableObj.prefabName = data.prefabName;
                placeableObj.Place();
            }

            obj.name = data.prefabName + " #" + BuildingSystem.ObjectCount++;
            obj.transform.SetPositionAndRotation(data.position, data.rotation);
            obj.transform.localScale = data.scale;
            obj.transform.SetParent(null);
            obj.SetActive(true);
        }
    }

    public void UploadEnvironment(int slot, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return;
        }

        string json = File.ReadAllText(filePath);
        Environment env = Deserialize(json);

        if (env == null || env.Items == null || env.Items.Length == 0)
        {
            Debug.LogError("Invalid or empty JSON file.");
            return;
        }

        File.WriteAllText(GetSlotPath(slot), json);
        Debug.Log("Uploaded to slot " + slot);
    }

    public void GetSlotName(int slot, Action<string> onResult)
    {
        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            onResult?.Invoke(null);
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            Environment env = Deserialize(json);

            int count = env.Items != null ? env.Items.Length : env.ObjectCount;
            string countLine = count + (count == 1 ? " object" : " objects");

            string display = env.Name;
            if (!string.IsNullOrEmpty(env.SavedAt)) display += "\n" + env.SavedAt;
            display += "\n" + countLine;

            onResult?.Invoke(display);
        }
        catch
        {
            onResult?.Invoke(null);
        }
    }

    #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void DownloadTextFile(string filename, string content);
    #endif

    public bool SlotExists(int slot) => File.Exists(GetSlotPath(slot));

    public bool RenameCurrentMap(string newName)
    {
        if (CurrentSlot == 0) return false;
        return RenameEnvironment(CurrentSlot, newName);
    }

    public bool RenameEnvironment(int slot, string newName)
    {
        string path = GetSlotPath(slot);
        if (!File.Exists(path)) return false;

        string json = File.ReadAllText(path);
        Environment env = Deserialize(json);
        env.Name = string.IsNullOrWhiteSpace(newName) ? "Slot " + slot : newName.Trim();
        CurrentMapName = env.Name;
        File.WriteAllText(path, JsonUtility.ToJson(env, true));
        return true;
    }

    public bool DeleteEnvironment(int slot)
    {
        string path = GetSlotPath(slot);
        if (!File.Exists(path)) return false;
        File.Delete(path);
        Debug.Log("Deleted slot " + slot + " at: " + path);
        return true;
    }

    public string DownloadMap(int slot)
    {
        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save found at slot " + slot);
            return "No map saved in slot " + slot;
        }

        string json     = File.ReadAllText(path);
        string filename = UserSession.Username + "-SLOT" + slot + ".json";

    #if UNITY_WEBGL && !UNITY_EDITOR
            DownloadTextFile(filename, json);
            return "Map downloaded";
    #elif UNITY_IOS && !UNITY_EDITOR
            GUIUtility.systemCopyBuffer = json;
            Debug.Log("Map JSON copied to clipboard");
            return "Map copied to clipboard";
    #else
        string dest = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            filename
        );
        File.WriteAllText(dest, json);
        Debug.Log("Map saved to Desktop: " + dest);
        return "Map saved to Desktop";
    #endif
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

    public bool IsValid => !string.IsNullOrEmpty(prefabName);

    public GameObjectData(GameObject gameObject)
    {
        PlaceableObject obj = gameObject.GetComponent<PlaceableObject>();
        if (obj == null)
        {
            Debug.LogWarning("No PlaceableObject on: " + gameObject.name);
            return;
        }

        name = gameObject.name;
        prefabName = obj.prefabName;
        position = gameObject.transform.position;
        rotation = gameObject.transform.rotation;
        scale = gameObject.transform.localScale;
    }
}

[Serializable]
public class Environment
{
    public string Name;
    public string SavedAt;
    public int ObjectCount;
    public GameObjectData[] Items;

    public Environment(GameObjectData[] items, string mapName = "Untitled")
    {
        Name = string.IsNullOrWhiteSpace(mapName) ? "Untitled" : mapName;
        SavedAt = DateTime.Now.ToString("MMM d, yyyy · h:mm tt");
        Items = items;
        ObjectCount = BuildingSystem.ObjectCount;
    }
}
