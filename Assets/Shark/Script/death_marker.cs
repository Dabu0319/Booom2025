using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMarker2D : MonoBehaviour
{
    //保证全局唯一访问
    public static DeathMarker2D Instance;
    [Header("标记设置")]
    public GameObject deathMarkerPrefab; // 死亡标记预制体
    private Vector2 lastDeathPosition; // 存储上一次死亡位置
    public bool hasPosition = false;


    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景不销毁
            SceneManager.sceneLoaded += OnSceneLoaded; // 监听场景加载事件
        }
        else
        {
            Destroy(gameObject); // 防止重复创建
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(hasPosition){
            SpawnDeathMarker(lastDeathPosition);
        }


    }


    // 记录死亡位置（只在内存中存储）
    public void RecordDeathPosition(Vector2 position)
    {
        lastDeathPosition = position;
        Debug.Log($"记录死亡位置: {position}");
        hasPosition = true;
        
    }

    // 在指定位置生成标记
    public void SpawnDeathMarker(Vector2 position)
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

    public Vector2 getDeathPosition(){
        return lastDeathPosition;
    }

    // 清除记录

}
