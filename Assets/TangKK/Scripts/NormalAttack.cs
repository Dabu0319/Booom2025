using UnityEngine;
using shark;

namespace TangKK
{
    public class NormalAttack : MonoBehaviour
    {
        public float attackDamage = 10f; 
        private Collider2D attackCollider;
        private PlayerMovementController playerMovement;
        private Animator animator;
        private PlayerAnimatorManager playerAnimatorManager;

        void Start()
        {
            attackCollider = GetComponentInChildren<Collider2D>();
            playerMovement = GetComponentInParent<PlayerMovementController>();
            animator = GetComponentInParent<Animator>();
            playerAnimatorManager = GetComponentInParent<PlayerAnimatorManager>();

            if (attackCollider != null)
                attackCollider.enabled = true; 

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                Debug.Log($"命中敌人，造成 {attackDamage} 点伤害！");

                if (playerAnimatorManager != null)
                {
                    playerAnimatorManager.canAttack = false; // 触发后禁止攻击
                    playerMovement.SetBackwardJumpState(true); // 启动后跳
                }
            }
        }
    }
}