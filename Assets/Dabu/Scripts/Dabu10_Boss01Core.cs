using UnityEngine;

public class Dabu10_Boss01Core : MonoBehaviour
{
    private bool isFiring = false;
    private float fireTimer = 0f;

    public GameObject laserPrefab;
    private GameObject activeLaser;

    [Header("Skill Settings")]
    public float laserDuration = 2f; // 激光持续时间（不旋转）

    [Header("Test Fire Button")]
    public bool testFire = false;

    void Update()
    {
        // 测试按钮触发
        if (testFire)
        {
            testFire = false;
            //FireSkill();
        }

        if (isFiring)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                EndSkill();
            }
        }
    }

    public void FireSkill(float ringAngleZ)
    {
        if (isFiring) return;

        isFiring = true;
        fireTimer = laserDuration;

        // Ring视觉开口为 z，Laser默认向右 ⇒ 修正 -90°
        float finalAngle = ringAngleZ - 90f;
        Quaternion laserRotation = Quaternion.Euler(0, 0, finalAngle);

        activeLaser = Instantiate(laserPrefab, transform.position, laserRotation);

        Vector3 parentScale = transform.lossyScale;
        Vector3 inverseScale = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f / parentScale.z
        );

        activeLaser.transform.SetParent(transform, worldPositionStays: true);
        activeLaser.transform.localScale = inverseScale;
    }
    private void EndSkill()
    {
        isFiring = false;

        if (activeLaser != null)
            Destroy(activeLaser);
    }
}