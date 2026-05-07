using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(PlayerInput)),
    RequireComponent(typeof(CapsuleCollider)), RequireComponent(typeof(PlayerData))]
public class PlayerMovement : MonoBehaviour, IPlayerStateProvider
{
    [Header("Movement")]
    public float runSpeed = 12f;
    public float walkSpeed = 8f;
    public float crouchSpeed = 5f;
    public float proneSpeed = 3f;

    [Tooltip("Скорость перемещения по лестнице")]
    public float ladderSpeed = 3f;
    public float airControl = 0.4f;

    [Header("Jumping")]
    public float jumpForce = 8f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private float groundCheckHeight = 0.1f;
    
    [Header("Crouch & Prone")]
    public PlayerPose currentPose = PlayerPose.Stand;
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public float proneHeight = 0.5f;
    public float poseChangeSpeed = 5f;

    private const float poseCheckOffset = 0.1f;
    private const float colliderRadiusMultiplier = 0.9f;

    private Rigidbody rb;
    private PlayerInput input;
    private PlayerData playerData;
    private LadderZone currentLadderZone;
    private CapsuleCollider capsuleCollider;
    private SurfaceInteractor surfaceInteractor;

    private bool isGrounded;
    private bool isOnLadder;
    private float targetHeight;
    private float targetCenterY;
    private float groundHeightControl;

    public Vector3 currentSurfaceNormal = Vector3.up;
    private Vector3 moveDirection;


