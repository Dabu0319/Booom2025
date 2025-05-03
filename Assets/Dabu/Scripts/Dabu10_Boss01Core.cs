using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Dabu10_Boss01Core : MonoBehaviour
{
    private bool isFiring = false;
    private float fireTimer = 0f;

    private LineRenderer laserLine;
    private Vector3 fireDirection;

    [Header("Skill Settings")]
    public float laserDuration = 2f; // 激光持续时间
    public float maxLaserDistance = 20f;
    public LayerMask blockLayer;

    [Header("Test Fire Button")]
    public bool testFire = false;
    
    
    private float alignedAngleLocal;

    void Start()
    {
        laserLine = GetComponent<LineRenderer>();
        laserLine.enabled = false;
        laserLine.positionCount = 2;
    }

    void Update()
    {
        if (testFire)
        {
            testFire = false;
            FireSkill(45f); // 示例角度
        }

        if (isFiring)
        {
            fireTimer -= Time.deltaTime;
            UpdateLaser();

            if (fireTimer <= 0f)
            {
                EndSkill();
            }
        }
    }

    public void FireSkill(float alignedAngleLocalInput)
    {
        isFiring = true;
        fireTimer = laserDuration;

        alignedAngleLocal = alignedAngleLocalInput;

        laserLine.enabled = true;
        UpdateLaser(); // 可选：第一帧就画出来
    }
    private void UpdateLaser()
    {
        Vector3 start = transform.position;

        // Ring 默认开口朝下 → Vector2.down 是基准方向
        Quaternion totalRotation = transform.rotation * Quaternion.Euler(0, 0, alignedAngleLocal);
        fireDirection = totalRotation * Vector2.down;

        Vector3 end = start + (Vector3)(fireDirection * maxLaserDistance);

        RaycastHit2D hit = Physics2D.Raycast(start, fireDirection, maxLaserDistance, blockLayer);
        if (hit.collider != null)
        {
            end = hit.point;
            
            
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player1"))
            {
                if (hit.collider.GetComponent<player_death>() && !hit.collider.gameObject.GetComponent<player_death>().isDead)
                {
                    hit.collider.GetComponent<player_death>().Die();
                }
                else
                {
                    // 处理玩家死亡逻辑
                    Debug.Log("Player has no death script");
                }
            }
        }
        

        laserLine.SetPosition(0, start);
        laserLine.SetPosition(1, end);
    }


    private void EndSkill()
    {
        isFiring = false;
        laserLine.enabled = false;
    }
}