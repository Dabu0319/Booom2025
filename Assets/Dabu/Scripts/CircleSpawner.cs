using UnityEngine;

public class CircleSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject prefab; // 你的矩形Prefab
    public int numberOfObjects = 20; // 想摆多少个矩形
    public float radius = 5f; // 圆的半径

    void Start()
    {
        SpawnCircle();
    }

    void SpawnCircle()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            float angle = i * Mathf.PI * 2f / numberOfObjects; // 每个的角度
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            GameObject obj = Instantiate(prefab, transform.position + pos, Quaternion.identity, transform);

            // 让矩形朝着圆心
            obj.transform.up = (obj.transform.position - transform.position).normalized;
        }
    }
}