    #region Start, Update & etc
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        surfaceInteractor = GetComponent<SurfaceInteractor>();
        playerData = GetComponent<PlayerData>();
    }

    void Start() 
    {
        SetPose(PlayerPose.Stand);
    }

    void Update()
    {
        UpdateGroundCheck();
        HandleInput();
    }

    void FixedUpdate()
    {
        UpdatePose();
        UpdateMovement();
    }
    #endregion

    #region Input
    private void HandleInput()
    {
        HandlePoseInput();
        HandleLadderInput();
        HandleJumpInput();
    }

    private void HandlePoseInput()
    {
        if (!isGrounded || isOnLadder)
        {
            if (currentPose != PlayerPose.Stand)
                SetPose(PlayerPose.Stand);
            return;
        }

        if (input.altCrouchHeld)
        {
            SetPose(PlayerPose.AltCrouch);
            return;
        }

        if (input.pronePressed)
        {
            TogglePose(PlayerPose.Prone);
            return;
        }

        if (input.crouchPressed)
        {
            TogglePose(PlayerPose.Crouch);
            return;
        }

        UpdateAutoPose();
    }

    void HandleLadderInput()
    {
        if (!input.interactPressed) return;

        if (!isOnLadder && currentLadderZone != null)
            EnterLadder();
        else if (isOnLadder)
            ExitLadder();
    }

    void HandleJumpInput()
    {
        if (!input.jumpPressed || isOnLadder) return;

        if (isGrounded)
        {
            switch (currentPose)
            {
                case PlayerPose.Stand:
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    break;
                case PlayerPose.Crouch:
                    SetPose(PlayerPose.Stand);
                    break;
                case PlayerPose.Prone:
                    SetPose(PlayerPose.Crouch);
                    break;
                default: break;
            }
        }
    }
    #endregion

    #region Movement
    void UpdateMovement()
    {
        if (isOnLadder)
        {
            HandleLadderMovement();
            return;
        }

        CalculateMoveDirection();
        ApplyMovement();
    }

    private void CalculateMoveDirection()
    {
        currentSurfaceNormal = surfaceInteractor?.GetNormalOfSurface() ?? Vector3.up;

        if (currentSurfaceNormal != Vector3.up) rb.AddForce(Vector3.down * 2f, ForceMode.Acceleration);

        Vector2 rawInput = input.moveInput;
        if (rawInput.magnitude < 0.1f)
        {
            moveDirection = Vector3.zero;
            return;
        }

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, currentSurfaceNormal).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, currentSurfaceNormal).normalized;

        Vector3 direction = right * rawInput.x + forward * rawInput.y;
        moveDirection = direction.normalized;
    }

    private void ApplyMovement()
    {
        if (moveDirection.magnitude < 0.1f)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        float targetSpeed = GetTargetSpeed();
        float currentSpeed = isGrounded ? targetSpeed : targetSpeed * airControl;

        Vector3 targetVelocity = moveDirection * currentSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    private float GetTargetSpeed()
    {
        bool canRun = input.isRunning && input.moveInput.y > 0 && currentPose == PlayerPose.Stand;

        return currentPose switch
        {
            PlayerPose.Stand => canRun ? runSpeed : walkSpeed,
            PlayerPose.Crouch => crouchSpeed,
            PlayerPose.AltCrouch => crouchSpeed,
            PlayerPose.Prone => proneSpeed,
            _ => walkSpeed
        };
    }
    #endregion

    #region PoseManager
    private void TogglePose(PlayerPose targetPose)
    {
        if (currentPose == targetPose)
        {
            if (targetPose == PlayerPose.Prone && CanChangePoseTo(PlayerPose.Stand))
                SetPose(PlayerPose.Stand);
            else if (targetPose == PlayerPose.Prone && CanChangePoseTo(PlayerPose.Crouch))
                SetPose(PlayerPose.Crouch);
            else if ((targetPose == PlayerPose.Crouch || targetPose == PlayerPose.AltCrouch) && CanChangePoseTo(PlayerPose.Stand))
                SetPose(PlayerPose.Stand);
        }
        else if (CanChangePoseTo(targetPose))
        {
            SetPose(targetPose);
        }
    }

    private void UpdateAutoPose()
    {
        if (currentPose == PlayerPose.AltCrouch && CanChangePoseTo(PlayerPose.Stand))
            SetPose(PlayerPose.Stand);
    }

    private bool CanChangePoseTo(PlayerPose targetPose)
    {
        if (!isGrounded) return true;

        float fromHeight = GetPoseHeight(currentPose);
        float toHeight = GetPoseHeight(targetPose);

        if (Mathf.Approximately(fromHeight, toHeight)) return true;

        float checkHeight = Mathf.Abs(toHeight - fromHeight) + poseCheckOffset;
        Vector3 rayStart = transform.position + Vector3.up * fromHeight;

        return !Physics.SphereCast(
            rayStart,
            capsuleCollider.radius * colliderRadiusMultiplier,
            Vector3.up,
            out _,
            checkHeight,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private void UpdatePose()
    {
        (targetHeight, targetCenterY) = GetPoseDimensions(currentPose);

        if (Mathf.Abs(capsuleCollider.height - targetHeight) > 0.01f ||
            Mathf.Abs(capsuleCollider.center.y - targetCenterY) > 0.01f)
        {
            rb.AddForce(Vector3.down * 1f, ForceMode.Impulse);
            SmoothAdjustCollider();
        }
    }

    private void SmoothAdjustCollider()
    {
        float newHeight = Mathf.Lerp(capsuleCollider.height, targetHeight,
            poseChangeSpeed * Time.fixedDeltaTime);

        Vector3 center = capsuleCollider.center;
        float newCenterY = Mathf.Lerp(center.y, targetCenterY,
            poseChangeSpeed * Time.fixedDeltaTime);

        float deltaCenter = center.y - newCenterY;
        rb.position -= Vector3.up * deltaCenter;

        capsuleCollider.height = newHeight;
        capsuleCollider.center = new Vector3(center.x, newCenterY, center.z);
    }

    private (float height, float centerY) GetPoseDimensions(PlayerPose pose)
    {
        return pose switch
        {
            PlayerPose.Stand => (standHeight, 0f),
            PlayerPose.Crouch => (crouchHeight, 0.275f),
            PlayerPose.AltCrouch => (crouchHeight, 0.275f),
            PlayerPose.Prone => (proneHeight, 0.5f),
            _ => (standHeight, 0f)
        };
    }

    private float GetPoseHeight(PlayerPose pose)
    {
        return pose switch
        {
            PlayerPose.Stand => standHeight,
            PlayerPose.Crouch => crouchHeight,
            PlayerPose.AltCrouch => crouchHeight,
            PlayerPose.Prone => proneHeight,
            _ => standHeight
        };
    }

    public void SetPose(PlayerPose pose) => currentPose = pose;
    #endregion

    #region Ladder
    public void EnterLadderZone(LadderZone zone) => currentLadderZone = zone;

    void EnterLadder()
    {
        if (currentLadderZone == null) return;

        // Проверяем, может ли игрок войти на лестницу из текущей позиции
        // Учитываем не только дистанцию, но и направление взгляда игрока
        if (!currentLadderZone.CanEnterFromPosition(transform.position, transform.forward))
        {
            Debug.LogWarning("[PlayerMovement] Игрок слишком далеко от точки входа на лестницу или смотрит в другую сторону");
            return;
        }

        SetPose(PlayerPose.Stand);
        isOnLadder = true;
        rb.useGravity = false;
        ToggleWeaponModels(false);

        rb.linearVelocity = Vector3.zero;
        
        // Плавное перемещение в точку входа
        Vector3 enterPos = currentLadderZone.GetEnterPosition();
        transform.position = enterPos;

        // Поворот игрока в направлении взгляда на лестнице
        Vector3 lookDir = currentLadderZone.GetClimbLookDirection();
        transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
    }

    void HandleLadderMovement()
    {
        if (currentLadderZone == null) return;

        Vector3 bottom = currentLadderZone.GetEnterPosition();
        Vector3 top = currentLadderZone.GetExitPosition();
        
        Vector3 ladderDir = (top - bottom).normalized;
        float ladderLength = Vector3.Distance(bottom, top);

        if (ladderLength < 0.1f)
        {
            ExitLadder();
            return;
        }

        float verticalInput = input.moveInput.y;
        Vector3 pos = transform.position;

        float currentDist = Vector3.Dot(pos - bottom, ladderDir);
        float targetDist = currentDist + verticalInput * ladderSpeed * Time.fixedDeltaTime;
        float clampedDist = Mathf.Clamp(targetDist, 0f, ladderLength);

        // Позиция вдоль лестницы
        Vector3 newPos = bottom + ladderDir * clampedDist;
        
        // Небольшое смещение к лестнице для лучшего визуала
        Vector3 lookDir = currentLadderZone.GetClimbLookDirection();
        newPos += lookDir * 0.3f;
        
        transform.position = newPos;
        rb.linearVelocity = Vector3.zero;

        HandleLadderExitConditions(clampedDist, verticalInput, ladderLength, top, lookDir);
    }

    private void HandleLadderExitConditions(float clampedDist, float verticalInput,
        float ladderLength, Vector3 top, Vector3 lookDir)
    {
        // Выход снизу
        if (clampedDist <= 0.01f && verticalInput < 0f)
        {
            // Смещаем игрока назад от лестницы при выходе
            transform.position = currentLadderZone.GetEnterPosition() - lookDir * 0.8f;
            ExitLadder();
            return;
        }

        // Выход сверху
        if (clampedDist >= ladderLength - 0.01f && verticalInput > 0f)
        {
            // Плавный выход на верхнюю платформу
            transform.position = top - lookDir * 0.5f + Vector3.up * 0.1f;
            ExitLadder();
        }
    }

    public void ExitLadderZone(LadderZone zone)
    {
        if (currentLadderZone == zone)
        {
            if (isOnLadder) ExitLadder();
            currentLadderZone = null;
        }
    }

    void ExitLadder()
    {
        isOnLadder = false;
        rb.useGravity = true;
        ToggleWeaponModels(true);
    }

    private void ToggleWeaponModels(bool active) //true - оружие, false - лестница
    {
        playerData.gunContainer.SetActive(active);
        playerData.ladderArms.SetActive(!active);
    }
    #endregion

    #region Utility
    private void UpdateGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (groundHeightControl == capsuleCollider.height) return;
        float currentHeight = capsuleCollider.height;
        float currentCenterY = capsuleCollider.center.y;
        float bottomY = currentCenterY - currentHeight / 2;

        float groundCheckY = bottomY - groundCheckHeight;
        groundHeightControl = capsuleCollider.height;
        groundCheck.localPosition = new Vector3(0f, groundCheckY, 0f);
    }

    public bool IsGrounded() => isGrounded;
    public bool IsOnLadder() => isOnLadder;
    public PlayerPose GetCurrentPose() => currentPose;
    public bool IsMoving() => input.moveInput.magnitude > 0.1f;
    public bool IsRunning() => input.isRunning && input.moveInput.magnitude > 0.1f && currentPose == PlayerPose.Stand;
    #endregion

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}