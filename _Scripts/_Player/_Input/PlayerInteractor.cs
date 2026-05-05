using LightSide;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private string defaultPrompt = "Нажмите [F] чтобы подобрать";

    [SerializeField] private int raycastFrameInterval = 2;
    [SerializeField] private float raycastSphereRadius = 0.1f;

    PlayerData playerData;
    UniText interactionHint;
    Camera playerCamera;
    PlayerInput playerInput;
    WeaponInventory weaponInventory;

    private bool isHintActive;
    private int frameCounter;
    private float holdTimer = 0f;
    private const float holdDuration = 0.3f;
    private IInteractable currentTarget = null;

    private Ray ray;
    private RaycastHit[] raycastHits = new RaycastHit[8];
    private Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0);

    #region Start, Update & etc
    private void Awake()
    {
        playerData = GetComponentInParent<PlayerData>();
        playerInput = playerData.playerInput;
        playerCamera = playerData.playerCamera;
        interactionHint = playerData.interactHint;
        weaponInventory = playerData.weaponInventory;
    }

    private void Start()
    {
        if (interactionHint != null)
            interactionHint.gameObject.SetActive(false);

        isHintActive = false;
    }

    void Update()
    {
        frameCounter++;
        if (frameCounter % raycastFrameInterval == 0)
            PerformRaycast();

        HandleInput();
        UpdateHoldTimer();
    }

    private void LateUpdate()
    {
        if (frameCounter >= 1024)
            frameCounter = 0;
    }
    #endregion

    #region Raycast & Target
    void PerformRaycast()
    {
        ray = playerCamera.ViewportPointToRay(viewportCenter);

        int hitCount = Physics.SphereCastNonAlloc(
            ray,
            raycastSphereRadius,
            raycastHits,
            interactionDistance,
            interactableLayers
        );

        IInteractable newTarget = FindNearestInteractable(hitCount);

        if (ReferenceEquals(currentTarget, newTarget)) return;

        UpdateTarget(newTarget);
    }

    IInteractable FindNearestInteractable(int hitCount)
    {
        IInteractable nearestInteractable = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            ref RaycastHit hit = ref raycastHits[i];

            if (!hit.collider) continue;

            var interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable == null) continue;
            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearestInteractable = interactable;
            }
        }

        return nearestInteractable;
    }

    void UpdateTarget(IInteractable newTarget)
    {
        if (currentTarget != null)
            HideHint();

        currentTarget = newTarget;
        if (currentTarget != null)
            ShowHint(currentTarget.GetInteractionPrompt());
    }
    #endregion

    #region Input
    void HandleInput()
    {
        HandleWeaponSwitching();
        HandleInteraction();
    }

    void HandleWeaponSwitching()
    {
        if (playerInput.weaponSlot1Pressed) 
            SwitchWeapon(0);
        else if (playerInput.weaponSlot2Pressed) 
            SwitchWeapon(1);
    }

    void HandleInteraction()
    {
        if (playerInput.interactPressed && currentTarget != null)
            PerformInteraction();
    }

    void UpdateHoldTimer()
    {
        if (currentTarget is not WeaponPickup weaponPickup)
        {
            holdTimer = 0f;
            return;
        }

        if (playerInput.inspectOrChangeHold)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdDuration)
            {
                weaponPickup.OnHoldInteract(this);
                holdTimer = 0f;
            }
        }
        else holdTimer = 0f;
    }

    void PerformInteraction()
    {
        currentTarget.OnInteract(this);
        HideHint();
        currentTarget = null;
    }

    void SwitchWeapon(int slotIndex) => weaponInventory?.SwitchToSlot(slotIndex);
    #endregion

    #region Hint
    void ShowHint(string text)
    {
        if (interactionHint == null) return;

        if (interactionHint.Text != text)
            interactionHint.Text = string.IsNullOrEmpty(text) ? defaultPrompt : text;

        if (!isHintActive)
        {
            interactionHint.gameObject.SetActive(true);
            isHintActive = true;
        }
    }

    void HideHint()
    {
        if (interactionHint == null || !isHintActive) return;
        
        interactionHint.gameObject.SetActive(false);
        isHintActive = false;
    }
    #endregion

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(viewportCenter);
        Vector3 end = ray.origin + ray.direction * interactionDistance;

        Gizmos.color = currentTarget != null ? Color.green : Color.red;
        Gizmos.DrawLine(ray.origin, end);
        Gizmos.DrawWireSphere(end, 0.1f);
    }
}