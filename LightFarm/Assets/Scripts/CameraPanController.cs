using UnityEngine;

// Simple smooth horizontal camera pan with clamped limits.
[RequireComponent(typeof(Camera))]
public class CameraPanController : MonoBehaviour
{
    [Header("Pan Settings")]
    public float panSpeed = 0.15f;          // Drag sensitivity.
    public float smoothTime = 0.12f;        // Damping for smoothing.
    public float minX = -20f;
    public float maxX = 20f;

    private Camera cam;
    private Vector3 targetPos;
    private Vector3 velocity;               // For SmoothDamp.
    private bool isDragging = false;
    private Vector3 lastMousePos;

    void Awake()
    {
        cam = GetComponent<Camera>();
        targetPos = transform.position;
    }

    void Update()
    {
        if (FireflyController.AnyDragging)
        {
            // Freeze camera while dragging fireflies to avoid accidental panning.
            isDragging = false;
            targetPos = transform.position;
            velocity = Vector3.zero;
            return;
        }

        HandleDrag();
        SmoothMove();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!isDragging)
        {
            return;
        }

        Vector3 currentMousePos = Input.mousePosition;
        Vector3 delta = currentMousePos - lastMousePos;
        lastMousePos = currentMousePos;

        // Convert pixel delta to world delta at camera depth.
        float worldDeltaX = delta.x * panSpeed * Time.deltaTime;

        float desiredX = Mathf.Clamp(targetPos.x - worldDeltaX, minX, maxX);
        targetPos = new Vector3(desiredX, targetPos.y, targetPos.z);
    }

    void SmoothMove()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        Vector3 smoothed = Vector3.SmoothDamp(pos, new Vector3(targetPos.x, pos.y, pos.z), ref velocity, smoothTime);
        smoothed.x = Mathf.Clamp(smoothed.x, minX, maxX);
        transform.position = smoothed;
    }
}
