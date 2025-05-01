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
        [SerializeField] private float speedPreserveRatio = 0.7f;
        [SerializeField] private PlayerAnimatorManager playerAnimatorManager;

        private bool isFreezing = false;
        private bool hasTriggeredRecovery = false;

        private Vector3 frozenPosition;
        private Vector2 frozenVelocity;
        private Vector2 frozenDirection;
        private Vector2 lastInputDirection;

        private float freezeTimer = 0f;
        private float pressSpaceTimer = 0f;

        private bool canTriggerPerfectAttack = true; // ✅ 新增：控制是否可触发完美攻击

        private void Update()
        {
            if (!isFreezing) return;

            freezeTimer += Time.unscaledDeltaTime;

            HandleInputDuringFreeze();
            LockPositionDuringFreeze();

            // ✅ 时停时间到，自动恢复（无冲刺）
            if (freezeTimer >= freezeDuration && !hasTriggeredRecovery)
            {
                GetComponent<Collider2D>().enabled = true;
                hasTriggeredRecovery = true;
                ResumeTime(false, 0f);
            }
        }

        private void HandleInputDuringFreeze()
        {
            // ✅ 读取方向输入
            Vector2 inputDir = new Vector2(
                Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0,
                Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0
            ).normalized;

            if (inputDir != Vector2.zero)
            {
                lastInputDirection = inputDir;

                // ✅ 缓慢旋转而不是瞬间转动
                if (playerAnimatorManager != null)
                {
                    playerAnimatorManager.RotateTowardsDirection(inputDir);
                }
            }

            playerMovementController.SetDirection(inputDir);
            playerMovementController.SetDashDirection(inputDir);

            if (!hasTriggeredRecovery)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    pressSpaceTimer += Time.unscaledDeltaTime;

                    // ✅ 长按达到极限冲刺时间 ➜ 自动触发极限冲刺
                    if (pressSpaceTimer >= playerMovementController.ultimateDashRequiremnetTimer)
                    {
                        hasTriggeredRecovery = true;
                        ResumeTime(true, pressSpaceTimer);
                        return;
                    }
                }

                // ✅ 松开空格 ➜ 根据时间判断普通 or 极限冲刺
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    hasTriggeredRecovery = true;
                    ResumeTime(true, pressSpaceTimer);
                }
            }
        }

        private void LockPositionDuringFreeze()
        {
            if (Time.timeScale == 0f)
            {
                GetComponent<Collider2D>().enabled = false;
                transform.position = frozenPosition;
                // 可添加冻结动画、特效等
            }
        }

        private IEnumerator FreezeTime()
        {
            isFreezing = true;
            hasTriggeredRecovery = false;

            freezeTimer = 0f;
            pressSpaceTimer = 0f;

            frozenPosition = transform.position;
            frozenVelocity = playerMovementController.GetComponent<Rigidbody2D>().linearVelocity;
            frozenDirection = playerMovementController.GetDirection();

            playerMovementController.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            playerMovementController.SetExtraSpeed(0f);

            Time.timeScale = 0f;

            playerMovementController.SetSpaceLock(false);
            playerMovementController.LockDirection(false);
            playerMovementController.SetisStartAttackRecory(false);

            canTriggerPerfectAttack = false; // ✅ 禁用攻击

            Debug.Log($"[时停启动] 位置:{frozenPosition} 速度:{frozenVelocity} 方向:{frozenDirection}");

            yield break;
        }

        private void ResumeTime(bool fromSpaceKey, float spaceDuration)
        {
            Time.timeScale = 1f;
            isFreezing = false;
            GetComponent<Collider2D>().enabled = true;

            playerMovementController.SetSpaceLock(false);
            playerMovementController.LockDirection(false);

            Vector2 resumeDir = lastInputDirection != Vector2.zero ? lastInputDirection : frozenDirection;

            if (fromSpaceKey)
            {
                float preservedSpeed = frozenVelocity.magnitude * speedPreserveRatio;
                playerMovementController.SetExtraSpeed(preservedSpeed);
                playerMovementController.SetDashDirection(resumeDir);
                playerMovementController.SetDirection(resumeDir);

                bool isUltimate = spaceDuration >= playerMovementController.ultimateDashRequiremnetTimer;

                if (isUltimate)
                {
                    Debug.Log("[冲刺判定] 极限冲刺 ✅");
                    playerMovementController.PrepareUltimateDash();
                    playerMovementController.SetUltimateDashing(true);
                }
                else
                {
                    Debug.Log("[冲刺判定] 普通冲刺 ✅");
                    playerMovementController.StartDash();
                }
            }
            else
            {
                playerMovementController.ForceMoveInDirection(resumeDir);
                playerMovementController.SetDashState(0);
                playerMovementController.SetisStartAttackRecory(false);
                playerMovementController.SetUltimateDashing(false);
                playerMovementController.SetBackwardJumpState(false);
                Debug.Log("[时停] 自动恢复，无冲刺");
            }

            // ✅ 清空计时器
            pressSpaceTimer = 0f;
            freezeTimer = 0f;

            canTriggerPerfectAttack = true; // ✅ 恢复攻击能力
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!canTriggerPerfectAttack) return; // ✅ 禁止攻击期间不触发

            if (collision.CompareTag("Enemy") && !isFreezing)
            {
                StartCoroutine(FreezeTime());
            }
        }

        private void OnGUI()
        {
            if (!isFreezing) return;

            GUI.Box(new Rect(Screen.width / 2 - 100, 20, 200, 25), "TIME FREEZE ACTIVE");

            GUI.Label(new Rect(10, 100, 300, 20), $"方向: {lastInputDirection}");
            GUI.Label(new Rect(10, 120, 300, 20), $"空格计时: {pressSpaceTimer:F2}s");
            GUI.Label(new Rect(10, 140, 300, 20), $"时停计时: {freezeTimer:F2}s");
            GUI.Label(new Rect(10, 160, 300, 20), $"hasTriggeredRecovery: {hasTriggeredRecovery}");
            GUI.Label(new Rect(10, 180, 300, 20), $"极限冲刺时间: {playerMovementController.ultimateDashRequiremnetTimer:F2}s");
        }
    }
}