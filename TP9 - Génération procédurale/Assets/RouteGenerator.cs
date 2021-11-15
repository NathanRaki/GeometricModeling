using System.Collections.Generic;
using UnityEngine;

public class RouteGenerator : MonoBehaviour
{

    private LineRenderer _lr;

    private Vector3[] _vertices;
    private List<Vector3> _route;

    public int RouteSize;

    private void Start()
    {
        _lr = GetComponent<LineRenderer>();
    }

    public void InitVertices(Vector3[] vertices)
    {
        _vertices = vertices;
        Debug.Log("RouteGenerator : " + _vertices.Length + " vertices");

        //RandomRoute();
    }

    private void RandomRoute()
    {
        _route = new List<Vector3>();

        for (int i = 0; i < RouteSize; i++)
        {
            _route.Add(_vertices[Random.Range(0, _vertices.Length)]);
        }

        _lr.positionCount = _route.Count;
        _lr.SetPositions(_route.ToArray());
    }

}
