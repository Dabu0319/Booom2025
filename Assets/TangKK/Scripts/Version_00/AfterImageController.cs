using System.Collections;
using UnityEngine;

namespace TangKK{
public class AfterImageController : MonoBehaviour
{
    [Header("✅ 残影开关（外部控制）")]
    public bool enableAfterImage = false;

    [Header("残影预设体（需要有 SpriteRenderer）")]
    public GameObject afterImagePrefab;

    [Header("残影颜色（带透明度）")]
    public Color afterImageColor = new Color(1f, 1f, 1f, 0.5f);

    [Header("🔥 时停期间残影颜色")]
    public Color timeStopAfterImageColor = new Color(0f, 1f, 1f, 0.7f); // 青色

    [Header("残影持续时间")]
    public float afterImageDuration = 0.3f;

    [Header("🔥 时停期间残影持续时间")]
    public float timeStopAfterImageDuration = 0.5f;

    [Header("🔥 时停残影生成间隔")]
    public float timeStopSpawnInterval = 0.1f; // 时停期间残影生成的时间间隔

    private SpriteRenderer playerRenderer;
    private Sprite lastSprite;
    private PlayerAnimatorManager_01 playerAnimatorManager;
    private PerfectAttack_01 perfectAttack;

    // 🔥 时停残影相关变量
    private float timeStopSpawnTimer = 0f;
    private Vector3 lastTimeStopPosition;

    void Start()
    {
        playerAnimatorManager = GetComponent<PlayerAnimatorManager_01>();
        
        // 🔥 修复：在父对象和所有子对象中查找 PerfectAttack_01
        perfectAttack = GetComponent<PerfectAttack_01>();
        if (perfectAttack == null)
        {
            perfectAttack = GetComponentInChildren<PerfectAttack_01>();
        }
        if (perfectAttack == null)
        {
            perfectAttack = GetComponentInParent<PerfectAttack_01>();
        }
        
        playerRenderer = GetComponent<SpriteRenderer>();
        if (playerRenderer == null)
        {
            Debug.LogError("❌ 没有找到 SpriteRenderer 组件！");
        }

        if (perfectAttack == null)
        {
            Debug.LogError("❌ 没有找到 PerfectAttack_01 组件！请确保它在当前对象、子对象或父对象中。");
        }
        else
        {
            Debug.Log($"✅ 找到 PerfectAttack_01 组件：{perfectAttack.gameObject.name}");
        }

        lastTimeStopPosition = transform.position;
    }

    void Update()
    {
        UpdateAfterImageState();
        
        if (!enableAfterImage || afterImagePrefab == null || playerRenderer == null)
            return;

        // 🔥 根据不同状态使用不同的残影生成逻辑
        if (IsInTimeStop())
        {
            HandleTimeStopAfterImage();
        }
        else
        {
            HandleNormalAfterImage();
        }
    }

    // 🔥 新增：更新残影状态的逻辑
    void UpdateAfterImageState()
    {
        // 原有的冲刺残影逻辑
        bool shouldEnableForDash = playerAnimatorManager.isDash;
        
        // 🔥 新增：时停期间的残影逻辑
        bool shouldEnableForTimeStop = IsInTimeStop() && IsPlayerMovingInTimeStop();
        
        enableAfterImage = shouldEnableForDash || shouldEnableForTimeStop;
    }

    // 🔥 新增：检查是否在时停状态
    bool IsInTimeStop()
    {
        return perfectAttack != null && perfectAttack.isFreezing;
    }

    // 🔥 新增：检查玩家在时停期间是否在移动
    bool IsPlayerMovingInTimeStop()
    {
        if (!IsInTimeStop()) return false;

        // 检查位置变化
        float distanceMoved = Vector3.Distance(transform.position, lastTimeStopPosition);
        bool isMoving = distanceMoved > 0.01f; // 移动阈值

        return isMoving;
    }

    // 🔥 新增：处理普通状态下的残影
    void HandleNormalAfterImage()
    {
        // 每当动画帧发生变化时生成残影
        Sprite currentSprite = playerRenderer.sprite;

        if (currentSprite != null && currentSprite != lastSprite)
        {
            SpawnAfterImage(currentSprite, afterImageColor, afterImageDuration);
            lastSprite = currentSprite;
        }
    }

