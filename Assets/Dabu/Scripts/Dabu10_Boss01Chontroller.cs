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
    
    public enum BossPhase
    {
        Phase1,
        Phase2
    }

    [Header("Settings")]
    public float moveSpeed = 10f;
    public float recoverDuration = 1f;
    public float rotationSpeed = 45f;

    [SerializeField]
    private BossState currentState = BossState.Idle;
    [SerializeField]
    private BossPhase currentPhase = BossPhase.Phase1;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float recoverTimer = 0f;
    private Transform playerTransform;
    
    public Dabu10_Boss01Ring[] rings; 
    public Dabu10_Boss01Core core; 
    public float currentRingAlignedAngle = 0f;

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
    
    public void TryCheckPhase2()
    {
        if (currentPhase == BossPhase.Phase2) return;
        if (rings == null || rings.Length != 3) return;

        // if (!rings[0].isPaused || !rings[1].isPaused || !rings[2].isPaused)
        //     return;
        
        Debug.Log("Checking Phase 2 in Boss01Controller");

        float tolerance = 1f;

        float angle0 = rings[0].transform.eulerAngles.z;
        float angle1 = rings[1].transform.eulerAngles.z;
        float angle2 = rings[2].transform.eulerAngles.z;

        if (Mathf.Abs(Mathf.DeltaAngle(angle0, angle1)) < tolerance &&
            Mathf.Abs(Mathf.DeltaAngle(angle0, angle2)) < tolerance)
        {
            SwitchToPhase2();
            currentRingAlignedAngle = angle0;
        }
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
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            SwitchToPhase2();
        }
    }
    
    
    private void SwitchToPhase2()
    {
        currentPhase = BossPhase.Phase2;
        Debug.Log("Boss 切换到 Phase 2");
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
        if (collision.collider.CompareTag("Enemy") )
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

        if (currentPhase == BossPhase.Phase2)
        {
            FireLaser(); // ★ Phase2 特有行为
        }
    }

    private void FireLaser()
    {
        core.FireSkill(currentRingAlignedAngle);
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
