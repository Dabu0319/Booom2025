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

    public BossState currentState = BossState.Idle;

    [Header("Settings")]
    public float dashForce = 10f;        // 冲刺推力
    public float bounceForce = 5f;       // 碰撞时弹开的力度
    public float recoverDuration = 1f;   // Recover状态持续时间

    private float recoverTimer = 0f;
    private Rigidbody2D rb;
    private Transform playerTransform;

    private Vector2 dashDirection; // 记录冲刺方向

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTransform = GameObject.FindWithTag("Player").transform;
        EnterDash(); // 开局直接冲刺
    }

    void Update()
    {
        switch (currentState)
        {
            case BossState.Dash:
                DashUpdate();
                break;
            case BossState.Recover:
                RecoverUpdate();
                break;
        }
    }

    void DashUpdate()
    {
        rb.AddForce(dashDirection * dashForce);
    }

    void RecoverUpdate()
    {
        recoverTimer -= Time.deltaTime;
        if (recoverTimer <= 0f)
        {
            EnterDash();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            if (currentState == BossState.Dash)
            {
                Vector2 normal = collision.contacts[0].normal;
                rb.AddForce(normal * bounceForce, ForceMode2D.Impulse);
                EnterRecover();
            }
        }
    }

    private void EnterDash()
    {
        currentState = BossState.Dash;
        dashDirection = (playerTransform.position - transform.position).normalized; // 只记录一次
    }

    private void EnterRecover()
    {
        currentState = BossState.Recover;
        recoverTimer = recoverDuration;
    }
}