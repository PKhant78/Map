#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class FolderIconGenerator : EditorWindow
{
    private IconGenerator iconGenerator;
    private string prefabFolderPath = "Assets/Prefabs";
    private bool includeSubfolders = true;
    private bool overwriteExisting = false;
    private GameObject testPrefab;
    private bool showTestObject = true;
    private float testDuration = 3.0f;
    private bool isTestingIcon = false;
    private GameObject testInstance;
    private float testStartTime;

    // Track instantiated objects for cleanup
    private List<GameObject> instantiatedObjects = new List<GameObject>();

    [MenuItem("Tools/Folder Icon Generator")]
    public static void ShowWindow()
    {
        GetWindow<FolderIconGenerator>("Folder Icon Generator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Folder Icon Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        iconGenerator = (IconGenerator)EditorGUILayout.ObjectField("Icon Generator", iconGenerator, typeof(IconGenerator), true);

        // Test Icon Generation Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Test Icon Generation", EditorStyles.boldLabel);

        testPrefab = (GameObject)EditorGUILayout.ObjectField("Test Prefab", testPrefab, typeof(GameObject), false);
        showTestObject = EditorGUILayout.Toggle("Show Test Object", showTestObject);
        testDuration = EditorGUILayout.Slider("Preview Duration (seconds)", testDuration, 1f, 10f);

        EditorGUI.BeginDisabledGroup(isTestingIcon || testPrefab == null || iconGenerator == null);
        if (GUILayout.Button("Test Generate Icon"))
        {
            TestGenerateIcon();
        }
        EditorGUI.EndDisabledGroup();

        if (isTestingIcon)
        {
            EditorGUILayout.HelpBox("Testing in progress... Object will be visible for " +
                (testDuration - (Time.realtimeSinceStartup - testStartTime)).ToString("F1") + " seconds.",
                MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Folder Settings", EditorStyles.boldLabel);

        prefabFolderPath = EditorGUILayout.TextField("Prefabs Folder Path", prefabFolderPath);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Browse..."))
        {
            string path = EditorUtility.OpenFolderPanel("Select Prefabs Folder", "Assets", "");
            if (path.StartsWith(Application.dataPath))
            {
                prefabFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();

        includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Icons", overwriteExisting);

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(isTestingIcon);
        if (GUILayout.Button("Generate Icons for All Prefabs") && iconGenerator != null)
        {
            GenerateIconsForFolder();
        }

        // Add a cleanup button
        if (GUILayout.Button("Force Cleanup All Objects"))
        {
            CleanupAllObjects();
        }
        EditorGUI.EndDisabledGroup();

        // If test is ongoing, force repaint to update the timer
        if (isTestingIcon)
        {
            Repaint();

            // Check if time's up for the test
            if (Time.realtimeSinceStartup - testStartTime > testDuration)
            {
                FinishIconTest();
            }
        }
    }

    private void TestGenerateIcon()
    {
        if (testPrefab == null || iconGenerator == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign both a Test Prefab and Icon Generator", "OK");
            return;
        }

        // Clean up any previous test
        if (testInstance != null)
        {
            DestroyImmediate(testInstance);
        }

        Debug.Log("=== STARTING ICON GENERATION TEST ===");
        Debug.Log($"Test prefab: {testPrefab.name}");

        // Create directory if it doesn't exist
        string savePath = "Assets/Resources/Icons/";
        if (!Directory.Exists(savePath))
        {
            Debug.Log($"Creating directory: {savePath}");
            Directory.CreateDirectory(savePath);
            AssetDatabase.Refresh();
        }

        // Instantiate object at the item position
        Transform itemPosition = iconGenerator.transform.Find("ItemPosition");
        if (itemPosition == null)
        {
            // Try to find a child with ItemPosition in the name
            foreach (Transform child in iconGenerator.transform)
            {
                if (child.name.Contains("ItemPosition"))
                {
                    itemPosition = child;
                    break;
                }
            }

            // If still null, use the IconGenerator's position
            if (itemPosition == null)
            {
                Debug.LogWarning("ItemPosition not found, using IconGenerator's position");
                itemPosition = iconGenerator.transform;
            }
        }

        // Instantiate the prefab
        testInstance = Instantiate(testPrefab, itemPosition.position, itemPosition.rotation);
        testInstance.name = testPrefab.name + " (Test Instance)";
        Debug.Log($"Instantiated test object at {itemPosition.position}");

        // Make visuals of everything
        iconGenerator.gameObject.SetActive(true);

        // Find camera and make it visible in scene view by selecting it
        Camera iconCamera = iconGenerator.GetComponent<Camera>();
        if (iconCamera == null)
        {
            iconCamera = iconGenerator.GetComponentInChildren<Camera>();
        }

        if (iconCamera != null)
        {
            Selection.activeGameObject = iconCamera.gameObject;
            Debug.Log($"Selected icon camera: {iconCamera.name}");

            // Position camera for object
            Renderer[] renderers = testInstance.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }

                float objectSize = bounds.size.magnitude;
                float distance = objectSize * 1.5f; // Similar to what IconGenerator uses

                Debug.Log($"Object bounds center: {bounds.center}, size: {bounds.size}, magnitude: {objectSize}");
                Debug.Log($"Positioning camera at distance: {distance}");

                // Show a line in scene view from camera to object
                Debug.DrawLine(iconCamera.transform.position, bounds.center, Color.red, testDuration);

                // Position camera to look at object
                iconCamera.transform.position = bounds.center - iconCamera.transform.forward * distance;
                iconCamera.transform.LookAt(bounds.center);

                Debug.Log($"Camera positioned at: {iconCamera.transform.position}, looking at: {bounds.center}");
            }
            else
            {
                Debug.LogWarning("No renderers found on test object");
            }
        }
        else
        {
            Debug.LogWarning("Icon camera not found");
        }

        // Start timer for cleanup
        isTestingIcon = true;
        testStartTime = Time.realtimeSinceStartup;

        // If not showing the test object, generate icon immediately
        if (!showTestObject)
        {
            GenerateTestIcon();
        }
        else
        {
            Debug.Log("Showing test object for preview. Icon will be generated after timer ends.");
            EditorUtility.DisplayDialog("Test in Progress",
                $"Test object '{testPrefab.name}' has been instantiated and will be visible for {testDuration} seconds.\n\n" +
                "The camera has been selected in the hierarchy.\n\n" +
                "Check the Scene view to see how the camera is positioned relative to the object.", "OK");
        }
    }

    private void GenerateTestIcon()
    {
        if (testInstance != null && iconGenerator != null)
        {
            Debug.Log("Generating test icon now...");

            try
            {
                // Generate icon using the real instance (instead of re-instantiating)
                Sprite iconSprite = iconGenerator.GenerateIcon(testPrefab);

                // Save to file
                string savePath = "Assets/Resources/Icons/";
                string iconPath = savePath + testPrefab.name + "_Icon.png";

                // Get texture from sprite
                Texture2D iconTexture = new Texture2D((int)iconSprite.rect.width, (int)iconSprite.rect.height, TextureFormat.RGBA32, false);
                Color[] pixels = iconSprite.texture.GetPixels(
                    (int)iconSprite.rect.x,
                    (int)iconSprite.rect.y,
                    (int)iconSprite.rect.width,
                    (int)iconSprite.rect.height
                );
                iconTexture.SetPixels(pixels);
                iconTexture.Apply();

                // Save as PNG
                byte[] bytes = iconTexture.EncodeToPNG();
                File.WriteAllBytes(iconPath, bytes);

                AssetDatabase.Refresh();
                Debug.Log("Test icon saved to: " + iconPath);

                // Set texture import settings
                TextureImporter importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }

                // Ping the generated icon in the project window
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath));

                EditorUtility.DisplayDialog("Test Complete",
                    $"Icon for '{testPrefab.name}' has been generated and saved to:\n\n{iconPath}", "OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error generating test icon: {e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("Error", $"Failed to generate test icon: {e.Message}", "OK");
            }
        }
    }

    private void FinishIconTest()
    {
        if (isTestingIcon)
        {
            if (showTestObject)
            {
                // Now generate the icon 
                GenerateTestIcon();
            }

            // Clean up
            if (testInstance != null)
            {
                DestroyImmediate(testInstance);
                testInstance = null;
            }

            isTestingIcon = false;
            Debug.Log("=== ICON GENERATION TEST COMPLETE ===");
        }
    }

    private void GenerateIconsForFolder()
    {
        if (iconGenerator == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign an IconGenerator", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(prefabFolderPath))
        {
            EditorUtility.DisplayDialog("Error", "Invalid folder path", "OK");
            return;
        }

        // Get all prefabs in the folder
        string[] prefabGuids;
        if (includeSubfolders)
        {
            prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolderPath });
        }
        else
        {
            prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolderPath });
            // Filter out prefabs in subfolders
            List<string> filteredGuids = new List<string>();
            foreach (string guid in prefabGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string directoryName = Path.GetDirectoryName(assetPath).Replace('\\', '/');
                if (directoryName == prefabFolderPath)
                {
                    filteredGuids.Add(guid);
                }
            }
            prefabGuids = filteredGuids.ToArray();
        }

        if (prefabGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Result", "No prefabs found in the selected folder", "OK");
            return;
        }

        // Clean up any leftover objects before we start
        CleanupAllObjects();

        int count = 0;
        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null)
            {
                Debug.LogWarning($"Failed to load prefab at path: {assetPath}");
                continue;
            }

            // Check if icon already exists and if we should skip it
            string iconPath = "Assets/Resources/Icons/" + prefab.name + "_Icon.png";
            if (!overwriteExisting && File.Exists(iconPath))
            {
                Debug.Log("Skipping existing icon for: " + prefab.name);
                continue;
            }

            // Generate the icon - let the IconGenerator handle instantiation and destruction
            try
            {
                Debug.Log($"Generating icon for: {prefab.name}");
                iconGenerator.SaveIconToFile(prefab);
                count++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error generating icon for {prefab.name}: {e.Message}");
            }

            // Update progress bar
            EditorUtility.DisplayProgressBar("Generating Icons",
                "Processing " + prefab.name, (float)count / prefabGuids.Length);

            // Give Unity time to process the previous operation
            EditorApplication.QueuePlayerLoopUpdate();
            System.Threading.Thread.Sleep(100); // Short delay to ensure rendering completes
        }

        // Do a final cleanup pass
        CleanupAllObjects();

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Complete", "Generated " + count + " icons", "OK");
    }

    // Helper method to force destroy all found objects
    private void CleanupAllObjects()
    {
        // Find all objects with "(Clone)" in their name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Check for null before accessing properties
            if (obj != null && obj.scene.name != null)
            {
                // Look for clones which are likely our instantiated prefabs
                if (obj.name.Contains("(Clone)") || obj.name.Contains("(Test Instance)"))
                {
                    string objName = obj.name; // Store name before destroying
                    DestroyImmediate(obj);
                    Debug.Log("Cleaned up: " + objName);
                }
            }
        }

        // Force update
        EditorApplication.QueuePlayerLoopUpdate();
    }

    // Make sure objects are cleaned up if the window is closed
    private void OnDestroy()
    {
        CleanupAllObjects();
        // Make sure to end any ongoing test
        if (isTestingIcon)
        {
            FinishIconTest();
        }
    }
}
#endif
