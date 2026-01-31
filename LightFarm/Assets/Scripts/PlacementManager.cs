using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [Header("Placement")]
    public LayerMask placementMask; // ground / farm layer
    public Color previewColor = new Color(1, 1, 1, 0.5f);

    private GameObject previewObject;
    private GameObject prefabToPlace;
    private bool isPlacing = false;

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
        previewObject.transform.position = worldPos;
    }

    void PlaceObject()
    {
        AudioManager.Instance.PlayPlacement();
        GameObject placed = Instantiate(prefabToPlace, previewObject.transform.position, Quaternion.identity);
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
        foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.color = previewColor;
        }

        foreach (var col in obj.GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }
    }
}
