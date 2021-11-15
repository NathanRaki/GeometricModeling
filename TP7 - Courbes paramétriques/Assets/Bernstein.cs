using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bernstein : MonoBehaviour
{
    [SerializeField]
    private List<Transform> nodes = new List<Transform>();

    [SerializeField]
    private int precision = 100;

    private int Fact(int n)
    {
        if (n <= 1) { return 1; }
        return n * Fact(n - 1);
    }

    private float GetBernstein(int n, int i, float t)
    {
        return (Fact(n) / (Fact(i) * Fact(n - i))) * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
    }

    void Start()
    {
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            Gizmos.DrawLine(nodes[i].position, nodes[i + 1].position);
        }

        Gizmos.color = Color.green;
        Vector3 prev = nodes[0].position;
        float step = 1f / (float)precision;
        for (float t = 0f; t <= 1f; t += step)
        {
            Vector3 Q = Vector3.zero;
            Debug.Log(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
            {
                Debug.Log(nodes[i].position);
                Q += nodes[i].position * GetBernstein(nodes.Count - 1, i, t);
            }
            Gizmos.DrawLine(prev, Q);
            prev = Q;
        }
        Gizmos.DrawLine(prev, nodes[nodes.Count - 1].position);
    }
}
