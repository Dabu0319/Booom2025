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

    [Header("残影持续时间")]
    public float afterImageDuration = 0.3f;

    private SpriteRenderer playerRenderer;
    private Sprite lastSprite;

    void Start()
    {
        playerRenderer = GetComponent<SpriteRenderer>();
        if (playerRenderer == null)
        {
            Debug.LogError("❌ 没有找到 SpriteRenderer 组件！");
        }
    }

    void Update()
    {
        if (!enableAfterImage || afterImagePrefab == null || playerRenderer == null)
            return;

        // 每当动画帧发生变化时生成残影
        Sprite currentSprite = playerRenderer.sprite;

        if (currentSprite != null && currentSprite != lastSprite)
        {
            SpawnAfterImage(currentSprite);
            lastSprite = currentSprite;
        }
    }

    void SpawnAfterImage(Sprite sprite)
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
        cloneRenderer.color = afterImageColor;
        clone.transform.localScale = transform.localScale;

        // ✅ 正确设置渲染层级
        cloneRenderer.sortingLayerID = playerRenderer.sortingLayerID;
        cloneRenderer.sortingOrder = playerRenderer.sortingOrder - 1;

        // 残影渐隐销毁
        StartCoroutine(FadeAndDestroy(cloneRenderer, afterImageDuration));
    }

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
}
}