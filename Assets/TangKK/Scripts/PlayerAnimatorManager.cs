using UnityEngine;
using shark;

namespace TangKK
{
    public class PlayerAnimatorManager : MonoBehaviour
    {
        private PlayerMovementController playerMovement;
        private Animator animator;
        private Rigidbody2D rb;

        public bool isAttacking = false;
        public bool canAttack = true;
        public bool isRunning = false;

        [Header("旋转控制")]
        [SerializeField] private float turnSpeed = 10f;

        private void Awake()
        {
            playerMovement = GetComponentInParent<PlayerMovementController>();
            animator = GetComponent<Animator>();
            rb = GetComponentInParent<Rigidbody2D>();

            if (playerMovement == null) Debug.LogError("未找到 PlayerMovementController");
            if (animator == null) Debug.LogError("未找到 Animator");
            if (rb == null) Debug.LogError("未找到 Rigidbody2D");
        }

        private void Update()
        {
            if (playerMovement == null || animator == null || rb == null) return;

            isAttacking = playerMovement.GetisStartAttackRecory() && canAttack;
            animator.SetBool("isAttacking", isAttacking);

            isRunning = rb.linearVelocity.magnitude > 0.05f;
            animator.SetBool("isRunning", isRunning);
        }

        private void LateUpdate()
        {
            if (playerMovement == null) return;

            Vector2 direction = playerMovement.GetDirection();
            if (direction != Vector2.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    turnSpeed * Time.unscaledDeltaTime // ✅ 支持时停中转向
                );
            }
        }

        public void InterruptAttack()
        {
            canAttack = false;
            isAttacking = false;
            if (animator != null)
            {
                animator.SetBool("isAttacking", false);
            }
        }

        public void ResetAttack()
        {
            canAttack = true;
        }

        public void RotateTowardsDirection(Vector2 direction)
        {
            if (direction == Vector2.zero) return;

            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.unscaledDeltaTime
            );
        }











        
    }
}