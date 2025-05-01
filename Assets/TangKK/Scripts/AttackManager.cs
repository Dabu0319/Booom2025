using UnityEngine;
using shark;

namespace TangKK
{
    public class AttackManager : MonoBehaviour
    {
        public bool hasAttackHit = false; // 全局攻击成功标记

        private PlayerMovementController playerMovement;
        private PlayerAnimatorManager playerAnimatorManager; // 🔥新增：控制canAttack

        private void Awake()
        {
            playerMovement = GetComponentInParent<PlayerMovementController>();
            playerAnimatorManager = GetComponentInParent<PlayerAnimatorManager>();

            if (playerMovement == null)
                Debug.LogError("AttackManager: 找不到 PlayerMovementController，请确认挂载正确！");
            
            if (playerAnimatorManager == null)
                Debug.LogError("AttackManager: 找不到 PlayerAnimatorManager，请确认挂载正确！");
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