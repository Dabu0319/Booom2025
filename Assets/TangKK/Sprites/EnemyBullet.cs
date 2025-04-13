using UnityEngine;

namespace TangKK
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(LineRenderer))]
    public class EnemyBullet : MonoBehaviour
    {
        public float speed = 5f;
        public float lifeTime = 5f;
        public Vector2 direction = Vector2.right;
        public bool deflected = false;

        [Header("Deflection Settings")]
        public Color deflectedColor = Color.red;
        public float slowEffectDuration = 1f; // 慢动作持续时间（真实时间）
        public float inputWindow = 1f;        // 输入方向的时间窗口

        private bool waitingForDirection = false;
        private TimeSlowEffect timeSlowEffect;
        private SpriteRenderer spriteRenderer;
        private LineRenderer lineRenderer;
        

        private void Awake()
        {
            timeSlowEffect = gameObject.AddComponent<TimeSlowEffect>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            lineRenderer = GetComponent<LineRenderer>();

            // 初始化 LineRenderer
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.cyan;
            lineRenderer.endColor = Color.cyan;
            lineRenderer.enabled = false;
        }

        private void Start()
        {
            Destroy(gameObject, lifeTime);

            // 配置时停参数
            timeSlowEffect.slowTimeScale = 0.05f;
            timeSlowEffect.slowDuration = slowEffectDuration;
        }

        private void Update()
        {
            if (!waitingForDirection)
            {
                transform.Translate(direction.normalized * speed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!deflected && collision.CompareTag("ParryZone"))
            {
                var playerAttack = collision.GetComponentInParent<PlayerAttack>();
                if (playerAttack != null && playerAttack.isAttack)
                {
                    StartCoroutine(ChooseDeflectDirection(playerAttack.transform));
                    return;
                }
            }

            if (!deflected && collision.CompareTag("Player"))
            {
                Debug.Log("Player hit!");
                Destroy(gameObject);
            }

            if (deflected && collision.CompareTag("Enemy"))
            {
                Debug.Log("Enemy hit by deflected bullet!");
                Destroy(gameObject);
            }
        }

        private System.Collections.IEnumerator ChooseDeflectDirection(Transform player)
        {
            deflected = true;
            waitingForDirection = true;

            // 视觉变化
            spriteRenderer.color = deflectedColor;
            gameObject.layer = LayerMask.NameToLayer("DeflectedBullet");

            // 启动时间减速
            timeSlowEffect.TriggerTimeSlow();
            Debug.Log("Choose direction to deflect (mouse)!");

            Vector2 chosenDirection = Vector2.zero;
            float timer = 0f;

            // 启用 LineRenderer 可视化射线
            lineRenderer.enabled = true;

            while (timer < inputWindow)
            {
                timer += Time.unscaledDeltaTime;

                // 获取鼠标世界坐标
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 dir = mouseWorldPos - (Vector2)transform.position;

                if (dir.sqrMagnitude > 0.1f)
                {
                    chosenDirection = dir.normalized;

                    // ✅ 更新 LineRenderer 可视化方向线
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, (Vector2)transform.position + chosenDirection * 3f); // 拉长一点
                }

                // ✅ Scene 视图中显示调试射线
                Debug.DrawLine(transform.position, mouseWorldPos, Color.cyan, 0f);

                yield return null;
            }

            // 隐藏 LineRenderer
            lineRenderer.enabled = false;

            // 设置方向（无输入则反射）
            direction = (chosenDirection == Vector2.zero) ? -direction : chosenDirection;
            waitingForDirection = false;

            Debug.Log($"Bullet deflected toward mouse direction: {direction}");
            TimeStopManager.Instance?.AddParryPoint();
        }
    }
}