using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [Header("Placement")]
    public LayerMask placementMask; // ground / farm layer
    public Color previewColor = new Color(1, 1, 1, 0.5f);
    public Color blockedPreviewColor = new Color(1f, 0.6f, 0.6f, 0.9f);
    public float gridSize = 1f;
    [Header("Preview Sorting")]
    public string previewSortingLayerName = "Objects";
    public int previewSortingOrder = 5000;

    private GameObject previewObject;
    private GameObject prefabToPlace;
    private bool isPlacing = false;
    private Vector2Int previewCell;
    private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isPlacing || previewObject == null)
            return;

        FollowMouse();

        if (Input.GetMouseButtonDown(0))
        {
            PlaceObject();
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    public void StartPlacement(GameObject prefab)
    {
        if (isPlacing)
            return;

        prefabToPlace = prefab;
        previewObject = Instantiate(prefab);
        SetPreviewVisual(previewObject);
        isPlacing = true;
    }

    void FollowMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;

        previewCell = WorldToCell(worldPos);
        Vector3 snappedPos = CellToWorld(previewCell);
        previewObject.transform.position = snappedPos;

        bool blocked = occupiedCells.Contains(previewCell);
        ApplyPreviewColor(blocked ? blockedPreviewColor : previewColor);
    }

    void PlaceObject()
    {
        bool blocked = occupiedCells.Contains(previewCell);
        if (blocked)
        {
            AudioManager.Instance.PlayClick();
            return;
        }

        AudioManager.Instance.PlayPlacement();
        GameObject placed = Instantiate(prefabToPlace, previewObject.transform.position, Quaternion.identity);
        occupiedCells.Add(previewCell);
        CleanupPreview();
    }

    void CancelPlacement()
    {
        AudioManager.Instance.PlayClick();
        CleanupPreview();
    }

    void CleanupPreview()
    {
        Destroy(previewObject);
        previewObject = null;
        prefabToPlace = null;
        isPlacing = false;
    }

    void SetPreviewVisual(GameObject obj)
    {
        foreach (var sorter in obj.GetComponentsInChildren<SortingByY>(true))
        {
            sorter.enabled = false;
        }

        foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.color = previewColor;
            if (!string.IsNullOrEmpty(previewSortingLayerName))
            {
                sr.sortingLayerName = previewSortingLayerName;
            }
            sr.sortingOrder = previewSortingOrder; 
        }

        foreach (var col in obj.GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }
    }

    Vector2Int WorldToCell(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / gridSize);
        int y = Mathf.RoundToInt(worldPos.y / gridSize);
        return new Vector2Int(x, y);
    }

    Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x * gridSize, cell.y * gridSize, 0f);
    }

    void ApplyPreviewColor(Color c)
    {
        if (previewObject == null)
        {
            return;
        }

        foreach (var sr in previewObject.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.color = c;
            if (!string.IsNullOrEmpty(previewSortingLayerName))
            {
                sr.sortingLayerName = previewSortingLayerName;
            }
            sr.sortingOrder = previewSortingOrder;
        }
    }
}
