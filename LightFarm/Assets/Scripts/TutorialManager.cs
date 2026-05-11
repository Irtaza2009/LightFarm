using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private enum TutorialStep
    {
        DragToCore,
        WaitForTenLights,
        PlaceNectarTower,
        DragToTower,
        PanCamera,
        Complete
    }

    [Header("UI")]
    public TMP_Text tutorialText;

    [Header("References")]
    public CoreController core;

    [Header("Checks")]
    public float towerNearCoreRadius = 6f;
    public float cameraMoveThreshold = 0.5f;

    private TutorialStep currentStep = TutorialStep.DragToCore;
    private Vector3 cameraStartPosition;
    private bool cameraStartCaptured = false;
    private bool nectarTowerPlaced = false;
    private bool isSubscribedToPlacement = false;

    void OnEnable()
    {
        TrySubscribeToPlacementEvents();
    }

    void OnDisable()
    {
        UnsubscribeFromPlacementEvents();
    }

    void Start()
    {
        TrySubscribeToPlacementEvents();
        ShowStep(TutorialStep.DragToCore);
    }

    void Update()
    {
        if (currentStep == TutorialStep.Complete)
        {
            return;
        }

        if (core == null)
        {
            core = FindObjectOfType<CoreController>();
        }

        TrySubscribeToPlacementEvents();

        switch (currentStep)
        {
            case TutorialStep.DragToCore:
                if (GameManager.Instance != null && GameManager.Instance.GetLightCount() > 0)
                {
                    ShowStep(TutorialStep.WaitForTenLights);
                }
                break;

            case TutorialStep.WaitForTenLights:
                if (GameManager.Instance != null && GameManager.Instance.GetLightCount() >= 10)
                {
                    ShowStep(TutorialStep.PlaceNectarTower);
                }
                break;

            case TutorialStep.PlaceNectarTower:
                if (nectarTowerPlaced)
                {
                    ShowStep(TutorialStep.DragToTower);
                }
                break;

            case TutorialStep.DragToTower:
                if (IsAnyTowerOccupied())
                {
                    CaptureCameraStartIfNeeded();
                    ShowStep(TutorialStep.PanCamera);
                }
                break;

            case TutorialStep.PanCamera:
                if (HasCameraMoved())
                {
                    ShowStep(TutorialStep.Complete);
                }
                break;
        }
    }

    void ShowStep(TutorialStep nextStep)
    {
        currentStep = nextStep;

        switch (currentStep)
        {
            case TutorialStep.DragToCore:
                SetTutorialText("Drag the firefly to the purple core to generate light.");
                break;
            case TutorialStep.WaitForTenLights:
                SetTutorialText("The firefly keeps randomly moving around. Get 10 lights first.");
                break;
            case TutorialStep.PlaceNectarTower:
                SetTutorialText("Sell the light, and buy and place a nectar tower near the core to attract the flies.");
                break;
            case TutorialStep.DragToTower:
                SetTutorialText("Drag the firefly onto the nectar tower.");
                break;
            case TutorialStep.PanCamera:
                SetTutorialText("Click and drag to pan the camera.");
                CaptureCameraStartIfNeeded();
                break;
            case TutorialStep.Complete:
                SetTutorialText(string.Empty);
                break;
        }
    }

    void SetTutorialText(string message)
    {
        if (tutorialText != null)
        {
            tutorialText.text = message;
        }
    }

    void HandleObjectPlaced(GameObject placedObject)
    {
        if (currentStep != TutorialStep.PlaceNectarTower || placedObject == null)
        {
            return;
        }

        if (IsTowerNearCore(placedObject.transform))
        {
            nectarTowerPlaced = true;
        }
    }

    void TrySubscribeToPlacementEvents()
    {
        if (isSubscribedToPlacement || PlacementManager.Instance == null)
        {
            return;
        }

        PlacementManager.Instance.ObjectPlaced += HandleObjectPlaced;
        isSubscribedToPlacement = true;
    }

    void UnsubscribeFromPlacementEvents()
    {
        if (!isSubscribedToPlacement || PlacementManager.Instance == null)
        {
            return;
        }

        PlacementManager.Instance.ObjectPlaced -= HandleObjectPlaced;
        isSubscribedToPlacement = false;
    }

    bool IsTowerNearCore(Transform towerTransform)
    {
        if (towerTransform == null)
        {
            return false;
        }

        if (core == null)
        {
            core = FindObjectOfType<CoreController>();
            if (core == null)
            {
                return false;
            }
        }

        float distance = Vector3.Distance(towerTransform.position, core.transform.position);
        return distance <= towerNearCoreRadius;
    }

    bool IsAnyTowerOccupied()
    {
        PillarController[] pillars = FindObjectsOfType<PillarController>();
        foreach (var pillar in pillars)
        {
            if (pillar != null && pillar.FireflyCount > 0)
            {
                return true;
            }
        }

        return false;
    }

    void CaptureCameraStartIfNeeded()
    {
        if (cameraStartCaptured)
        {
            return;
        }

        if (Camera.main == null)
        {
            return;
        }

        cameraStartPosition = Camera.main.transform.position;
        cameraStartCaptured = true;
    }

    bool HasCameraMoved()
    {
        if (!cameraStartCaptured || Camera.main == null)
        {
            return false;
        }

        return Vector3.Distance(Camera.main.transform.position, cameraStartPosition) >= cameraMoveThreshold;
    }
}
