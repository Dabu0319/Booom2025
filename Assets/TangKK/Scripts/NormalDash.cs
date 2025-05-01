using UnityEngine;
using shark;

namespace TangKK
{
    public class NormalDash : MonoBehaviour
    {
        private PlayerMovementController playerMovement;

        private void Start()
        {
            playerMovement = GetComponentInParent<PlayerMovementController>();

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (playerMovement == null) return;

            if (other.CompareTag("Enemy"))
            {
                int dashState = playerMovement.GetDashState();

                if (dashState == 2)
                {
                    Debug.Log("在非0/3状态下撞到敌人，触发后跳！");

                    playerMovement.SetBackwardJumpState(true);
                    playerMovement.SetAttackRecoryState(true);
                }
            }
        }
    }
}