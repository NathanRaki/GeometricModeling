using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier : MonoBehaviour
{

    [SerializeField]
    private Transform PointA;

    [SerializeField]
    private Transform PointB;

    [SerializeField]
    private Transform A1;

    [SerializeField]
    private Transform A2;

    [SerializeField]
    private Transform B1;

    [SerializeField]
    private Transform B2;


    Vector3 GetBezier(Vector3 A, Vector3 B, Vector3 C, float t)
    {
        Vector3 LerpAB = Vector3.Lerp(A, B, t);
        Vector3 LerpBC = Vector3.Lerp(B, C, t);
        return Vector3.Lerp(LerpAB, LerpBC, t);
    }

    private void Update()
    {
        Vector3 posA = PointA.position - A1.position;
        A2.position = PointA.position + posA;
        Vector3 posB = PointB.position - B1.position;
        B2.position = PointB.position + posB;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(A1.position, A2.position);
        Gizmos.DrawLine(B1.position, B2.position);

        Gizmos.color = Color.green;
        float it = 100;
        Vector3 prev = Vector3.Lerp(GetBezier(PointA.position, A1.position, B1.position, 0), GetBezier(A1.position, B1.position, PointB.position, 0), 0);

        for (int i = 0; i < it; i++)
        {

            Vector3 pos = Vector3.Lerp(GetBezier(PointA.position, A1.position, B1.position, i / it), GetBezier(A1.position, B1.position, PointB.position, i / it), i / it);
            Gizmos.DrawLine(prev, pos);
            prev = pos;
        }
    }
}
