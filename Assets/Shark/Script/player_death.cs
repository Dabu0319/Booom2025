using UnityEngine;
using UnityEngine.SceneManagement;

public class player_death : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private PlayerMovementController movementController;
    
    [Header("重置设置")]
    [SerializeField] private float resetDelay = 1.5f; // 死亡后重置场景的延迟时间
    private bool isDead = false; // 防止多次触发死亡


    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 可以使用标签检测敌人，也可以在敌人身上添加特定组件检测
        if (!isDead && collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 同上，适用于碰撞体而非触发器的情况
        if (!isDead && collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
    }
    */


    public void Die()
    {
        if (DeathMarker2D.Instance != null){
            DeathMarker2D.Instance.RecordDeathPosition(playerCharacter.transform.position);
        }
        movementController.SetIsDead(true);
        isDead = true;
        // 延迟后重置场景
        Invoke("ResetScene", resetDelay);
    }


    private void ResetScene()
    {
        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // 或者可以加载特定的场景：SceneManager.LoadScene("你的场景名称");
    }
}