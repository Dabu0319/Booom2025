using UnityEngine;

namespace TangKK
{
    public class SpearColliderManager : MonoBehaviour
    {
        [Header("✅ 开关：是否启用偏移")]
        public bool enableOffset = false;

        [Header("要滑动偏移的碰撞体 GameObject")]
        public GameObject spearColliderObject;

        [Header("角色本体（用于读取Z轴方向）")]
        public Transform characterTransform;

        [Header("角度修正（如果模型默认朝向不是X轴，建议填 -90）")]
        public float angleOffset = -90f;

        [Header("攻击时偏移距离")]
        public float attackOffsetDistance = 1.0f;

        [Header("默认局部位置")]
        public Vector3 defaultLocalPosition = Vector3.zero;

        [Header("滑动速度（影响平滑插值）")]
        public float slideSpeed = 10f;

        [Header("偏移持续时间（秒）")]
        public float offsetDuration = 0.5f;

        [Header("自定义滑动曲线（用于非线性偏移）")]
        public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private float offsetTimer = 0f;
        private PlayerAnimatorManager playerAnimatorManager;
        private Collider2D collider;
        private SpriteGlowOnAttack spriteGlowOnAttack;

        // ✅ 偏移进度（0 ~ 1）
        [Range(0f, 1f)]
        public float attackOffsetProgress = 0f;

        // ✅ 阶段标记（可用于事件触发）
        public bool phase1Triggered = false;
        public bool phase2Triggered = false;

        void Start()
        {
            collider = GetComponent<Collider2D>();
            playerAnimatorManager = GetComponentInParent<PlayerAnimatorManager>();
            spriteGlowOnAttack = GetComponentInParent<SpriteGlowOnAttack>();

            if (playerAnimatorManager == null)
                Debug.LogError("❌ 未找到 PlayerAnimatorManager，请确认它挂在父物体上");

            if (characterTransform == null)
                Debug.LogError("❌ characterTransform 未赋值，请在 Inspector 中手动拖入角色本体");
        }

        void Update()
        {
            if (spearColliderObject == null || characterTransform == null || playerAnimatorManager == null)
                return;

            Vector3 targetLocalPosition = defaultLocalPosition;

            if (enableOffset && playerAnimatorManager.isAttacking)
            {
                offsetTimer += Time.unscaledDeltaTime;
                attackOffsetProgress = Mathf.Clamp01(offsetTimer / offsetDuration);

                // ✅ 使用自定义曲线控制滑动进度
                float easedProgress = slideCurve.Evaluate(attackOffsetProgress);

                float angleZ = characterTransform.eulerAngles.z + angleOffset;
                float angleRad = angleZ * Mathf.Deg2Rad;

                Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
                float distance = attackOffsetDistance * easedProgress;
                Vector3 offset = new Vector3(dir.x, dir.y, 0f) * distance;
                Vector3 worldTargetPos = transform.TransformPoint(defaultLocalPosition + offset);
                targetLocalPosition = transform.InverseTransformPoint(worldTargetPos);

                // ✅ 时间段判断（阶段触发控制）
                if (!phase1Triggered && offsetTimer >= 0f && offsetTimer <= 0.5f)
                {
                    phase1Triggered = true;
                    Debug.Log("➡ 进入第一阶段（0.1s）");
                }

                if (!phase2Triggered && offsetTimer > 0.5f)
                {
                    phase2Triggered = true;
                    phase1Triggered = false;
                    Debug.Log("➡ 进入第二阶段（0.3s）");
                }
            }
            else
            {
                // ✅ 攻击结束，重置状态
                offsetTimer = 0f;
                attackOffsetProgress = 0f;
                phase1Triggered = false;
                phase2Triggered = false;
            }

            // ✅ 平滑移动（用于缓和突变）
            spearColliderObject.transform.localPosition = Vector3.Lerp(
                spearColliderObject.transform.localPosition,
                targetLocalPosition,
                slideSpeed * Time.unscaledDeltaTime
            );

            NormalAttack();
            HidCollider();
        }

        void OnDrawGizmos()
        {
            if (characterTransform == null)
                return;

            float angleZ = characterTransform.eulerAngles.z + angleOffset;
            float angleRad = angleZ * Mathf.Deg2Rad;

            Vector3 dir = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f).normalized;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + dir * 1.5f);
            Gizmos.DrawSphere(transform.position + dir * attackOffsetDistance, 0.05f);
        }

        public void NormalAttack()
        {
            if (playerAnimatorManager.isAttacking)
                enableOffset = true;
        }

        public void HidCollider()
        {
            if(playerAnimatorManager.isAttacking == true || playerAnimatorManager.isDash == true)
            {
                collider.enabled = true;
            }
            else
            {
                collider.enabled = false;
            }
        }
    }
}