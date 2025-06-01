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

        // 🔥 新增：保存时停前的状态，用于更好的恢复
        private int preFreezeDashState;
        private bool preFreezeSpaceLock;
        private bool preFreezeDirectionLock;

        private void Start()
        {
            spearColliderManager = GetComponent<SpearColliderManager_01>();
            
            // 🔥 添加空值检查
            if (playerMovementController == null)
            {
                Debug.LogError("[PerfectAttack] playerMovementController 未分配！请在Inspector中设置。", this);
                return;
            }
            
            playerRigidbody = playerMovementController.GetComponent<Rigidbody2D>();
            
            if (playerRigidbody == null)
            {
                Debug.LogError("[PerfectAttack] PlayerMovementController上没有找到Rigidbody2D组件！", this);
            }

            if (attackManager == null)
            {
                attackManager = GetComponent<AttackManager_01>();
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
            CinemachineTimeStopUpdater.Instance.Enter();

            // 启动时停特效
            timeStopEffect?.Activate();

            // 🔥 保存当前状态以便更好的恢复
            SavePreFreezeState();

            // 保存当前状态
            if (playerRigidbody != null)
            {
                frozenVelocity = playerRigidbody.linearVelocity;
            }
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

        // 🔥 新增：保存时停前的状态
        private void SavePreFreezeState()
        {
            preFreezeDashState = playerMovementController.GetDashState();
            preFreezeSpaceLock = playerMovementController.GetSpaceLock();
            // preFreezeDirectionLock 暂时没有对应的获取方法，可以默认为false
            preFreezeDirectionLock = false;
            
            Debug.Log($"[PerfectAttack] 保存时停前状态 - DashState: {preFreezeDashState}, SpaceLock: {preFreezeSpaceLock}");
        }

        private void ResumeTime(bool fromSpaceKey, float spaceDuration)
        {
            Debug.Log($"[ResumeTime] 执行，fromSpaceKey={fromSpaceKey}, spaceDuration={spaceDuration}");

            // 恢复时间流逝
            Time.timeScale = 1f;
            timeStopEffect?.Deactivate();
            isFreezing = false;
            CinemachineTimeStopUpdater.Instance.Exit();

            // 退出时停状态
            playerMovementController.SetTimeFreezeState(false);

            // 🔥 关键修改：统一的状态清理，确保一致性
            ResetPlayerStateForRecovery();

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
                    // 🔥 关键修改：自动恢复时使用相同的逻辑
                    ExecuteAutoRecovery(resumeDir);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ResumeTime] 异常：{ex.Message}\n{ex.StackTrace}");
            }

            ResetTimers();
            TriggerAttackRecovery();
        }

        // 🔥 新增：统一的状态重置方法
        private void ResetPlayerStateForRecovery()
        {
            // 🔥 关键修复：强制重置所有冲刺相关的内部标志
            playerMovementController.SetUltimateDashing(false);     // 清除极限冲刺标志
            playerMovementController.SetisStartAttackRecory(false); // 清除攻击恢复标志
            playerMovementController.SetBackwardJumpState(false);   // 清除后跳标志
            
            // 🔥 最重要：强制设置为完全空闲状态
            playerMovementController.SetDashState(0);
            
            // 清理控制锁定
            playerMovementController.SetSpaceLock(false);
            playerMovementController.LockDirection(false);
            
            // 🔥 重要：清除冷却时间，确保玩家可以立即操作
            playerMovementController.SetDashCooldownTimer(0f);
            
            // 🔥 新增：清除额外速度，避免状态残留
            playerMovementController.SetExtraSpeed(0f);
            
            // 🔥 关键修复：清零 PlayerMovementController 中的 pressSpaceTimer
            playerMovementController.pressSpaceTimer = 0f;
            
            Debug.Log("[ResumeTime] 玩家状态已强制重置为空闲状态，pressSpaceTimer已清零");
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

        // 🔥 新的自动恢复方法：确保与手动恢复后的状态一致
        private void ExecuteAutoRecovery(Vector2 resumeDir)
        {
            // 设置方向
            playerMovementController.SetDirection(resumeDir);
            playerMovementController.SetDashDirection(resumeDir);
            
            // 🔥 关键修复：确保玩家完全退出所有冲刺状态
            playerMovementController.SetUltimateDashing(false);
            playerMovementController.SetisStartAttackRecory(false);
            playerMovementController.SetDashState(0); // 强制设置为空闲状态
            
            // 🔥 关键修复：清零 PlayerMovementController 中的 pressSpaceTimer
            playerMovementController.pressSpaceTimer = 0f;
            
            // 给予一个很小的初始移动，让玩家脱离时停位置
            if (resumeDir != Vector2.zero)
            {
                playerMovementController.SetExtraSpeed(0f); // 确保没有额外速度
            }
            
            // 🔥 新增：等待一帧后再最终确认状态
            StartCoroutine(DelayedStateConfirmation());
            
            Debug.Log("[时停] 自动恢复完成，pressSpaceTimer已清零");
        }

        // 🔥 新增：延迟确认状态，确保系统稳定
        private IEnumerator DelayedStateConfirmation()
        {
            // 等待一帧，让所有系统都完成状态更新
            yield return null;
            
            // 🔥 关键修复：强制确认玩家处于空闲状态
            playerMovementController.SetUltimateDashing(false);  // 再次确认清除极限冲刺
            playerMovementController.SetisStartAttackRecory(false); // 再次确认清除攻击恢复
            playerMovementController.SetSpaceLock(false);
            playerMovementController.SetDashCooldownTimer(0f);
            
            // 🔥 关键修复：再次确认清零 pressSpaceTimer
            playerMovementController.pressSpaceTimer = 0f;
            
            // 🔥 最关键：强制设置为状态0，确保下次空格键会触发状态1而不是保持状态2
            playerMovementController.SetDashState(0);
            
            // 验证状态是否正确设置
            int finalDashState = playerMovementController.GetDashState();
            float finalPressSpaceTimer = playerMovementController.pressSpaceTimer;
            
            Debug.Log($"[DelayedStateConfirmation] 最终状态 - DashState: {finalDashState}, pressSpaceTimer: {finalPressSpaceTimer}");
            
            if (finalDashState != 0)
            {
                Debug.LogError($"[DelayedStateConfirmation] 警告！状态重置失败，当前状态: {finalDashState}，尝试再次重置...");
                
                // 如果还是不对，尝试更激进的重置
                yield return new WaitForSecondsRealtime(0.1f);
                playerMovementController.SetDashState(0);
                playerMovementController.SetUltimateDashing(false);
                playerMovementController.pressSpaceTimer = 0f;
                
                int retryState = playerMovementController.GetDashState();
                Debug.Log($"[DelayedStateConfirmation] 重试后状态: {retryState}");
            }
            else
            {
                Debug.Log("[DelayedStateConfirmation] ✅ 状态确认完成，玩家可以正常冲刺 (状态0，pressSpaceTimer=0)");
            }
        }

        private void ResetTimers()
        {
            pressSpaceTimer = 0f;
            freezeTimer = 0f;
            
            // 🔥 关键修复：同时清零 PlayerMovementController 中的 pressSpaceTimer
            if (playerMovementController != null)
            {
                playerMovementController.pressSpaceTimer = 0f;
                Debug.Log("[ResetTimers] 本地和控制器的 pressSpaceTimer 都已清零");
            }
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
            
            // 🔥 新增调试信息
            if (playerMovementController != null)
            {
                GUI.Label(new Rect(10, 90, 300, 20), $"冲刺状态: {playerMovementController.GetDashState()}");
                GUI.Label(new Rect(10, 110, 300, 20), $"空格锁: {playerMovementController.GetSpaceLock()}");
                GUI.Label(new Rect(10, 130, 300, 20), $"攻击恢复: {playerMovementController.GetisStartAttackRecory()}");
            }
        }
    }
}