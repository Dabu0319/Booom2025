using UnityEngine;
public class DeathMarker2D : MonoBehaviour
{
    //保证全局唯一访问
    public static DeathMarker2D Instance;
    [Header("标记设置")]
    public GameObject deathMarkerPrefab; // 死亡标记预制体
    private Vector2? lastDeathPosition = null; // 存储上一次死亡位置
    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景不销毁
        }
        else
        {
            Destroy(gameObject); // 防止重复创建
        }
    }

    void Start()
    {
        // 游戏开始时检查是否有记录的死亡位置
        if (lastDeathPosition.HasValue)
        {
            SpawnDeathMarker(lastDeathPosition.Value);
        }
    }

    // 记录死亡位置（只在内存中存储）
    public void RecordDeathPosition(Vector2 position)
    {
        lastDeathPosition = position;
        Debug.Log($"记录死亡位置: {position}");
    }

    // 在指定位置生成标记
    private void SpawnDeathMarker(Vector2 position)
    {
        if (deathMarkerPrefab != null)
        {
            Instantiate(deathMarkerPrefab, position, Quaternion.identity);
            Debug.Log($"在 {position} 生成死亡标记");
        }
        else
        {
            Debug.LogWarning("未分配死亡标记预制体！");
        }
    }

    // 清除记录
    public void ClearDeathPosition()
    {
        lastDeathPosition = null;
    }
}
