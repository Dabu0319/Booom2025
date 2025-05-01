using UnityEngine;
using shark;

namespace TangKK
{
    public class AttackManager : MonoBehaviour
    {
        public bool hasAttackHit = false; // 全局攻击成功标记

        private PlayerMovementController playerMovement;
        private PlayerAnimatorManager playerAnimatorManager;

        [Header("攻击判定相关")]
        [SerializeField] private Collider2D attackCollider;

        public bool canTriggerPerfectAttack = true;

        private void Awake()
        {
            playerMovement = GetComponentInParent<PlayerMovementController>();
            playerAnimatorManager = GetComponentInParent<PlayerAnimatorManager>();

        }

        private void Update()
        {
            if (playerMovement == null || playerAnimatorManager == null) return;

            bool dashState = playerMovement.GetisStartAttackRecory();

            // 🔥如果Dash状态不是3（攻击状态），就可以恢复攻击
            if (dashState == false)
            {
                if (!playerAnimatorManager.canAttack)
                {
                    playerAnimatorManager.ResetAttack(); // 调用恢复攻击的方法
                }
            }
        }

        public void ResetAttack()
        {
            hasAttackHit = false;
        }

        public void SetAttackHit()
        {
            hasAttackHit = true;
        }
    }
}