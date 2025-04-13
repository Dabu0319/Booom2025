using Unity.VisualScripting;
using UnityEngine;

namespace TangKK
{
    public class PlayerAttack : MonoBehaviour
    {
        [Header("Attack")]
        public bool isAttack = false;
        public float attackDuration = 0.3f;

        [Header("Parry")]
        public GameObject parryZone; // 拖入 ParryZone 子物体

        void Start()
        {
            if (parryZone != null)
            {
                parryZone.SetActive(false);
            }
        }

        void Update()
        {
            // 按下鼠标左键攻击
            if (Input.GetMouseButtonDown(0))
            {
                isAttack = true;
                Debug.Log("Attack Triggered");

                // 启动弹反 + 攻击协程
                StartCoroutine(DoAttack());
            }
        }

        private System.Collections.IEnumerator DoAttack()
        {
            if (parryZone != null)
                parryZone.SetActive(true);

            yield return new WaitForSeconds(attackDuration);

            isAttack = false;
            if (parryZone != null)
                parryZone.SetActive(false);
        }
    }
}