using UnityEngine;



namespace shark{
    

public class enemy_defense_info : MonoBehaviour
{
    [Header("Defense Value")]
    [SerializeField] private float front;
    [SerializeField] private float left;
    [SerializeField] private float right;
    [SerializeField] private float back;

    public Rigidbody2D rb;
    public Vector2 faceDirection = Vector2.left; // 默认朝左
    
    // 获取指定方向的防御值
    public float GetDefenseValue(Vector2 attackDirection) 
    {
        // 将攻击方向转换到敌人局部坐标系
        Vector2 localDir = transform.InverseTransformDirection(attackDirection);
        
        // 计算与面朝方向的夹角（使用点积判断方向）
        float dot = Vector2.Dot(faceDirection.normalized, attackDirection.normalized);
        
        // 方向判定阈值
        const float SIDE_THRESHOLD = 0.5f; // 45度左右分界
        
        if (dot > SIDE_THRESHOLD) {
            return front;  // 正面受击
        } 
        else if (dot < -SIDE_THRESHOLD) {
            return back;   // 背面受击
        }
        else {
            // 判断左右侧（使用叉积符号）
            float cross = faceDirection.x * attackDirection.y - faceDirection.y * attackDirection.x;
            return cross > 0 ? right : left;
        }
    }

    //##########测试用代码###############
    void OnDrawGizmosSelected()
    {
        // 绘制面朝方向（红色箭头）
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, faceDirection * 1f);
    }
}
}
