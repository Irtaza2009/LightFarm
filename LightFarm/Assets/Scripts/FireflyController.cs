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
        if (cam == null)
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
                EnterMoveState(cam);
            }
            return;
        }

        // moving
        KeepWithinViewport(cam);
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

    void EnterMoveState(Camera cam)
    {
        isIdle = false;
        stateTimer = 0f;
        ChooseNewDirection(cam);
    }

    void ChooseNewDirection(Camera cam)
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);

        // toward center if near edge
        bool nearEdge = viewportPos.x < viewportPadding || viewportPos.x > 1f - viewportPadding ||
                        viewportPos.y < viewportPadding || viewportPos.y > 1f - viewportPadding;

        if (nearEdge)
        {
            Vector2 toCenter = new Vector2(0.5f - viewportPos.x, 0.5f - viewportPos.y);
            targetDirection = new Vector3(toCenter.x, toCenter.y, 0f).normalized;
        }
        else
        {
            float angle = Random.Range(0f, 360f);
            targetDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f).normalized;
        }
    }

    void KeepWithinViewport(Camera cam)
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        viewportPos.x = Mathf.Clamp(viewportPos.x, viewportPadding, 1f - viewportPadding);
        viewportPos.y = Mathf.Clamp(viewportPos.y, viewportPadding, 1f - viewportPadding);
        transform.position = cam.ViewportToWorldPoint(viewportPos);

        if (viewportPos.x <= viewportPadding || viewportPos.x >= 1f - viewportPadding ||
            viewportPos.y <= viewportPadding || viewportPos.y >= 1f - viewportPadding)
        {
            Vector2 toCenter = new Vector2(0.5f - viewportPos.x, 0.5f - viewportPos.y);
            targetDirection = new Vector3(toCenter.x, toCenter.y, 0f).normalized;
        }
    }

    void HandleDragInput(Camera cam)
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag(cam);
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            EnterIdleState();
        }
    }

    void DragUpdate(Camera cam)
    {
        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragDepth));
        transform.position = worldPoint + dragOffset;
        KeepWithinViewport(cam);
    }

    void TryStartDrag(Camera cam)
    {
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
        dragDepth = cam.WorldToScreenPoint(transform.position).z;
        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, dragDepth));
        dragOffset = transform.position - worldPoint;
    }

    void IdleWiggle(Camera cam)
    {
        float theta = idlePhase + Time.time * idleLoopSpeed;
        Vector3 offset = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0f) * idleLoopRadius;
        transform.position = idleAnchor + offset;
        KeepWithinViewport(cam);
        idleAnchor = transform.position;
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
            Debug.Log("Firefly entered core, starting ticks.");
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
            Debug.Log("Firefly ticking light inside core");
        }
    }
}
