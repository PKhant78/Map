using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.UI;
using Firebase.Database;
#if UNITY_EDITOR
//using Firebase.Unity.Editor;
#endif
using System.Threading.Tasks;
using TMPro;

// This attribute makes the class serializable by Unity, allowing it to be inspected and used in the editor.
[System.Serializable]
public class ItemData
{
    public string name; // The name of the game object instance.
    public string prefabName; // The name of the prefab asset (without the .prefab extension) to instantiate.
    public Position position; // The position of the game object in the scene.
    public Rotation rotation; // The rotation of the game object in the scene.
    public Scale scale; // The scale of the game object in the scene.
}

// Serializable class to hold the x, y, z coordinates for position.
[System.Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;
}

// Serializable class to hold the x, y, z, w values for rotation (Quaternion).
[System.Serializable]
public class Rotation
{
    public float x;
    public float y;
    public float z;
    public float w;
}

// Serializable class to hold the x, y, z values for scale.
[System.Serializable]
public class Scale
{
    public float x;
    public float y;
    public float z;
}

// Serializable class to represent a template fetched from Firebase.
[System.Serializable]
public class TemplateData
{
    public string displayName; // The name to display on the template button.
    public List<ItemData> items; // A list of ItemData objects that make up this template.
}

// The main script responsible for fetching templates from Firebase and spawning objects.
public class ObjectSpawner : MonoBehaviour
{
    public GameObject buttonPrefab; // The prefab used to create the UI buttons for each template. Assign in the Inspector.
    public Transform buttonParent; // The Transform of the GameObject that will be the parent of the created buttons. Assign in the Inspector.
    public string templatesPath = "Templates"; // The path in the Firebase Realtime Database where the template data is stored.
    public string databaseURL = "YOUR_FIREBASE_DATABASE_URL"; // The URL of your Firebase Realtime Database. **Replace the placeholder in the Inspector.**
    private DatabaseReference databaseReference; // A reference to the Firebase Realtime Database at the specified templatesPath.
    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>(); // A cache to store loaded prefabs to avoid redundant loading.
    private Dictionary<string, TemplateData> availableTemplates = new Dictionary<string, TemplateData>(); // A dictionary to store the fetched template data. Key is the Firebase key.
    private List<GameObject> spawnedObjects = new List<GameObject>(); // A list to keep track of the GameObjects spawned by the current template.

    // Called once when the script starts.
    void Start()
    {
	#if UNITY_EDITOR
    //    FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(databaseURL); // Initializes the Firebase app with the provided database URL (important for editor use).
        #endif
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference.Child(templatesPath); // Gets a reference to the 'Templates' node in the Firebase database.

        // Error checking to ensure the button prefab and parent are assigned in the Inspector.
        if (buttonPrefab == null || buttonParent == null)
        {
            Debug.LogError("Button Prefab or Button Parent not assigned in the Inspector!");
            return; // Exit the Start method if necessary UI elements are missing.
        }

        FetchAvailableTemplates(); // Start fetching the template data from Firebase.
    }

