using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Dabu10_PlayerController : MonoBehaviour
{
    [Header("Input Actions Asset")]
    public InputActionAsset inputActionsAsset;

    [Header("Settings")]
    public float moveSpeed = 5f;
    public float directionLineRotateSpeed = 200f; // 控制方向线旋转速度（输入响应）
    public float rotationSpeedFactor = 10f;       // 玩家转向速度倍数（角度差越大转越快）

    [Header("References")]
    public Transform directionLine; // 世界空间下的目标朝向

    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction stopAction;

    private bool isMoving = false;
    private Vector2 moveInput;
    
    
    public Animator animator; // 动画控制器引用

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

        if (moveInput != Vector2.zero)
            isMoving = true;

        if (stopAction.WasPressedThisFrame())
        {
            isMoving = false;
            rb.linearVelocity = Vector2.zero;
        }

        // 控制方向线旋转（独立于玩家）
        if (moveInput.x != 0)
        {
            float angle = directionLine.eulerAngles.z;
            angle -= moveInput.x * directionLineRotateSpeed * Time.deltaTime;
            directionLine.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            // 当前和目标角度
            float currentAngle = transform.eulerAngles.z;
            float targetAngle = directionLine.eulerAngles.z;
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

            // 越偏差越快旋转
            float rotateAmount = Mathf.Clamp(angleDiff, -1f, 1f) * rotationSpeedFactor;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, Mathf.Abs(rotateAmount) * Time.fixedDeltaTime);

            rb.MoveRotation(newAngle);
            
            animator.enabled = true; // 启用动画控制器

            // 前进
            rb.linearVelocity = transform.up * moveSpeed;
        }
        else
        {
            
            animator.enabled = false; // 禁用动画控制器
        }
    }

    void LateUpdate()
    {
        // 让方向线跟随玩家位置，但不跟随旋转
        if (directionLine != null)
        {
            directionLine.position = transform.position;
        }
    }
}
