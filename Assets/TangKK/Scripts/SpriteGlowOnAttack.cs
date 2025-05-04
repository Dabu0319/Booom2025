using UnityEngine;

public class SpriteGlowOnAttack : MonoBehaviour
{
    public SpriteRenderer weaponSprite;
    
    [Header("发光设置")]
    public bool shouldGlow = false; // ✅ 控制是否启用高亮
    public Color glowColor = new Color(1.5f, 1.5f, 1.5f, 1f); // 比白色更亮
    public float glowDuration = 0.2f;

    private Color originalColor;
    private float timer;
    private bool isGlowing = false;

    void Start()
    {
        if (weaponSprite == null)
            weaponSprite = GetComponent<SpriteRenderer>();

        originalColor = weaponSprite.color;
    }

    void Update()
    {
        if (isGlowing)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                weaponSprite.color = originalColor;
                isGlowing = false;
            }
        }
    }

    /// <summary>
    /// 外部调用这个方法来触发高亮（如果 shouldGlow 为 true）
    /// </summary>
    public void TriggerGlow()
    {
        if (!shouldGlow) return; // ✅ 如果未开启发光，直接跳过

        if (weaponSprite == null) return;

        weaponSprite.color = glowColor;
        timer = glowDuration;
        isGlowing = true;
    }
}