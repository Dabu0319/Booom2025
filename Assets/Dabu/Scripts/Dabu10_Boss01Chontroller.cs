using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Dabu10_Boss01Controller : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Dash,
        Recover
    }

    [Header("Settings")]
    public float moveSpeed = 10f;
    public float recoverDuration = 1f;
    public float rotationSpeed = 45f;

    private BossState currentState = BossState.Idle;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float recoverTimer = 0f;
    private Transform playerTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        playerTransform = GameObject.FindWithTag("Player").transform;

        EnterDash();
    }

    void Update()
    {
        transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);

        switch (currentState)
        {
            case BossState.Dash:
                // Dash阶段，不需要额外操作，靠速度移动
                break;
            case BossState.Recover:
                RecoverUpdate();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (currentState == BossState.Recover)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, 0f, 2f * Time.fixedDeltaTime);
            rb.linearVelocity = moveDirection.normalized * moveSpeed;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflectedDirection = Vector2.Reflect(moveDirection, normal).normalized;

            moveDirection = reflectedDirection;

            if (currentState == BossState.Dash)
            {
                EnterRecover();
            }
        }
    }

    private void EnterDash()
    {
        currentState = BossState.Dash;
        moveDirection = (playerTransform.position - transform.position).normalized;
        moveSpeed = 10f; 
        rb.linearVelocity = moveDirection * moveSpeed; // ★只在这里设速度
    }


    private void EnterRecover()
    {
        currentState = BossState.Recover;
        recoverTimer = recoverDuration;
    }

    private void RecoverUpdate()
    {
        recoverTimer -= Time.deltaTime;
        if (recoverTimer <= 0f)
        {
            EnterDash();
        }
    }
}
