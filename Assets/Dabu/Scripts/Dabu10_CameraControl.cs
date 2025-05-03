using Cinemachine;
using UnityEngine;

public class Dabu10_CameraControl : MonoBehaviour
{
    [Header("目标玩家")]
    public Transform player;
    public PlayerMovementController playerMovementController;

    [Header("缩放参数")]
    public float minSize = 5f;
    public float maxSize = 10f;
    public float zoomSmoothness = 5f;

    [Header("速度检测")]
    public float speedThreshold = 5.5f;
    public int bufferSize = 10;

    private CinemachineVirtualCamera vcam;
    private Rigidbody2D targetRb;
    private Vector3[] positionBuffer;
    private int bufferIndex;
    private int playerDashState;

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam == null)
        {
            Debug.LogError("找不到 CinemachineVirtualCamera");
        }

        targetRb = player.GetComponent<Rigidbody2D>();

        positionBuffer = new Vector3[bufferSize];
        for (int i = 0; i < bufferSize; i++)
        {
            positionBuffer[i] = player.position;
        }
    }

    void Update()
    {
        // 更新位置缓存
        positionBuffer[bufferIndex] = player.position;
        bufferIndex = (bufferIndex + 1) % bufferSize;

        // 获取当前冲刺状态（可选）
        playerDashState = playerMovementController.GetDashState();
    }

    void LateUpdate()
    {
        if (playerDashState == 1) return; // 冲刺中不调整 zoom

        float currentSpeed = CalculateCurrentSpeed();
        float targetSize = CalculateTargetSize(currentSpeed);
        ApplyZoom(targetSize);
    }

    float CalculateCurrentSpeed()
    {
        if (targetRb != null)
            return targetRb.linearVelocity.magnitude;

        // 使用位置差值估算速度（备用）
        float totalDistance = 0f;
        for (int i = 0; i < bufferSize - 1; i++)
        {
            int next = (bufferIndex + i) % bufferSize;
            int prev = (next - 1 + bufferSize) % bufferSize;
            totalDistance += Vector2.Distance(positionBuffer[next], positionBuffer[prev]);
        }
        return totalDistance / (bufferSize - 1) / Time.deltaTime;
    }

    float CalculateTargetSize(float speed)
    {
        if (speed <= speedThreshold)
            return minSize;

        float normalizedSpeed = Mathf.InverseLerp(
            speedThreshold,
            speedThreshold * 3f,
            Mathf.Min(speed, speedThreshold * 3f)
        );
        return Mathf.Lerp(minSize, maxSize, normalizedSpeed);
    }

    void ApplyZoom(float targetSize)
    {
        if (vcam != null)
        {
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(
                vcam.m_Lens.OrthographicSize,
                targetSize,
                Time.deltaTime * zoomSmoothness
            );
        }
    }
}
