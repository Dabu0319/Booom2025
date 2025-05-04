using UnityEngine;
using shark;
using System.Collections;

namespace TangKK
{
    public class AttackManager : MonoBehaviour
    {
        public bool hasAttackHit = false;

        private PlayerMovementController playerMovement;
        private PlayerAnimatorManager playerAnimatorManager;
        private SpearColliderManager spearColliderManager;
        private PerfectAttack perfectAttack;

        public bool canTriggerPerfectAttack = true;

        private bool hasHandledTrigger = false; // ✅ 保证每次攻击只处理一次碰撞

        private void Awake()
        {
            perfectAttack = GetComponent<PerfectAttack>();
            spearColliderManager = GetComponent<SpearColliderManager>();
            playerMovement = GetComponentInParent<PlayerMovementController>();
            playerAnimatorManager = GetComponentInParent<PlayerAnimatorManager>();
        }

        private void Update()
        {
            bool dashState = playerMovement.GetisStartAttackRecory();

            if (!dashState)
            {
                if (!playerAnimatorManager.canAttack)
                {
                    playerAnimatorManager.ResetAttack();
                }
            }
        }

        public void ResetAttack()
        {
            hasAttackHit = false;
            hasHandledTrigger = false; // ✅ 重置触发状态
        }

        public void SetAttackHit()
        {
            hasAttackHit = true;
        }

        /// <summary>
        /// 🔥 外部调用，延迟恢复攻击判定逻辑
        /// </summary>
        public void TriggerPerfectAttackRecovery(float delay = 0.5f)
        {
            Debug.Log($"[AttackManager] 接收到恢复攻击请求，延迟 {delay} 秒");
            StartCoroutine(DelayedEnableAttackLogic(delay));
        }

        private IEnumerator DelayedEnableAttackLogic(float delay)
        {
            Debug.Log($"[AttackManager] 开始等待 {delay} 秒");
            yield return new WaitForSecondsRealtime(delay);

            canTriggerPerfectAttack = true;
            Debug.Log("[AttackManager] 攻击判定恢复 ✅");
        }

        private void OnTriggerEnter2D(Collider2D other)
{
    // ✅ 只处理指定目标
    if (!(other.CompareTag("Enemy") || other.CompareTag("Wall") || other.CompareTag("Scarecrow")))
        return;

    // ✅ 优先级 1：冲刺攻击
    if (playerMovement.GetDashState() == 2)
    {
        HandleDashAttack(other);
        return;
    }

    // ✅ 优先级 2：普通攻击（phase1）
    if (playerAnimatorManager.isAttacking &&
        spearColliderManager.phase1Triggered &&
        !spearColliderManager.phase2Triggered)
    {
        HandleNormalAttack(other);
        return;
    }

    // ✅ 优先级 3：Perfect 攻击（phase2）
    if (playerAnimatorManager.isAttacking &&
        spearColliderManager.phase2Triggered &&
        !perfectAttack.isFreezing)
    {
        HandlePerfectAttack(other);
        return;
    }
}




        private void HandlePerfectAttack(Collider2D other)
        {
            hasHandledTrigger = true;
            PlayAttackSFX(other, perfect: true);

            Debug.Log("[PerfectAttack] 触发 FreezeTime");
            Dabu10_CameraShake.instance.ShakeCamera(5f, 0.1f);
            TutorialManager.Instance.TryAdvance(4);

            perfectAttack.StartCoroutine(perfectAttack.FreezeTime());
        }

        private void HandleNormalAttack(Collider2D other)
        {
            hasHandledTrigger = true;
            PlayAttackSFX(other, perfect: false);

            Dabu10_CameraShake.instance.ShakeCamera(5f, 0.1f);
            TutorialManager.Instance.TryAdvance(3);

            playerAnimatorManager.canAttack = false;
            playerMovement.SetBackwardJumpState(true);
        }

        private void HandleDashAttack(Collider2D other)
        {
            hasHandledTrigger = true;
            PlayAttackSFX(other, perfect: false);

            Dabu10_CameraShake.instance.ShakeCamera(5f, 0.1f);
            TutorialManager.Instance.TryAdvance(3);

            playerMovement.SetBackwardJumpState(true);
            playerMovement.SetAttackRecoryState(true);
        }

        private void PlayAttackSFX(Collider2D other, bool perfect)
        {
            if (other.CompareTag("Wall"))
            {
                AudioManager.instance.PlaySFX("Boss Attack");
            }
            else if (other.CompareTag("Scarecrow"))
            {
                AudioManager.instance.PlaySFX(perfect ? "Player Perfect Attack to Scarecrow" : "Player Normal Attack to Scarecrow");
            }
            else if (other.CompareTag("Enemy"))
            {
                AudioManager.instance.PlaySFX(perfect ? "Player Perfect Attack to Boss" : "Player Normal Attack to Boss");
            }
        }
    }
}