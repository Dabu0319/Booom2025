
using UnityEngine;

public class camera_zoom : MonoBehaviour
{
    [Header("必填参数")]
    public Transform player;
    public Transform enemy;
    public Transform mapCenter;
    public PlayerMovementController playerMovementController;
    [Header("视野控制")]
    [Tooltip("静止时的视野大小")]
    public float minSize = 5f;
    [Tooltip("最大速度时的视野大小")]
    public float maxSize = 10f;
    [Tooltip("视野缩放平滑速度"), Range(1f, 20f)]
    public float zoomSmoothness = 5f;
    [Header("速度检测")]
    [Tooltip("达到此速度才开始缩放"), Min(0.1f)]
    public float speedThreshold = 5.5f;
    [Tooltip("速度采样帧数"), Range(3, 20)]

    public int bufferSize = 10;
    // 运行时变量
    private Camera cam;
    private Vector3[] positionBuffer;
    private Vector3 cameraPosition;
    private int bufferIndex;
    private Rigidbody2D targetRb;
    private int playerDashState;
 
    void Awake()
    {
        cam = GetComponent<Camera>();
        targetRb = player.GetComponent<Rigidbody2D>();
        
        // 初始化位置缓存
        positionBuffer = new Vector3[bufferSize];
        for (int i = 0; i < bufferSize; i++)
        {
            positionBuffer[i] = player.position;
        }
    }
    void Update()
    {
        // 循环更新位置缓存
        positionBuffer[bufferIndex] = player.position;
        bufferIndex = (bufferIndex + 1) % bufferSize;
        playerDashState = playerMovementController.GetDashState();
    }
    void LateUpdate()
    {
        CalculateCameraCenter();
        // 直接使用目标位置
        transform.position = new Vector3(
            cameraPosition.x,
            cameraPosition.y,
            transform.position.z
        );
        // 计算速度并调整视野
        if(playerDashState == 1) return;
        float currentSpeed = CalculateCurrentSpeed();
        float targetSize = CalculateTargetSize(currentSpeed);
        ApplyZoom(targetSize);
    }
    float CalculateCurrentSpeed()
    {
        // 优先使用刚体速度
        if (targetRb != null) return targetRb.linearVelocity.magnitude;
        // 计算基于位置变化的平均速度
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
        if (speed <= speedThreshold) return minSize;
        
        // 线性插值计算目标大小
        float normalizedSpeed = Mathf.InverseLerp(
            speedThreshold, 
            speedThreshold * 3f, 
            Mathf.Min(speed, speedThreshold * 3f)
        );
        return Mathf.Lerp(minSize, maxSize, normalizedSpeed);
    }
    void ApplyZoom(float targetSize)
    {
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetSize,
            Time.deltaTime * zoomSmoothness
        );
    }

    private void CalculateCameraCenter(){
        cameraPosition.x = ((player.position.x + enemy.position.x)*0.5f+(1*player.position.x + 2*mapCenter.position.x)/3.0f)*0.5f;
        cameraPosition.y = ((player.position.y + enemy.position.y)*0.5f+(1*player.position.y + 2*mapCenter.position.y)/3.0f)*0.5f;
    }
}
