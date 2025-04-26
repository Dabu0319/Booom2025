using UnityEngine;
using shark;

namespace TangKK
{
    public class NormalAttack : MonoBehaviour
    {
        private PlayerMovementController playerMovement;
        private bool hasTriggeredAttack = false; // 防止多次触发

        void Start()
        {
            playerMovement = GetComponent<PlayerMovementController>();
        }

        void Update()
        {
            if (playerMovement == null) return;

            // 检查是否处于极限冲刺后摇状态
            if (playerMovement.GetDashState() == 3)
            {
                if (!hasTriggeredAttack)
                {
                    TriggerAttack();  // 触发攻击
                    hasTriggeredAttack = true; // 标记已经触发过
                }
            }
            else
            {
                // 如果不在后摇状态，重置触发器，防止漏掉下次
                hasTriggeredAttack = false;
            }
        }

        /// <summary>
        /// 你的攻击逻辑写在这里
        /// </summary>
        private void TriggerAttack()
        {
            Debug.Log("极限冲刺后摇，攻击触发！");
            // 在这里写你的攻击处理，比如播放动画、伤害判定等等
        }
    }
}