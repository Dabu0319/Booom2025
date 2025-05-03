using UnityEngine;

public class sprite_fade_by_distance : MonoBehaviour
{
    [Header("透明化设置")]
    public Transform player;            // 玩家角色的Transform
    public float minDistance = 3f;     // 完全不透明的最小距离
    public float maxDistance = 10f;    // 开始透明化的最大距离
    [Range(0, 1)]
    public float minAlpha = 0.3f;      // 最低透明度 (0=完全透明，1=完全不透明)
    private SpriteRenderer spriteRenderer;

    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer未找到！");
        }
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("玩家对象未找到！");
            }
        }
    }
    void Update()
    {
        if (player == null || spriteRenderer == null) return;
        // 计算距离
        float distance = Vector3.Distance(player.position, transform.position);
        
        // 计算透明度 (从minAlpha到1之间变化)
        float alpha;
        if (distance <= minDistance)
        {
            alpha = 1f; // 在不透明距离内完全不透明
        }
        else if (distance >= maxDistance)
        {
            alpha = minAlpha; // 在最远距离时保持最低透明度
        }
        else
        {
            // 介于minDistance和maxDistance之间时插值计算
            float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
            alpha = Mathf.Lerp(1f, minAlpha, t);
        }
        // 平滑过渡（可选）
        float currentAlpha = spriteRenderer.color.a;
        float smoothAlpha = Mathf.Lerp(currentAlpha, alpha, Time.deltaTime * 5f);
        // 设置透明度
        Color color = spriteRenderer.color;
        color.a = smoothAlpha;
        spriteRenderer.color = color;
    }
}
