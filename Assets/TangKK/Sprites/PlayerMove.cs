using UnityEngine;

namespace TangKK{

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;  // 移动速度

    private Rigidbody2D rb;
    private Vector2 movementInput;

    [Header("Visual Settings")]
    public Transform characterSprite;  // 拖拽你的角色精灵到这里
    public bool faceMovementDirection = true;

    [Header("State")]
    public bool isMoving = false;  // 是否正在移动

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;  // 禁用重力
    }

    void Update()
    {
        // 获取WSAD输入
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        // 标准化向量以防斜向移动速度加快
        movementInput = movementInput.normalized;

        // 设置是否正在移动
        isMoving = movementInput != Vector2.zero;

        // 根据移动方向翻转角色朝向
        if (faceMovementDirection && movementInput.x != 0)
        {
            float scaleX = Mathf.Sign(movementInput.x);  // 获取方向(1或-1)
            characterSprite.localScale = new Vector3(scaleX, 1, 1);
        }
    }

    void FixedUpdate()
    {
        // 移动角色
        rb.linearVelocity = movementInput * moveSpeed;
    }
}
}