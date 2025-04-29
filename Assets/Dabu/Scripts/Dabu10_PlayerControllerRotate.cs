using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Dabu10_PlayerControllerRotate : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Moving,
        TacticalDash
    }

    [Header("Input Actions Asset")]
    public InputActionAsset inputActionsAsset;

    [Header("Settings")]
    public float moveSpeed = 5f;
    public float directionLineRotateSpeed = 200f;
    public float playerTurnSpeed = 10f;

    [Header("References")]
    public Transform directionLine;
    public Animator animator;
    public LineRenderer tacticalLine; // ← 拖入引用

    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction stopAction;

    private Vector2 moveInput;
    public PlayerState currentState = PlayerState.Idle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        var map = inputActionsAsset.FindActionMap("Player");

        moveAction = map.FindAction("Move");
        stopAction = map.FindAction("Stop");

        if (tacticalLine == null)
        {
            Debug.LogWarning("Tactical LineRenderer 未设置！");
        }
    }

    void OnEnable()
    {
        moveAction.Enable();
        stopAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        stopAction.Disable();
    }

    void Update()
    {
        if (currentState == PlayerState.TacticalDash)
        {
            UpdateTacticalDash();
            return;
        }

        moveInput = moveAction.ReadValue<Vector2>();

        if (stopAction.WasPressedThisFrame())
        {
            SetState(PlayerState.Idle);
            return;
        }

        if (moveInput.x != 0)
        {
            float angle = directionLine.eulerAngles.z;
            angle -= moveInput.x * directionLineRotateSpeed * Time.deltaTime;
            directionLine.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (moveInput.y > 0.1f)
        {
            SetState(PlayerState.Moving);
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            EnterTacticalDash();
        }
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                HandleIdle();
                break;

            case PlayerState.Moving:
                HandleMovement();
                break;
        }
    }

    void LateUpdate()
    {
        if (directionLine != null)
        {
            directionLine.position = transform.position;
        }
    }

    // ----------------------
    // State Handlers
    // ----------------------

    private void HandleIdle()
    {
        rb.linearVelocity = Vector2.zero;
        animator.enabled = false;
    }

    private void HandleMovement()
    {
        float currentAngle = transform.eulerAngles.z;
        float targetAngle = directionLine.eulerAngles.z;
        float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

        float rotateAmount = Mathf.Clamp(angleDiff, -1f, 1f) * playerTurnSpeed;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, Mathf.Abs(rotateAmount) * Time.fixedDeltaTime);

        rb.MoveRotation(newAngle);
        rb.linearVelocity = transform.up * moveSpeed;
        animator.enabled = true;
    }

    // ----------------------
    // Tactical Dash
    // ----------------------

    private void EnterTacticalDash()
    {
        SetState(PlayerState.TacticalDash);
        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        if (tacticalLine != null) tacticalLine.enabled = true;
        rb.linearVelocity = Vector2.zero;
    }

    private void UpdateTacticalDash()
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (tacticalLine != null)
        {
            tacticalLine.SetPosition(0, transform.position);
            tacticalLine.SetPosition(1, mouseWorld);
        }

        // 实时旋转朝向鼠标
        Vector2 dir = (mouseWorld - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        directionLine.rotation = Quaternion.Euler(0, 0, angle);
        rb.MoveRotation(angle);

        // 鼠标点击 → 旋转并瞬移
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            rb.MoveRotation(angle);
            rb.MovePosition(mouseWorld);
            rb.linearVelocity = transform.up * moveSpeed;
        }

        // 按 F 退出
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            ExitTacticalDash();
        }
    }

    private void ExitTacticalDash()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        if (tacticalLine != null) tacticalLine.enabled = false;
        SetState(PlayerState.Moving);
    }

    // ----------------------
    // Utilities
    // ----------------------

    private void SetState(PlayerState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }
}