    // 🔥 新增：处理时停期间的残影
    void HandleTimeStopAfterImage()
    {
        // 使用 unscaledDeltaTime 确保在时停期间正常工作
        timeStopSpawnTimer += Time.unscaledDeltaTime;

        // 检查是否应该生成残影
        if (timeStopSpawnTimer >= timeStopSpawnInterval && IsPlayerMovingInTimeStop())
        {
            Sprite currentSprite = playerRenderer.sprite;
            if (currentSprite != null)
            {
                SpawnAfterImage(currentSprite, timeStopAfterImageColor, timeStopAfterImageDuration);
            }
            
            timeStopSpawnTimer = 0f;
            lastTimeStopPosition = transform.position;
        }
        else if (!IsPlayerMovingInTimeStop())
        {
            // 如果玩家停止移动，更新位置但不重置计时器
            lastTimeStopPosition = transform.position;
        }
    }

    // 🔥 修改：支持自定义颜色和持续时间的残影生成
    void SpawnAfterImage(Sprite sprite, Color color, float duration)
    {
        GameObject clone = Instantiate(afterImagePrefab, transform.position, transform.rotation);
        SpriteRenderer cloneRenderer = clone.GetComponent<SpriteRenderer>();

        if (cloneRenderer == null)
        {
            Debug.LogWarning("❌ 残影预设体上缺少 SpriteRenderer！");
            Destroy(clone);
            return;
        }

        // 设置残影样式
        cloneRenderer.sprite = sprite;
        cloneRenderer.flipX = playerRenderer.flipX;
        cloneRenderer.color = color;
        clone.transform.localScale = transform.localScale;

        // ✅ 正确设置渲染层级
        cloneRenderer.sortingLayerID = playerRenderer.sortingLayerID;
        cloneRenderer.sortingOrder = playerRenderer.sortingOrder - 1;

        // 🔥 在时停期间使用特殊的渐隐逻辑
        if (IsInTimeStop())
        {
            StartCoroutine(FadeAndDestroyTimeStop(cloneRenderer, duration));
        }
        else
        {
            StartCoroutine(FadeAndDestroy(cloneRenderer, duration));
        }
    }

    // 🔥 原有的渐隐协程（用于普通状态）
    IEnumerator FadeAndDestroy(SpriteRenderer sr, float duration)
    {
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            sr.color = c;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(sr.gameObject);
    }

    // 🔥 新增：时停期间的渐隐协程（使用 unscaledDeltaTime）
    IEnumerator FadeAndDestroyTimeStop(SpriteRenderer sr, float duration)
    {
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            sr.color = c;

            elapsed += Time.unscaledDeltaTime; // 🔥 关键：使用 unscaledDeltaTime
            yield return null;
        }

        Destroy(sr.gameObject);
    }

    // 🔥 保留原有的方法名以保持兼容性（现在内部调用 UpdateAfterImageState）
    public void OpenAfterimage()
    {
        // 这个方法现在由 UpdateAfterImageState 处理，保留以防其他脚本调用
        UpdateAfterImageState();
    }

    // 🔥 新增：外部接口 - 强制启用时停残影
    public void ForceEnableTimeStopAfterImage(bool enable)
    {
        if (enable && IsInTimeStop())
        {
            enableAfterImage = true;
            Debug.Log("[AfterImage] 🔥 强制启用时停残影");
        }
    }

    // 🔥 新增：外部接口 - 设置时停残影颜色
    public void SetTimeStopAfterImageColor(Color color)
    {
        timeStopAfterImageColor = color;
    }

    // 🔥 新增：外部接口 - 设置时停残影生成间隔
    public void SetTimeStopSpawnInterval(float interval)
    {
        timeStopSpawnInterval = Mathf.Max(0.01f, interval);
    }

    // 🔥 新增：调试信息显示
    void OnGUI()
    {
        if (!IsInTimeStop()) return;

        GUI.Label(new Rect(10, 200, 300, 20), "=== 时停残影调试 ===");
        GUI.Label(new Rect(10, 220, 300, 20), $"残影启用: {enableAfterImage}");
        GUI.Label(new Rect(10, 240, 300, 20), $"玩家移动: {IsPlayerMovingInTimeStop()}");
        GUI.Label(new Rect(10, 260, 300, 20), $"生成计时: {timeStopSpawnTimer:F2}s");
        GUI.Label(new Rect(10, 280, 300, 20), $"生成间隔: {timeStopSpawnInterval:F2}s");
    }
}
}