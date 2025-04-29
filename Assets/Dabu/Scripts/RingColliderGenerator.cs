using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PolygonCollider2D))]
public class RingColliderGenerator : MonoBehaviour
{
    public float outerRadius = 5f;
    public float innerRadius = 4f;
    public int segments = 60;

    void Start()
    {
        PolygonCollider2D poly = GetComponent<PolygonCollider2D>();
        List<Vector2> points = new List<Vector2>();

        // 外圈（顺时针）
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            points.Add(new Vector2(Mathf.Cos(angle) * outerRadius, Mathf.Sin(angle) * outerRadius));
        }

        // 内圈（逆时针）
        for (int i = segments - 1; i >= 0; i--)
        {
            float angle = i * Mathf.PI * 2 / segments;
            points.Add(new Vector2(Mathf.Cos(angle) * innerRadius, Mathf.Sin(angle) * innerRadius));
        }

        poly.pathCount = 1;
        poly.SetPath(0, points.ToArray());
    }
}