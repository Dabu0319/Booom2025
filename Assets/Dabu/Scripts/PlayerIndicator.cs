using UnityEngine;

public class PlayerIndicator : MonoBehaviour
{
    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 lineScreenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 dir = mouseScreenPos - lineScreenPos;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);  // 因为初始是向上
    }
}
