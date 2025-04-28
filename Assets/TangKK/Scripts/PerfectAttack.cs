using UnityEngine;
using shark;
using System.Collections;

namespace TangKK
{
    public class PerfectAttack : MonoBehaviour
    {
        [Header("Freeze Settings")]
        [SerializeField] private float freezeDuration = 3f;
        [SerializeField] private PlayerMovementController playerMovementController;
        [SerializeField] private float ultimateDashThreshold = 0.3f;
        [SerializeField] private float speedPreserveRatio = 0.7f; // 原速度保留比例

        private bool isFreezing = false;
        private Vector3 frozenPosition;
        private Vector2 frozenVelocity;
        private Vector2 frozenDirection; // 新增：记录时停前方向
        private float frozenBaseSpeed;    // 新增：记录时停前基础速度
        private bool hasTriggeredUltimate = false;
        private Coroutine freezeCoroutine;

        private PlayerAnimatorManager playerAnimatorManager;

        private void Start()
        {
            playerAnimatorManager = GetComponentInParent<PlayerAnimatorManager>();
        }


        private void Update()
        {
            if (isFreezing)
            {
                HandleInputDuringFreeze();
                transform.position = frozenPosition;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy") && !isFreezing)
            {
                freezeCoroutine = StartCoroutine(FreezeTime());
            }
        }

        private IEnumerator FreezeTime()
        {
            // 记录所有关键状态
            isFreezing = true;
            frozenPosition = transform.position;
            frozenVelocity = playerMovementController.GetComponent<Rigidbody2D>().linearVelocity;
            frozenDirection = playerMovementController.GetDirection();
            frozenBaseSpeed = playerMovementController.GetSpeed();
            
            // 冻结物理状态
            playerMovementController.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            playerMovementController.SetExtraSpeed(0);
            Time.timeScale = 0f;

            // 重置冲刺状态
            hasTriggeredUltimate = false;
            playerMovementController.LockDirection(false);
            playerMovementController.SetSpaceLock(false);

            Debug.Log($"[时停] 启动 | 原速度: {frozenVelocity} | 方向: {frozenDirection}");

            yield return new WaitForSecondsRealtime(freezeDuration);

            if (isFreezing)
            {
                Debug.Log("[时停] 超时自动恢复");
                ResumeTime(false);
            }
        }

        private void HandleInputDuringFreeze()
        {
            // 方向输入处理
            Vector2 inputDirection = new Vector2(
                Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0,
                Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0
            ).normalized;

            if (inputDirection != Vector2.zero)
            {
                playerMovementController.SetDirection(inputDirection);
                playerMovementController.SetDashDirection(inputDirection);
            }

            // 空格键状态检测
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerMovementController.SetSpaceLock(true);
                Debug.Log("[时停] 空格键按下");
            }

            // 长按触发极限冲刺
            if (Input.GetKey(KeyCode.Space) && !hasTriggeredUltimate)
            {
                if (playerMovementController.GetSpaceLock())
                {
                    playerMovementController.pressSpaceTimer += Time.unscaledDeltaTime;
                    
                    if (playerMovementController.pressSpaceTimer >= ultimateDashThreshold)
                    {
                        Debug.Log("[时停] 长按触发极限冲刺");
                        ResumeTime(true);
                    }
                }
            }

            // 松开触发普通冲刺
            if (Input.GetKeyUp(KeyCode.Space) && !hasTriggeredUltimate)
            {
                Debug.Log("[时停] 松开触发冲刺");
                ResumeTime(true);
            }
        }

        private void ResumeTime(bool triggerDash)
        {
            // 终止协程
            if (freezeCoroutine != null)
            {
                StopCoroutine(freezeCoroutine);
                freezeCoroutine = null;
            }

            // 恢复时间
            Time.timeScale = 1f;
            isFreezing = false;
            playerMovementController.SetSpaceLock(false);

            if (triggerDash)
            {
                hasTriggeredUltimate = true;
                
                // 根据按压时间选择冲刺类型
                bool isUltimateDash = playerMovementController.pressSpaceTimer >= ultimateDashThreshold;
                
                // 配置冲刺参数
                if (isUltimateDash)
                {
                    // 极限冲刺配置
                    playerMovementController.PrepareUltimateDash();
                    playerMovementController.SetUltimateDashing(true);
                    playerMovementController.LockDirection(true);
                    Debug.Log("[恢复] 极限冲刺启动");
                }
                else
                {
                    // 普通冲刺配置
                    playerMovementController.StartDash();
                    playerAnimatorManager.isAttacking = false;
                    Debug.Log("[恢复] 普通冲刺启动");
                }

                // 保留部分原速度 (通过extraSpeed实现)
                float preservedSpeed = frozenVelocity.magnitude * speedPreserveRatio;
                playerMovementController.SetExtraSpeed(preservedSpeed);
            }
            else
            {
                // 无冲刺恢复原速度
                playerMovementController.GetComponent<Rigidbody2D>().linearVelocity = frozenVelocity;
            }
        }
    }
}
