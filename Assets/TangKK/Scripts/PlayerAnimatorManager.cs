using UnityEngine;
using shark;

namespace TangKK
{
    public class PlayerAnimatorManager : MonoBehaviour
    {
        private PlayerMovementController playerMovement;
        private Animator animator;
        
        public bool isAttacking = false; // 仍然保留，控制动画状态
        public bool canAttack = true;    // 🔥新增：控制是否允许攻击

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovementController>();
            animator = GetComponentInChildren<Animator>();

            if (playerMovement == null)
            {
                Debug.LogError("PlayerAnimatorManager: 没找到 PlayerMovementController 组件！");
            }

            if (animator == null)
            {
                Debug.LogError("PlayerAnimatorManager: 没找到 Animator 组件！（包括子物体）");
            }
        }

        private void Update()
        {
            if (playerMovement == null || animator == null) return;

            // 🔥 修改这里：只有在 canAttack == true 的情况下才允许攻击
            isAttacking = (playerMovement.GetDashState() == 3) && canAttack;

            animator.SetBool("isAttacking", isAttacking);
        }

        private void LateUpdate()
        {
            if (playerMovement == null) return;

            Vector2 direction = playerMovement.GetDirection();
            if (direction != Vector2.zero)
            {
                transform.up = direction.normalized;
            }
        }

        // 🔥新增：专门提供一个方法，外部可以打断攻击
        public void InterruptAttack()
        {
            canAttack = false; // 禁止攻击
            isAttacking = false; // 手动设置为不攻击
            if (animator != null)
            {
                animator.SetBool("isAttacking", false); // 立刻停止动画
            }
            Debug.Log("攻击被打断！");
        }

        // 🔥新增：恢复攻击能力
        public void ResetAttack()
        {
            canAttack = true;
            Debug.Log("可以再次攻击了！");
        }
    }
}