using UnityEngine;

namespace TangKK
{

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab;       // 拖入子弹预制体
    public Transform firePoint;           // 开火位置
    public float fireInterval = 2f;       // 发射间隔
    public Vector2 fireDirection = Vector2.right;  // 发射方向
    public Transform target; // 玩家

    private float fireTimer;

    private void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            Fire();
            fireTimer = 0f;
        }
    }

        void Fire()
        {
            Vector2 directionToPlayer = (target.position - firePoint.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript != null)
            {
                bulletScript.direction = directionToPlayer;
            }
        }
}
}