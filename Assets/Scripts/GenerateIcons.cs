using UnityEngine;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class IconGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera iconCamera;
    [SerializeField] private Light iconLight;
    [SerializeField] private Transform itemPosition;

    [Header("Settings")]
    [SerializeField] private int iconSize = 256;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0);
    [SerializeField] private bool autoPositionCamera = true;
    [SerializeField] private float cameraDistanceMultiplier = 1.5f;

    [Header("Save Options")]
    [SerializeField] private string savePath = "Assets/Resources/Icons/";

    private RenderTexture renderTexture;
    private Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();

    private void Awake()
    {
        // Create a render texture for our icon
        renderTexture = new RenderTexture(iconSize, iconSize, 24);

        // Setup camera
        if (iconCamera == null)
        {
            Debug.LogError("Icon Camera reference is missing");
            return;
        }

        iconCamera.targetTexture = renderTexture;
        iconCamera.clearFlags = CameraClearFlags.SolidColor;
        iconCamera.backgroundColor = backgroundColor;
    }

    public Sprite GetIconForObject(GameObject prefab)
    {
        // Check if we already have this icon cached
        string prefabName = prefab.name;
        if (iconCache.ContainsKey(prefabName))
        {
            return iconCache[prefabName];
        }

        // Try to load a pre-generated icon
        Sprite loadedSprite = Resources.Load<Sprite>("Icons/" + prefabName + "_Icon");
        if (loadedSprite != null)
        {
            iconCache[prefabName] = loadedSprite;
            return loadedSprite;
        }

        // Generate a new icon if we couldn't load one
        Sprite newIcon = GenerateIcon(prefab);
        iconCache[prefabName] = newIcon;
        return newIcon;
    }

    public Sprite GenerateIcon(GameObject itemPrefab)
    {
        // Instantiate the object in front of our camera
        GameObject item = Instantiate(itemPrefab, itemPosition.position, itemPosition.rotation);

        // Make sure it's visible
        item.SetActive(true);

        if (autoPositionCamera)
        {
            PositionCameraForObject(item);
        }

        // Render the camera's view to our render texture
        iconCamera.Render();

        // Create a texture from the render texture
        Texture2D icon = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        icon.ReadPixels(new Rect(0, 0, iconSize, iconSize), 0, 0);
        icon.Apply();
        RenderTexture.active = null;

        // Create a sprite from the texture
        Sprite iconSprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height),
                                         new Vector2(0.5f, 0.5f));

        // Clean up
        Destroy(item);

        return iconSprite;
    }

    private void PositionCameraForObject(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        // Calculate bounds
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        // Position the camera to frame the object nicely
        float objectSize = bounds.size.magnitude;
        float distance = objectSize * cameraDistanceMultiplier;

        // Adjust camera to point at the object's center
        iconCamera.transform.position = bounds.center - iconCamera.transform.forward * distance;
        iconCamera.transform.LookAt(bounds.center);
    }

#if UNITY_EDITOR
    public void SaveIconToFile(GameObject prefab)
    {
        // Create directory if it doesn't exist
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            AssetDatabase.Refresh();
        }

        // Generate the icon
        Sprite iconSprite = GenerateIcon(prefab);
        Texture2D iconTexture = GetTextureFromSprite(iconSprite);

        // Save as PNG
        byte[] bytes = iconTexture.EncodeToPNG();
        string iconPath = savePath + prefab.name + "_Icon.png";
        File.WriteAllBytes(iconPath, bytes);

        AssetDatabase.Refresh();
        Debug.Log("Icon saved to: " + iconPath);

        // Set texture import settings
        TextureImporter importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }

    private Texture2D GetTextureFromSprite(Sprite sprite)
    {
        // Create a new Texture2D from the sprite
        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false);
        Color[] pixels = sprite.texture.GetPixels(
            (int)sprite.rect.x,
            (int)sprite.rect.y,
            (int)sprite.rect.width,
            (int)sprite.rect.height
        );
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
#endif
}
