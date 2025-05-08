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
    private static List<PlaceableObject> allPlaceableObjects = new List<PlaceableObject>();
    private Color originalColor;
    private Material defaultMaterial;

    private void Awake()
    {
        allPlaceableObjects.Add(this);
    }

    private void OnDestroy()
    {
        allPlaceableObjects.Remove(this);

        if (defaultMaterial != null)
        {
            Destroy(defaultMaterial);
        }
    }

    private void GetColliderVertexPositionsLocal()
    {
        BoxCollider b = gameObject.GetComponent<BoxCollider>();
        if (b == null)
        {
            Debug.LogError("BoxCollider not found on " + gameObject.name);
            return;
        }
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
                defaultMaterial = new Material(Shader.Find("Standard"));
                objectRenderer.material = defaultMaterial;
                
                defaultMaterial.SetFloat("_Mode", 3);
                defaultMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                defaultMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                defaultMaterial.EnableKeyword("_ALPHABLEND_ON");
                defaultMaterial.renderQueue = 3000;
            }
        }

        originalColor = objectRenderer.material.color;
    }

    private void Update()
    {
        if (!Placed)
        {
            ObjectColors(); 
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
            return;
        }

        ObjectDrag drag = gameObject.GetComponent<ObjectDrag>();
        Destroy(drag);

        Placed = true;
        gameObject.tag = "Selectable";

        // Clear the selected object if this object is selected
        if (BuildingSystem.current.Selected == gameObject)
        {
            BuildingSystem.current.Selected = null;
        }

        UpdateAllObjectColors();
    }

    public void ObjectColors()
{
    if (objectRenderer == null)
    {
        InitializeRenderer();
    }

    GameObject selectedObject = BuildingSystem.current?.Selected;
    if (Placed)
    {
        if (selectedObject == gameObject)
        {
            SetColor(new Color(0, 1, 0, 1)); 
        }
        if(selectedObject != gameObject)
        {
            SetColor(originalColor); 
        }
    }
    else
    {
        if (IsOverlapping())
        {
            SetColor(new Color(1, 0, 0, 0.5f)); // Red for overlapping
        }
        else
        {
            SetColor(new Color(0, 1, 0, 0.5f)); // Add this line to set green for valid placement
        }
    }
}
    public void SetColor(Color color)
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

    private bool IsOverlapping()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogWarning($"No Collider found on {gameObject.name}. Cannot check for overlaps.");
            return false;
        }

        Collider[] colliders = Physics.OverlapBox(transform.position, collider.bounds.extents, transform.rotation, LayerMask.GetMask("PlaceableObject"));
        foreach (Collider otherCollider in colliders)
        {
            if (otherCollider.gameObject != gameObject) // Ignore self
            {
                return true; // Overlap detected
            }
        }
        return false; // No overlap
    }
}

