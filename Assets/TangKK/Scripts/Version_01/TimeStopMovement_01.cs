using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TimeStopMovement_01 : MonoBehaviour
{
    [Header("时停移动参数")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 只有在时停状态下才读取输入
        if (Time.timeScale > 0f) return;

        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void FixedUpdate()
    {
        // 时停期间，FixedUpdate 不会被调用，因此我们在 Update 中用 MovePosition
        if (Time.timeScale > 0f) return;

        if (input.sqrMagnitude > 0.01f)
        {
            Vector2 moveDelta = input * moveSpeed * Time.unscaledDeltaTime;
            rb.MovePosition(rb.position + moveDelta);
        }
    }
}