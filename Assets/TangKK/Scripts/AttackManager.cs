using UnityEngine;
using shark;
using System.Collections;

namespace TangKK
{
    public class AttackManager : MonoBehaviour
    {
        public bool hasAttackHit = false;

        private PlayerMovementController playerMovement;
        private PlayerAnimatorManager playerAnimatorManager;
        private SpearColliderManager spearColliderManager;
        private PerfectAttack  perfectAttack;


        [Header("攻击判定相关")]
        [SerializeField] private Collider2D attackCollider;

        public bool canTriggerPerfectAttack = true;

        private void Awake()
        {
            perfectAttack = GetComponent<PerfectAttack>();
            spearColliderManager = GetComponent<SpearColliderManager>();
            playerMovement = GetComponentInParent<PlayerMovementController>();
            playerAnimatorManager = GetComponentInParent<PlayerAnimatorManager>();
        }

        private void Update()
        {
            bool dashState = playerMovement.GetisStartAttackRecory();
            // DashScore();

            if (!dashState)
            {
                if (!playerAnimatorManager.canAttack)
                {
                    playerAnimatorManager.ResetAttack();
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

        /// <summary>
        /// 🔥 外部调用，延迟恢复攻击判定逻辑
        /// </summary>
        public void TriggerPerfectAttackRecovery(float delay = 0.5f)
        {
            Debug.Log($"[AttackManager] 接收到恢复攻击请求，延迟 {delay} 秒");
            StartCoroutine(DelayedEnableAttackLogic(delay));
        }

        private IEnumerator DelayedEnableAttackLogic(float delay)
        {
            Debug.Log($"[AttackManager] 开始等待 {delay} 秒");
            yield return new WaitForSecondsRealtime(delay);
            
            if (attackCollider != null)
            {
                attackCollider.enabled = true;
                Debug.Log("[AttackManager] 攻击 Collider 恢复成功 ✅");
            }
            else
            {
                Debug.LogWarning("[AttackManager] Collider 为 null ❌");
            }

            canTriggerPerfectAttack = true;
            Debug.Log("[AttackManager] 攻击判定恢复 ✅");
        }


        private void OnTriggerEnter2D(Collider2D other)
        {

            if ((other.CompareTag("Enemy") ||  other.CompareTag("Wall") || other.CompareTag("Scarecrow")) && playerMovement.GetDashState() == 2 )
            {

                    if(other.CompareTag("Wall"))
                    {
                        AudioManager.instance.PlaySFX("Boss Attack");    
                    }

                    if(other.CompareTag("Scarecrow"))
                    {
                        AudioManager.instance.PlaySFX("Player Normal Attack to Scarecrow");    
                    }

                    if(other.CompareTag("Enemy"))
                    {
                        AudioManager.instance.PlaySFX("Player Normal Attack to Boss");    
                    }





                    TutorialManager.Instance.TryAdvance(3);

                    playerMovement.SetBackwardJumpState(true);
                    playerMovement.SetAttackRecoryState(true);
                    Dabu10_CameraShake.instance.ShakeCamera(5f, 0.1f);
            }


            if ((other.CompareTag("Enemy") ||  other.CompareTag("Wall")|| other.CompareTag("Scarecrow")) && playerAnimatorManager.isAttacking == true  && spearColliderManager.phase1Triggered == true)
            {
                

                if (playerAnimatorManager != null)
                {
                    if(other.CompareTag("Wall"))
                    {
                        AudioManager.instance.PlaySFX("Boss Attack");    
                    }

                    if(other.CompareTag("Scarecrow"))
                    {
                        AudioManager.instance.PlaySFX("Player Normal Attack to Scarecrow");    
                    }

                    if(other.CompareTag("Enemy"))
                    {
                        AudioManager.instance.PlaySFX("Player Normal Attack to Boss");    
                    }


                    Dabu10_CameraShake.instance.ShakeCamera(5f, 0.1f);
                    playerAnimatorManager.canAttack = false; // 触发后禁止攻击
                    playerMovement.SetBackwardJumpState(true); // 启动后跳
                }

            }

            if ((other.CompareTag("Enemy") ||  other.CompareTag("Wall") || other.CompareTag("Scarecrow")) && !perfectAttack.isFreezing && spearColliderManager.phase2Triggered == true && playerAnimatorManager.isAttacking == true)
            {


                if(other.CompareTag("Wall"))
                {
                    AudioManager.instance.PlaySFX("Boss Attack");    
                }

                if(other.CompareTag("Scarecrow"))
                {
                    AudioManager.instance.PlaySFX("Player Perfect Attack to Scarecrow");    
                }

                if(other.CompareTag("Enemy"))
                {
                    AudioManager.instance.PlaySFX("Player Perfect Attack to Boss");    
                }


                TutorialManager.Instance.TryAdvance(4);
                Debug.Log("[PerfectAttack] 触发 FreezeTime");
                Dabu10_CameraShake.instance.ShakeCamera(5f, 0.1f);
                perfectAttack.StartCoroutine(perfectAttack.FreezeTime());

            }
        }

        private void DashScore()
        {
            if (playerAnimatorManager.isDash == true)
            {
                //TutorialManager.Instance.TryAdvance(3);
                AudioManager.instance.PlaySFX("Player Dash");
            }
        }












    }
}