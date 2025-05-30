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

            if (playerMovement == null)
            {
                Debug.LogError("NormalDash: 找不到 PlayerMovementController，请确认父物体挂载了！");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {

            if ((other.CompareTag("Enemy") || other.CompareTag("Scarecrow") || other.CompareTag("Wall")) && playerMovement.GetDashState() == 2 )
            {
                    playerMovement.SetBackwardJumpState(true);
                    playerMovement.SetAttackRecoryState(true);
                }
            }
    }
}