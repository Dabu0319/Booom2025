using UnityEngine;
using shark;
using System.Collections;

namespace TangKK
{
    public class PerfectAttack_01 : MonoBehaviour
    {
        [Header("Freeze Settings")]
        [SerializeField] private float freezeDuration = 3f;
        [SerializeField] private PlayerMovementController playerMovementController;
        [SerializeField] private float speedPreserveRatio = 0.7f;
        [SerializeField] private PlayerAnimatorManager_01 playerAnimatorManager;
        [SerializeField] private URPTimeStopEffect timeStopEffect;
        private SpearColliderManager_01 spearColliderManager;

        [Header("Movement During Freeze")]
        [SerializeField] private float freezeMovementSpeed = 5f; // 时停期间的移动速度
        [SerializeField] private bool allowFullMovement = true;   // 是否允许完全自由移动

        [Header("Collider Reference")]

        [Header("Manager")]
        [SerializeField] private AttackManager_01 attackManager;

        public bool isFreezing = false;
        private bool hasTriggeredRecovery = false;

        private Vector3 frozenPosition;
        private Vector2 frozenVelocity;
        private Vector2 frozenDirection;
        private Vector2 currentInputDirection;
        private Vector2 lastValidDirection;

        private float freezeTimer = 0f;
        private float pressSpaceTimer = 0f;
        private Rigidbody2D playerRigidbody;

        private void Awake()
        {
            spearColliderManager = GetComponent<SpearColliderManager_01>();
            playerRigidbody = playerMovementController.GetComponent<Rigidbody2D>();

            if (attackManager == null)
            {
                attackManager = FindObjectOfType<AttackManager_01>();
            }
        }

        private void Update()
        {
            if (!isFreezing) return;

            freezeTimer += Time.unscaledDeltaTime;
            HandleInputAndMovementDuringFreeze();

            // 自动恢复检查
            if (freezeTimer >= freezeDuration && !hasTriggeredRecovery)
            {
                hasTriggeredRecovery = true;
                ResumeTime(false, 0f);
            }
        }

        private void HandleInputAndMovementDuringFreeze()
        {
            // 获取玩家输入
            Vector2 inputDir = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ).normalized;

            currentInputDirection = inputDir;

            // 更新最后有效方向
            if (inputDir != Vector2.zero)
            {
                lastValidDirection = inputDir;
                playerAnimatorManager?.RotateTowardsDirection(inputDir);
            }

            // 处理时停期间的自由移动
            if (allowFullMovement)
            {
                HandleFreeMovementDuringFreeze(inputDir);
            }
            else
            {
                // 原有的受限移动逻辑
                playerMovementController.SetDirection(inputDir);
                playerMovementController.SetDashDirection(inputDir);
            }

            // 处理空格键输入
            HandleSpaceInputDuringFreeze();
        }

        private void HandleFreeMovementDuringFreeze(Vector2 inputDir)
        {
            if (inputDir != Vector2.zero)
            {
                // 直接通过PlayerMovementController处理移动
                playerMovementController.HandleTimeFreezeMovement(inputDir, freezeMovementSpeed);
                
                // 更新控制器方向
                playerMovementController.SetDirection(inputDir);
                playerMovementController.SetDashDirection(inputDir);
            }
            else
            {
                // 停止移动
                playerMovementController.StopTimeFreezeMovement();
            }
        }

        private void HandleSpaceInputDuringFreeze()
        {
            if (hasTriggeredRecovery) return;

            if (Input.GetButton("Jump"))
            {
                pressSpaceTimer += Time.unscaledDeltaTime;

                // 达到极限冲刺时间要求
                if (pressSpaceTimer >= playerMovementController.ultimateDashRequiremnetTimer)
                {
                    hasTriggeredRecovery = true;
                    ResumeTime(true, pressSpaceTimer);
                    return;
                }
            }

            // 松开空格键时触发普通冲刺
            if (Input.GetButtonUp("Jump"))
            {
                hasTriggeredRecovery = true;
                ResumeTime(true, pressSpaceTimer);
            }
        }

