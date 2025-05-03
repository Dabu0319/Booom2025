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
        private SpearColliderManager spearColliderManager;

        void Start()
        {   spearColliderManager = GetComponent<SpearColliderManager>();
            attackCollider = GetComponentInChildren<Collider2D>();
            playerMovement = GetComponentInParent<PlayerMovementController>();
            animator = GetComponentInParent<Animator>();
            playerAnimatorManager = GetComponentInParent<PlayerAnimatorManager>();

            if (attackCollider != null)
                attackCollider.enabled = true; 

            if (playerMovement == null)
                Debug.LogError("找不到 PlayerMovementController！请检查父物体是否挂载了！");
            
            if (animator == null)
                Debug.LogError("找不到 Animator！请检查父物体是否挂载了！");

            if (playerAnimatorManager == null)
                Debug.LogError("找不到 PlayerAnimatorManager！请检查父物体是否挂载了！");
        }

        // private void OnTriggerEnter2D(Collider2D other)
        // {
        //     if (other.CompareTag("Enemy") && playerAnimatorManager.isAttacking == true  && spearColliderManager.phase1Triggered == true)
        //     {
        //         Debug.Log($"命中敌人，造成 {attackDamage} 点伤害！");

        //         if (playerAnimatorManager != null)
        //         {
        //             playerAnimatorManager.canAttack = false; // 触发后禁止攻击
        //             playerMovement.SetBackwardJumpState(true); // 启动后跳
        //         }

        //     }
        // }
    }
}