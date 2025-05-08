using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    public bool Placed { get; private set; }
    public string prefabName;
    public Vector3Int Size { get; private set; }
    private Vector3[] Vertices;
    private Renderer objectRenderer;  
    private static PlaceableObject lastPlacedObject;
    private static List<PlaceableObject> allPlaceableObjects = new List<PlaceableObject>();

    private void Awake()
    {
        allPlaceableObjects.Add(this);
    }

    private void OnDestroy()
    {
        allPlaceableObjects.Remove(this);
    }

    private void GetColliderVertexPositionsLocal()
    {
        BoxCollider b = gameObject.GetComponent<BoxCollider>();
        Vertices = new Vector3[4];
        Vertices[0] = b.center + new Vector3(-b.size.x, -b.size.y, -b.size.z) * 0.5f;
        Vertices[1] = b.center + new Vector3(b.size.x, -b.size.y, -b.size.z) * 0.5f;
        Vertices[2] = b.center + new Vector3(b.size.x, -b.size.y, b.size.z) * 0.5f;
        Vertices[3] = b.center + new Vector3(-b.size.x, -b.size.y, b.size.z) * 0.5f;
    }

    private void CalculateSizeInCells()
    {
        Vector3Int[] vertices = new Vector3Int[Vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            vertices[i] = BuildingSystem.current.gridLayout.WorldToCell(worldPos);
        }

        Size = new Vector3Int(x: Math.Abs((vertices[0] - vertices[1]).x),
                              y: Math.Abs((vertices[0] - vertices[3]).y),
                              z: 1);
    }

    public Vector3 GetStartPosition()
    {
        return transform.TransformPoint(Vertices[0]);
    }

    private void Start()
    {
        GetColliderVertexPositionsLocal();
        CalculateSizeInCells();
        
        InitializeRenderer();
        ObjectColors();
    }

    private void InitializeRenderer()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            objectRenderer = GetComponentInChildren<Renderer>();
            
            if (objectRenderer == null)
            {
                objectRenderer = gameObject.AddComponent<MeshRenderer>();
                Material defaultMaterial = new Material(Shader.Find("Standard"));
                objectRenderer.material = defaultMaterial;
                
                // Configure material for transparency
                defaultMaterial.SetFloat("_Mode", 3);
                defaultMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                defaultMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                defaultMaterial.EnableKeyword("_ALPHABLEND_ON");
                defaultMaterial.renderQueue = 3000;
            }
        }
    }

    private void Update()
    {
        if (!Placed)
        {
            if (IsOverlapping())
            {
                SetColor(new Color(1, 0, 0, 0.5f)); // Transparent red for invalid placement
            }
            else
            {
                SetColor(new Color(0, 1, 0, 0.5f)); // Transparent green for valid placement
            }
        }
    }

    public void Rotate()
    {
        transform.Rotate(new Vector3(0, 90, 0));
        Size = new Vector3Int(Size.y, Size.x, 1);

        Vector3[] vertices = new Vector3[Vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vertices[(i + 1) % Vertices.Length];
        }

        Vertices = vertices;
    }

    public void Place()
    {
        if (IsOverlapping())
        {
            Debug.LogWarning("Cannot place object here - Overlapping with another object.");
            return; // Prevent placement
        }

        ObjectDrag drag = gameObject.GetComponent<ObjectDrag>();
        Destroy(drag);

        lastPlacedObject = this;
        Placed = true;
        gameObject.tag = "Selectable";

        // Reset material to opaque
        Material material = objectRenderer.material;
        material.color = Color.white; // Default color
        material.SetFloat("_Mode", 0);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.DisableKeyword("_ALPHABLEND_ON");
        material.renderQueue = -1;

        UpdateAllObjectColors();
    }

    private void SetColor(Color color)
    {
        if (objectRenderer is MeshRenderer meshRenderer)
        {
            if (meshRenderer.materials.Length > 1)
            {
                Material[] materials = meshRenderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].color = color;
                }
                meshRenderer.materials = materials;
            }
            else
            {
                meshRenderer.material.color = color;
            }
        }
        else if (objectRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
        {
            if (skinnedMeshRenderer.materials.Length > 1)
            {
                Material[] materials = skinnedMeshRenderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].color = color;
                }
                skinnedMeshRenderer.materials = materials;
            }
            else
            {
                skinnedMeshRenderer.material.color = color;
            }
        }
    }

    private void SetOutlineColor(Color outlineColor)
    {
        if (objectRenderer is MeshRenderer meshRenderer)
        {
            foreach (Material material in meshRenderer.materials)
            {
                if (material.HasProperty("_Outline_Color"))
                {
                    material.SetColor("_Outline_Color", outlineColor); // Update only the outline color
                }
            }
        }
        else if (objectRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
        {
            foreach (Material material in skinnedMeshRenderer.materials)
            {
                if (material.HasProperty("_Outline_Color"))
                {
                    material.SetColor("_Outline_Color", outlineColor); // Update only the outline color
                }
            }
        }
    }

    public void UpdateState(bool isPlaced, bool isSelected)
    {
        Placed = isPlaced;
        
        if (isSelected)
        {
            BuildingSystem.current.Selected = gameObject;
        }
        else if (BuildingSystem.current.Selected == gameObject)
        {
            BuildingSystem.current.Selected = null;
        }
        
        UpdateAllObjectColors();
    }

    public static void UpdateAllObjectColors()
    {
        foreach (PlaceableObject obj in allPlaceableObjects)
        {
            if (obj != null)
            {
                obj.ObjectColors();
            }
        }
    }

    public static void SetSelectedObject(GameObject selectedObject)
    {
        if (BuildingSystem.current != null)
        {
            BuildingSystem.current.Selected = selectedObject;
            UpdateAllObjectColors();
        }
    }

    public void ObjectColors()
    {
        if (objectRenderer == null)
        {
            InitializeRenderer();
            if (objectRenderer == null)
            {
                Debug.LogError($"Cannot set colors - No Renderer on {gameObject.name}");
                return;
            }
        }

        try
        {
            if (BuildingSystem.current == null)
            {
                return;
            }

            if (IsOverlapping())
            {
                SetOutlineColor(new Color(1, 0, 0, 1)); // Red outline for invalid placement
            }
            else if (BuildingSystem.current.Selected == gameObject)
            {
                SetOutlineColor(new Color(0, 1, 0, 1)); // Green outline for selected
            }
            else if (Placed)
            {
                SetOutlineColor(new Color(1, 1, 0, 1)); // Yellow outline for already placed
            }
            else
            {
                SetOutlineColor(new Color(1, 0, 0, 1)); // Red outline for can't place
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in ObjectColors for {gameObject.name}: {e.Message}");
        }
    }

    private bool IsOverlapping()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, GetComponent<Collider>().bounds.extents, transform.rotation, LayerMask.GetMask("PlaceableObject"));
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject) // Ignore self
            {
                return true; // Overlap detected
            }
        }
        return false; // No overlap
    }

    public void SetTransparentGreen()
    {
        if (objectRenderer == null)
        {
            InitializeRenderer();
        }

        Material material = objectRenderer.material;
        material.color = new Color(0, 1, 0, 0.5f); // Transparent green
        material.SetFloat("_Mode", 3);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.EnableKeyword("_ALPHABLEND_ON");
        material.renderQueue = 3000;
    }
}