        public IEnumerator FreezeTime()
        {
            Debug.Log("[PerfectAttack] FreezeTime 协程启动 - 支持自由移动");

            isFreezing = true;
            hasTriggeredRecovery = false;

            freezeTimer = 0f;
            pressSpaceTimer = 0f;

            // 启动时停特效
            timeStopEffect?.Activate();

            // 保存当前状态
            frozenVelocity = playerRigidbody.linearVelocity;
            frozenDirection = playerMovementController.GetDirection();
            frozenPosition = transform.position;

            // 启动全局时停（但玩家可以移动）
            Time.timeScale = 0f;

            // 设置PlayerMovementController进入时停状态
            playerMovementController.SetTimeFreezeState(true);

            // 解锁玩家控制权，允许自由移动
            playerMovementController.SetSpaceLock(false);
            playerMovementController.LockDirection(false);
            playerMovementController.SetisStartAttackRecory(false);
            playerMovementController.SetDashState(0);
            playerMovementController.SetExtraSpeed(0f);

            // 禁止再次触发PerfectAttack
            if (attackManager != null)
                attackManager.canTriggerPerfectAttack = false;

            Debug.Log($"[PerfectAttack] 时停已激活，允许自由移动: {allowFullMovement}");

            yield break;
        }

        private void ResumeTime(bool fromSpaceKey, float spaceDuration)
        {
            Debug.Log($"[ResumeTime] 执行，fromSpaceKey={fromSpaceKey}, spaceDuration={spaceDuration}");

            // 恢复时间流逝
            Time.timeScale = 1f;
            timeStopEffect?.Deactivate();
            isFreezing = false;

            // 退出时停状态
            playerMovementController.SetTimeFreezeState(false);

            // 解除控制限制
            playerMovementController.SetSpaceLock(false);
            playerMovementController.LockDirection(false);

            // 确定恢复方向
            Vector2 resumeDir = GetResumeDirection();

            try
            {
                if (fromSpaceKey)
                {
                    ExecuteDashRecovery(resumeDir, spaceDuration);
                }
                else
                {
                    ExecuteNormalRecovery(resumeDir);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ResumeTime] 异常：{ex.Message}\n{ex.StackTrace}");
            }

            ResetTimers();
            TriggerAttackRecovery();
        }

        private Vector2 GetResumeDirection()
        {
            // 优先使用当前输入方向，其次是最后有效方向，最后是冻结时的方向
            if (currentInputDirection != Vector2.zero)
                return currentInputDirection;
            else if (lastValidDirection != Vector2.zero)
                return lastValidDirection;
            else
                return frozenDirection;
        }

        private void ExecuteDashRecovery(Vector2 resumeDir, float spaceDuration)
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

        private void ExecuteNormalRecovery(Vector2 resumeDir)
        {
            playerMovementController.ForceMoveInDirection(resumeDir);
            playerMovementController.SetDashState(0);
            playerMovementController.SetisStartAttackRecory(false);
            playerMovementController.SetUltimateDashing(false);
            playerMovementController.SetBackwardJumpState(false);
            Debug.Log("[时停] 自动恢复，无冲刺");
        }

        private void ResetTimers()
        {
            pressSpaceTimer = 0f;
            freezeTimer = 0f;
        }

        private void TriggerAttackRecovery()
        {
            Debug.Log("[ResumeTime] 委托 AttackManager 执行攻击恢复协程");
            attackManager?.TriggerPerfectAttackRecovery(0.5f);
        }

        // 公共方法用于运行时调整
        public void SetFreezeMovementSpeed(float speed)
        {
            freezeMovementSpeed = speed;
        }

        public void SetAllowFullMovement(bool allow)
        {
            allowFullMovement = allow;
        }

        // 调试信息
        private void OnGUI()
        {
            if (!isFreezing) return;

            GUI.Label(new Rect(10, 10, 300, 20), $"时停状态 - 剩余时间: {(freezeDuration - freezeTimer):F1}s");
            GUI.Label(new Rect(10, 30, 300, 20), $"空格计时: {pressSpaceTimer:F1}s");
            GUI.Label(new Rect(10, 50, 300, 20), $"当前方向: {currentInputDirection}");
            GUI.Label(new Rect(10, 70, 300, 20), $"自由移动: {allowFullMovement}");
        }
    }
}