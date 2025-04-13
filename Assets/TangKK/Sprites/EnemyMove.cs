using UnityEngine;

namespace TangKK{
public class EnemyMove : MonoBehaviour
{
    [Header("追踪设置")]
    public float moveSpeed = 2f;             // 移动速度
    public float stopDistance = 0.5f;        // 与目标保持的最小距离

    private Transform target;                // 目标（通常是玩家）

    void Start()
    {
        // 查找玩家对象（假设玩家有 "Player" 标签）
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("找不到带有标签 'Player' 的对象！");
        }
    }

    void Update()
    {
        if (target == null) return;

        // 计算目标方向
        Vector2 direction = target.position - transform.position;

        // 如果距离大于停止距离就向目标移动
        if (direction.magnitude > stopDistance)
        {
            Vector2 move = direction.normalized * moveSpeed * Time.deltaTime;
            transform.Translate(move);
        }
    }

    // 可选：在 Scene 视图绘制追踪线
    private void OnDrawGizmos()
    {
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
}