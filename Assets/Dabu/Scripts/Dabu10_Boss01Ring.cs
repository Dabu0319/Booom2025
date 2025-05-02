using UnityEngine;

public class Dabu10_Boss01Ring : MonoBehaviour
{
    public bool isPaused = false;
    public float defaultPauseTime = 1f;
    public float bossRotateSpeed = 45f; // 每秒转角度（和 Boss 一致）

    private float pauseTimer;
    private Dabu10_Boss01Controller bossController;
    
    public bool testPause = false;

    void Start()
    {
        bossController = GetComponentInParent<Dabu10_Boss01Controller>();
    }

    void Update()
    {
        if (testPause)
        {
            testPause = false;
            Pause(2f);
        }

        if (isPaused)
        {
            // 每秒反向旋转，抵消父物体
            float delta = bossRotateSpeed * Time.deltaTime;
            transform.Rotate(0, 0, delta, Space.Self); // ★ 本地反向转

            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
                pauseTimer = defaultPauseTime;
                bossController?.TryCheckPhase2();
                Debug.Log("Check Phase 2");
            }
        }
    }

    public void Pause(float duration)
    {
        if (isPaused) return; // 防止重复触发
        isPaused = true;
        pauseTimer = duration;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Weapon") && !isPaused)
        {
            var dashState = other.transform.root.GetComponent<PlayerMovementController>().GetDashState();
            if (dashState == 2 || dashState == 3)
            {
                Pause(2f);
            }
        }


        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<player_death>())
            {
                other.GetComponent<player_death>().Die();
            }
            else
            {
                // 处理玩家死亡逻辑
                Debug.Log("Player has no death script");
            }
        }
    }
}