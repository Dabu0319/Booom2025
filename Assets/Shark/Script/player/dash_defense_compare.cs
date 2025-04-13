using UnityEngine;
using System.Collections;



namespace shark{

public class dash_defense_compare : MonoBehaviour
{
    private float powerValue;         // 玩家的冲击力
    public float penetrationThreshold = 2f; // 穿透阈值（敌方防御值的 2 倍则穿透）
    public float knockbackForce = 4f;
    public Collider2D playerCollider;
    public MovementController playerMovementController;
    public GameObject player;


    private void OnTriggerEnter2D(Collider2D  collision)
    {
        powerValue = playerMovementController.GetSpeed();
        if(playerMovementController.GetDashState() == true){
            if (collision.gameObject.CompareTag("Enemy"))
            {
                Transform enemyShieldTransform = collision.gameObject.transform.Find("EnemyShield");
                if (enemyShieldTransform != null) 
                {
                    enemy_defense_info enemy = enemyShieldTransform.gameObject.GetComponent<enemy_defense_info>();
                    if (enemy != null) 
                    {
                        Debug.Log(powerValue);
                        // 计算碰撞方向（敌人 → 玩家）
                        Vector2 hitDirection = (transform.position - collision.transform.position).normalized;
                        float enemyDefense = enemy.GetDefenseValue(hitDirection);
                        // 数值比较 + 触发不同效果
                        if (powerValue > enemyDefense * penetrationThreshold)
                        {
                            // 玩家值 ≫ 敌人 → 穿透
                            StartCoroutine(TemporaryPenetration(collision));
                            playerMovementController.OnSuccessfulPenetration(); // 调用穿刺时的算法
                        }
                        else if (powerValue >= enemyDefense) 
                        {
                            // 撞飞敌人：用Lerp平滑移动
                            StartCoroutine(KnockbackCoroutine(collision.gameObject.transform, hitDirection * -1, 0.2f));
                        }
                        else if (powerValue < enemyDefense) 
                        {
                            // 玩家被弹开
                            StartCoroutine(KnockbackCoroutine(player.transform, hitDirection, 0.2f));
                            playerMovementController.OnFailDash();
                        }
                    }
                }
            }
        }

    }


    IEnumerator KnockbackCoroutine(Transform target, Vector2 direction, float duration) 
        {
            float elapsed = 0f;
            Vector2 startPos = target.position;
            Vector2 endPos = startPos + direction * knockbackForce;
            while (elapsed < duration) 
            {
                target.position = Vector2.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    
    IEnumerator TemporaryPenetration(Collider2D enemyCollider)
    {
        // 忽略碰撞
        Physics2D.IgnoreCollision(playerCollider, enemyCollider, true);
        
        // 等待X秒后恢复碰撞
        yield return new WaitForSeconds(0.5f);
        
        // 恢复碰撞
        Physics2D.IgnoreCollision(playerCollider, enemyCollider, false);
    }


}
}
