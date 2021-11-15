using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hermite : MonoBehaviour
{
    private Transform Root;

    [SerializeField]
    private Transform PointA, PointB;

    [SerializeField]
    private Transform CursorA, CursorB;

    [SerializeField]
    private int precision = 100;

    [SerializeField]
    public float scalar;

    void Start()
    {
        Root = GetComponent<Transform>();
    }

    private void OnDrawGizmos()
    {
        if (PointA == null || PointB == null || CursorA == null || CursorB == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(PointA.position, CursorA.position - PointA.position);
        Gizmos.DrawRay(PointB.position, CursorB.position - PointB.position);

        Gizmos.color = Color.green;
        Vector3 P0 = PointA.position;
        Vector3 P1 = PointB.position;
        Vector3 V0 = CursorA.position - PointA.position;
        Vector3 V1 = PointB.position - CursorB.position;
        Vector3 prev = PointA.position;

        V0 *= scalar;
        V1 *= scalar;

        float step = 1.0f / precision;
        for (float u = 0.0f; u < 1.0f; u += step)
        {
            Vector3 Point = new Vector3(0.0f, 0.0f, 0.0f);
            Point += P0 * (2.0f * Mathf.Pow(u, 3.0f) - 3.0f * Mathf.Pow(u, 2.0f) + 1.0f);
            Point += P1 * (-2.0f * Mathf.Pow(u, 3.0f) + 3.0f * Mathf.Pow(u, 2.0f));
            Point += V0 * (Mathf.Pow(u, 3.0f) - 2.0f * Mathf.Pow(u, 2.0f) + u);
            Point += V1 * (Mathf.Pow(u, 3.0f) - Mathf.Pow(u, 2.0f));
            Gizmos.DrawLine(prev, Point);
            prev = Point;
        }
    }
}
