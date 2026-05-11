using UnityEngine;

public class FireflyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float moveDuration = 2f;
    [Header("Idle")]
    public float minIdleDuration = 5f;
    public float idleLoopRadius = 0.05f;
    public float idleLoopSpeed = 3f;
    [Header("Screen Bounds")]
    public float viewportPadding = 0.05f;
    [Header("Drag")]
    public LayerMask dragLayerMask = ~0;
    [Header("Core Scoring")]
    public float coreTickInterval = 1.5f;

    private static int draggingCount = 0;
    public static bool AnyDragging => draggingCount > 0;

    private Vector3 targetDirection;
    private float stateTimer = 0f;
    private bool isIdle = true;
    private bool isDragging = false;
    private Vector3 dragOffset;
    private float dragDepth;
    private Collider2D col2D;
    private Collider col3D;
    private bool isInCore = false;
    private float coreTimer = 0f;
    private Vector3 idleAnchor;
    private float idlePhase;
    private bool isInPillar = false;
    private PillarController currentPillar;

    void Start()
    {
        col2D = GetComponent<Collider2D>();
        col3D = GetComponent<Collider>();
        EnterIdleState();
    }

    void Update()
    {
        var cam = Camera.main;

        if (cam == null && isDragging)
        {
            return;
        }

        HandleDragInput(cam);

        stateTimer += Time.deltaTime;

        TickCoreIfInside();

        if (isDragging)
        {
            DragUpdate(cam);
            return;
        }

        if (isInPillar)
        {
            IdleWiggle(cam);
            return;
        }

        if (isIdle)
        {
            IdleWiggle(cam);
            if (!isInPillar && stateTimer >= minIdleDuration)
            {
                EnterMoveState();
            }
            return;
        }

        // moving
        KeepWithinLand();
        transform.Translate(targetDirection * moveSpeed * Time.deltaTime, Space.World);

        if (stateTimer >= moveDuration)
        {
            EnterIdleState();
        }
    }

    void EnterIdleState()
    {
        isIdle = true;
        stateTimer = 0f;
        targetDirection = Vector3.zero;
        idleAnchor = transform.position;
        idlePhase = Random.Range(0f, Mathf.PI * 2f);
    }

    void EnterMoveState()
    {
        isIdle = false;
        stateTimer = 0f;
        ChooseNewDirection();
    }

    void ChooseNewDirection()
    {
        if (!TryGetActiveBounds(out Bounds bounds))
        {
            targetDirection = Vector3.zero;
            return;
        }

        float padX = bounds.extents.x * viewportPadding;
        float padY = bounds.extents.y * viewportPadding;
        Vector3 pos = transform.position;

        bool nearEdge = pos.x < bounds.min.x + padX || pos.x > bounds.max.x - padX ||
                        pos.y < bounds.min.y + padY || pos.y > bounds.max.y - padY;

        if (nearEdge)
        {
            Vector2 toCenter = bounds.center - pos;
            targetDirection = new Vector3(toCenter.x, toCenter.y, 0f).normalized;
        }
        else
        {
            float angle = Random.Range(0f, 360f);
            targetDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f).normalized;
        }
    }

    void KeepWithinLand()
    {
        if (!TryGetActiveBounds(out Bounds bounds))
        {
            return;
        }

        Vector3 pos = transform.position;
        float clampedX = Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x);
        float clampedY = Mathf.Clamp(pos.y, bounds.min.y, bounds.max.y);
        transform.position = new Vector3(clampedX, clampedY, pos.z);

        if (pos.x <= bounds.min.x || pos.x >= bounds.max.x || pos.y <= bounds.min.y || pos.y >= bounds.max.y)
        {
            Vector2 toCenter = bounds.center - transform.position;
            targetDirection = new Vector3(toCenter.x, toCenter.y, 0f).normalized;
        }
    }

    void HandleDragInput(Camera cam)
    {
        if (cam == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag(cam);
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                draggingCount = Mathf.Max(0, draggingCount - 1);
            }
            isDragging = false;
            EnterIdleState();
        }
    }

    void DragUpdate(Camera cam)
    {
        if (cam == null)
        {
            return;
        }

        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragDepth));
        transform.position = worldPoint + dragOffset;
        KeepWithinLand();
    }

    void TryStartDrag(Camera cam)
    {
        if (cam == null)
        {
            return;
        }

        if (isDragging)
        {
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        Ray ray = cam.ScreenPointToRay(mousePos);

        bool hit = false;

        if (col2D != null)
        {
            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, Mathf.Infinity, dragLayerMask);
            if (hit2D.collider != null && hit2D.collider.transform == transform)
            {
                hit = true;
            }
        }
        else if (col3D != null)
        {
            if (col3D.Raycast(ray, out RaycastHit hit3D, Mathf.Infinity))
            {
                hit = true;
            }
        }

        if (!hit)
        {
            return;
        }

        AudioManager.Instance.PlayTwinkle();

        isDragging = true;
        draggingCount++;
        dragDepth = cam.WorldToScreenPoint(transform.position).z;
        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, dragDepth));
        dragOffset = transform.position - worldPoint;
    }

    void IdleWiggle(Camera cam)
    {
        float theta = idlePhase + Time.time * idleLoopSpeed;
        Vector3 offset = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0f) * idleLoopRadius;
        transform.position = idleAnchor + offset;
        KeepWithinLand();
        idleAnchor = transform.position;
    }

    bool TryGetActiveBounds(out Bounds bounds)
    {
        bounds = new Bounds();

        if (PlacementManager.Instance == null || PlacementManager.Instance.landAreas == null)
        {
            return false;
        }

        Bounds? candidate = null;
        float bestDistance = float.MaxValue;
        Vector3 pos = transform.position;

        foreach (var land in PlacementManager.Instance.landAreas)
        {
            if (land == null || !land.unlocked || land.landCollider == null)
            {
                continue;
            }

            Bounds b = land.landCollider.bounds;

            if (b.Contains(pos))
            {
                bounds = b;
                return true;
            }

            float dist = SqrDistanceToBounds(b, pos);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                candidate = b;
            }
        }

        if (candidate.HasValue)
        {
            bounds = candidate.Value;
            return true;
        }

        return false;
    }

    float SqrDistanceToBounds(Bounds b, Vector3 p)
    {
        float dx = Mathf.Max(b.min.x - p.x, 0f, p.x - b.max.x);
        float dy = Mathf.Max(b.min.y - p.y, 0f, p.y - b.max.y);
        return dx * dx + dy * dy;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Core"))
        {
            isInCore = true;
            coreTimer = coreTickInterval; // immediate tick on enter
            if (GameManager.Instance != null)
            {
                GameManager.Instance.IncrementLightCount();
            }
        }
        else if (other.CompareTag("Pillar"))
        {
            // Only bind to one pillar at a time to avoid double counting when pillars overlap.
            if (currentPillar != null)
            {
                return;
            }

            PillarController pillar = other.GetComponent<PillarController>();
            if (pillar != null && pillar.TryAddFirefly(this))
            {
                currentPillar = pillar;
                isInPillar = true;
                EnterIdleState();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Core"))
        {
            isInCore = false;
            coreTimer = 0f;
        }
        else if (other.CompareTag("Pillar"))
        {
            if (currentPillar != null && other.GetComponent<PillarController>() == currentPillar)
            {
                currentPillar.RemoveFirefly(this);
                currentPillar = null;
            }
            isInPillar = false;
        }
    }

    void OnDestroy()
    {
        if (currentPillar != null)
        {
            currentPillar.RemoveFirefly(this);
        }

        if (isDragging)
        {
            draggingCount = Mathf.Max(0, draggingCount - 1);
        }
    }

    void TickCoreIfInside()
    {
        if (!isInCore)
        {
            return;
        }

        coreTimer += Time.deltaTime;
        if (coreTimer >= coreTickInterval)
        {
            coreTimer -= coreTickInterval;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.IncrementLightCount();
            }
        }
    }
}
