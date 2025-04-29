using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]

public class Dabu10_PlayerControllerFourDir : MonoBehaviour
{
[Header("Input Actions Asset")]
    public InputActionAsset inputActionsAsset;

    [Header("Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeedFactor = 10f;       // 玩家转向速度倍数（角度差越大转越快）

    [Header("References")]
    public Transform directionLine; // 世界空间下的目标朝向

    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction stopAction;

    private bool isMoving = false;
    private Vector2 moveInput;

    public Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        var map = inputActionsAsset.FindActionMap("Player");

        moveAction = map.FindAction("Move");
        stopAction = map.FindAction("Stop");
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
        moveInput = moveAction.ReadValue<Vector2>();

        // 如果有方向输入，更新 directionLine 的朝向
        if (moveInput.sqrMagnitude > 0.01f)
        {
            isMoving = true;

            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f; // up方向为0度
            directionLine.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 停止
        if (stopAction.WasPressedThisFrame())
        {
            isMoving = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            float currentAngle = transform.eulerAngles.z;
            float targetAngle = directionLine.eulerAngles.z;
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

            // 角度差越大旋转越快
            float rotateAmount = Mathf.Clamp(angleDiff, -1f, 1f) * rotationSpeedFactor;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, Mathf.Abs(rotateAmount) * Time.fixedDeltaTime);

            rb.MoveRotation(newAngle);
            animator.enabled = true;
            rb.linearVelocity = transform.up * moveSpeed;
        }
        else
        {
            animator.enabled = false;
        }
    }

    void LateUpdate()
    {
        // 让方向线跟随玩家位置
        if (directionLine != null)
        {
            directionLine.position = transform.position;
        }
    }
}
