using UnityEngine;

namespace TangKK
{
    public class PerfectAttackTrigger : MonoBehaviour
    {
        [SerializeField] public PerfectAttackFreeze freezeController;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy") && freezeController.CanRequestFreeze())
            {
                freezeController.RequestFreeze(); // ✅ 通知下一帧执行时停
            }
        }
    }
}