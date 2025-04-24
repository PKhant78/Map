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
        ObjectColors();
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

    public virtual void Place()
    {
        ObjectDrag drag = gameObject.GetComponent<ObjectDrag>();
        Destroy(drag);

        lastPlacedObject = this;
        Placed = true;
        gameObject.tag = "Selectable";
        
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
            if (BuildingSystem.current.Selected == gameObject)
            {
                SetColor(new Color(1, 1, 0, 0.3f)); // Yellow - Selected
            }
            else if (this == lastPlacedObject && Placed)
            {
                SetColor(new Color(0, 1, 0, 0.3f)); // Green - Last Placed
            }
            else if (Placed)
            {
                SetColor(new Color(0.5f, 0.5f, 0.5f, 0.2f)); // Gray - Already Placed
            }
            else
            {
                SetColor(new Color(1, 0, 0, 0.3f)); // Red - Can't Place
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in ObjectColors for {gameObject.name}: {e.Message}");
        }
    }
}