    // Asynchronously fetches the list of available templates from Firebase.
    async void FetchAvailableTemplates()
    {
        Debug.Log("Fetching available templates from Firebase...");
        DataSnapshot snapshot = await databaseReference.GetValueAsync(); // Asynchronously get a snapshot of the data at the 'templatesPath'.

        // Check if any data exists at the specified path.
        if (snapshot.Exists)
        {
            availableTemplates.Clear(); // Clear any previously loaded templates.
            // Iterate through each child node under the 'Templates' node. Each child represents a template.
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                try
                {
                    // Deserialize the JSON data of the child snapshot into a TemplateData object.
                    TemplateData templateData = JsonConvert.DeserializeObject<TemplateData>(childSnapshot.GetRawJsonValue());
                    // Check if the deserialization was successful and if the template data is valid.
                    if (templateData != null && !string.IsNullOrEmpty(templateData.displayName) && templateData.items != null)
                    {
                        string templateKey = childSnapshot.Key; // Get the unique key of this template from Firebase.
                        availableTemplates.Add(templateKey, templateData); // Add the template data to the dictionary.
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid template data found under key: {childSnapshot.Key}");
                    }
                }
                catch (JsonException e)
                {
                    Debug.LogError($"Error parsing template data under key {childSnapshot.Key}: {e.Message}");
                }
            }
            CreateTemplateButtons(); // After fetching and parsing, create the UI buttons.
        }
        else
        {
            Debug.Log("No templates found in Firebase under path: " + templatesPath);
        }
    }

    // Creates the UI buttons for each available template.
    void CreateTemplateButtons()
    {
        // Destroy any existing buttons under the buttonParent to update the UI if templates have changed.
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        // Iterate through the fetched templates.
        foreach (var templateEntry in availableTemplates)
        {
            string templateKey = templateEntry.Key; // The Firebase key of the template.
            TemplateData templateData = templateEntry.Value; // The TemplateData object.

            // Instantiate a new button from the buttonPrefab and set its parent.
            GameObject newButton = Instantiate(buttonPrefab, buttonParent);
            Button buttonComponent = newButton.GetComponent<Button>(); // Get the Button component of the new button.

            // Find the TextMeshProUGUI component in the button's children (for the button label).
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = templateData.displayName; // Set the button's text to the template's display name.
            }

            // Add a listener to the button's onClick event to load and spawn the template's items.
            if (buttonComponent != null)
            {
                // When the button is clicked, call the LoadAndSpawnTemplate method with the template's item list.
                buttonComponent.onClick.AddListener(() => LoadAndSpawnTemplate(templateData.items));
            }
            else
            {
                Debug.LogError("Button Prefab does not have a Button component!");
            }
        }
    }

    // Loads and spawns the GameObjects for a given template's item list.
    void LoadAndSpawnTemplate(List<ItemData> itemsToSpawn)
    {
        Debug.Log("Loading and spawning template with " + itemsToSpawn.Count + " items...");

        ClearSpawnedObjects(); // Clear any previously spawned objects before spawning the new template.

        // Iterate through the list of ItemData for the current template.
        foreach (var itemData in itemsToSpawn)
        {
            GameObject spawnedObject = SpawnObject(itemData); // Instantiate the prefab for the current ItemData.
            if (spawnedObject != null)
            {
                spawnedObjects.Add(spawnedObject); // Add the newly spawned object to the tracking list.
            }
        }
    }

    // Destroys all the GameObjects that were previously spawned by loading a template.
    void ClearSpawnedObjects()
    {
        Debug.Log("Clearing " + spawnedObjects.Count + " previously spawned objects.");
        // Iterate through the list of spawned objects.
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) // Check if the object still exists before trying to destroy it.
            {
                Destroy(obj); // Destroy the GameObject.
            }
        }
        spawnedObjects.Clear(); // Clear the list of spawned objects after destroying them.
    }

    // Instantiates a prefab based on the provided ItemData.
    GameObject SpawnObject(ItemData data)
    {
        // Error checking for an empty prefab name.
        if (string.IsNullOrEmpty(data.prefabName))
        {
            Debug.LogError($"Prefab name is empty for item: {data.name}");
            return null; // Return null if no prefab name is provided.
        }

        // Check if the prefab has already been loaded into the cache.
        if (!prefabCache.ContainsKey(data.prefabName))
        {
            // Load the prefab from the Resources folder.
            GameObject prefab = Resources.Load<GameObject>(data.prefabName);
            // Error checking if the prefab was not found.
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found in Resources folder: {data.prefabName}");
                return null; // Return null if the prefab cannot be found.
            }
            prefabCache.Add(data.prefabName, prefab); // Add the loaded prefab to the cache.
        }

        // Instantiate the prefab from the cache.
        GameObject newObject = Instantiate(prefabCache[data.prefabName]);
        newObject.name = data.name; // Set the name of the instantiated object.
        newObject.transform.position = new Vector3(data.position.x, data.position.y, data.position.z); // Set the position.
        newObject.transform.rotation = new Quaternion(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w); // Set the rotation.
        newObject.transform.localScale = new Vector3(data.scale.x, data.scale.y, data.scale.z); // Set the scale.

        Debug.Log($"Spawned object: {data.name} from prefab: {data.prefabName}");
        return newObject; // Return the newly spawned GameObject.
    }
}