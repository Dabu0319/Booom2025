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

        [Header("Collider Reference")]
        [SerializeField] private Collider2D boxCollider;

        [Header("Manager")]
        [SerializeField] private AttackManager attackManager;

        private bool isFreezing = false;
        private bool hasTriggeredRecovery = false;

        private Vector3 frozenPosition;
        private Vector2 frozenVelocity;
        private Vector2 frozenDirection;
        private Vector2 lastInputDirection;

        private float freezeTimer = 0f;
        private float pressSpaceTimer = 0f;

        private void Awake()
        {
            if (boxCollider == null)
            {
                boxCollider = GetComponent<Collider2D>();
            }

            if (attackManager == null)
            {
                attackManager = FindObjectOfType<AttackManager>();
            }
        }

        private void Update()
        {
            if (!isFreezing) return;

            freezeTimer += Time.unscaledDeltaTime;
            HandleInputDuringFreeze();
            LockPositionDuringFreeze();

            if (freezeTimer >= freezeDuration && !hasTriggeredRecovery)
            {
                hasTriggeredRecovery = true;
                ResumeTime(false, 0f);
            }
        }

        private void HandleInputDuringFreeze()
        {
            Vector2 inputDir = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ).normalized;

            if (inputDir != Vector2.zero)
            {
                lastInputDirection = inputDir;
                playerAnimatorManager?.RotateTowardsDirection(inputDir);
            }

            playerMovementController.SetDirection(inputDir);
            playerMovementController.SetDashDirection(inputDir);

            if (!hasTriggeredRecovery)
            {
                if (Input.GetButton("Jump"))
                {
                    pressSpaceTimer += Time.unscaledDeltaTime;

                    if (pressSpaceTimer >= playerMovementController.ultimateDashRequiremnetTimer)
                    {
                        hasTriggeredRecovery = true;
                        ResumeTime(true, pressSpaceTimer);
                        return;
                    }
                }

                if (Input.GetButtonUp("Jump"))
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
                // if (boxCollider != null)
                // {
                //     boxCollider.enabled = false;
                // }

                transform.position = frozenPosition;
            }
        }

        private IEnumerator FreezeTime()
        {
            Debug.Log("[PerfectAttack] FreezeTime 协程启动");

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

            if (attackManager != null)
                attackManager.canTriggerPerfectAttack = false;

            yield break;
        }

        private void ResumeTime(bool fromSpaceKey, float spaceDuration)
        {
            Debug.Log($"[ResumeTime] 执行，fromSpaceKey={fromSpaceKey}, spaceDuration={spaceDuration}");

            Time.timeScale = 1f;
            isFreezing = false;

            playerMovementController.SetSpaceLock(false);
            playerMovementController.LockDirection(false);

            Vector2 resumeDir = lastInputDirection != Vector2.zero ? lastInputDirection : frozenDirection;

            try
            {
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
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ResumeTime] 异常：{ex.Message}\n{ex.StackTrace}");
            }

            pressSpaceTimer = 0f;
            freezeTimer = 0f;

            Debug.Log("[ResumeTime] 委托 AttackManager 执行攻击恢复协程");
            attackManager?.TriggerPerfectAttackRecovery(0.5f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log($"[OnTriggerEnter2D] 检测到碰撞对象: {collision.name}");

            if (attackManager == null || !attackManager.canTriggerPerfectAttack)
            {
                Debug.Log("[PerfectAttack] 攻击判定不可用，忽略触发 ❌");
                return;
            }

            if (collision.CompareTag("Enemy") && !isFreezing)
            {
                Debug.Log("[PerfectAttack] 触发 FreezeTime");
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