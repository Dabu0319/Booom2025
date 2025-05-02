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
        if (testFire)
        {
            testFire = false;
            // 手动测试角度，例如：FireSkill(45f);
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

    public void FireSkill(float ringLocalAngleZ)
    {
        if (isFiring) return;

        isFiring = true;
        fireTimer = laserDuration;

        // 正确角度 = Ring 的本地角度 + 修正 -90（因为激光默认朝右，Ring默认朝下）
        float finalAngle = -90f + ringLocalAngleZ;
        Quaternion laserRotation = Quaternion.Euler(0, 0, finalAngle);

        activeLaser = Instantiate(laserPrefab, transform.position, laserRotation);

        // 抵消 parent 缩放
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