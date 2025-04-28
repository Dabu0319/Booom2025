using UnityEngine;

// Boss01Ring.cs
public class Dabu10_Boss01Ring : MonoBehaviour
{
    public bool isPaused = false;
    public float defaultPauseTime = 1f; // 默认暂停时间
    private float pauseTimer;
    private Quaternion lastRotation;
    private Vector3 lastPosition;

    private Transform bossRoot;

    void Start()
    {
        bossRoot = transform.parent;
        lastRotation = transform.rotation;
        lastPosition = transform.position;
        pauseTimer = defaultPauseTime; // 初始化暂停计时器
    }

    void Update()
    {
        if (isPaused)
        {
            // 保持自己位置和旋转角度不变
            transform.position = lastPosition;
            transform.rotation = lastRotation;

            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
                pauseTimer = defaultPauseTime; // 重置暂停计时器
            }
        }
        else
        {
            // 正常跟随父物体旋转
            lastRotation = transform.rotation;
            lastPosition = transform.position;
        }
    }

    public void Pause(float duration)
    {
        isPaused = true;
        pauseTimer = duration;
    }

    // 检测被攻击
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            Pause(2f); // 举例：暂停2秒
        }
    }
}