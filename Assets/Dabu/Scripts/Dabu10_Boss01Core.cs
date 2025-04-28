using UnityEngine;

public class Dabu10_Boss01Core : MonoBehaviour
{
    private bool isFiring = false;
    private float currentRotation = 0f;

    public GameObject laserPrefab;
    private GameObject activeLaser;

    private Quaternion startRotation;

    [Header("Skill Settings")]
    public float rotateDuration = 0.5f; // 完成一圈需要多少秒
    private float rotateSpeedDuringSkill; // 自动算出来，每秒多少度

    [Header("Test Fire Button")]
    public bool testFire = false;

    void Update()
    {
        // 测试按钮，按一下发射一次
        if (testFire)
        {
            testFire = false;
            FireSkill();
        }

        if (isFiring)
        {
            float deltaRotation = rotateSpeedDuringSkill * Time.deltaTime;
            transform.Rotate(0, 0, -deltaRotation); // 快速旋转
            currentRotation += deltaRotation;

            if (currentRotation >= 360f)
            {
                EndSkill();
            }
        }
    }

    public void FireSkill()
    {
        if (isFiring) return; // 防止重复触发

        isFiring = true;
        currentRotation = 0f;
        startRotation = transform.rotation;

        // 根据时间算出转速
        rotateSpeedDuringSkill = 360f / rotateDuration; 

        activeLaser = Instantiate(laserPrefab, transform.position, transform.rotation, transform);
    }

    private void EndSkill()
    {
        isFiring = false;
        if (activeLaser != null)
            Destroy(activeLaser);
        transform.rotation = startRotation;
    }
}