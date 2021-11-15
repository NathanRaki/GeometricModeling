using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{

    private Mesh _mesh;

    private Vector3[] _vertices;
    private int[] _triangles;
    private Color[] _colors;

    private float _minHeight;
    private float _maxHeight;

    public int XSize = 20;
    public int ZSize = 20;

    public float Variation = 1f; //affects how much height variation there will be ]0;1[
    public float Magnitude = 1f; //affects how high the peaks will be ]0;inf[

    public Gradient TerrainGradient;

    public Graphe G;


    public class Graphe
    {
        public List<Vector3> vertices;
        public List<List<float>> adjMatrix;

        public Graphe()
        {
            vertices = new List<Vector3>();
            adjMatrix = new List<List<float>>();
        }
    }

    private List<int> GenerateRoad(int start, int end)
    {
        int[] pred = Dijkstra(start);
        int cursor = end;
        List<int> Road = new List<int>();
        while (cursor != -1)
        {
            Road.Add(cursor);
            cursor = pred[cursor];
        }
        Road.Reverse();
        return Road;
    }

    private void GenerateGraphe()
    {
        for (int i = 0; i < _vertices.Length; i++)
            G.vertices.Add(_vertices[i]);

        // Init at 0
        for (int i = 0; i < (XSize+1)*(ZSize+1); i++)
        {
            G.adjMatrix.Add(new List<float>());
            for (int j = 0; j < (XSize+1)*(ZSize+1); j++)
            {
                G.adjMatrix[G.adjMatrix.Count - 1].Add(0f);
            }
        }

        for (int i = 0; i <= XSize; i++)
        {
            for (int j = 0; j <= ZSize; j++)
            {
                int a = i * (XSize + 1) + j;
                if (i != XSize)
                {
                    int b = (i + 1) * (XSize + 1) + j;
                    G.adjMatrix[a][b] = Mathf.Abs(_vertices[a].y - _vertices[b].y);
                    G.adjMatrix[b][a] = Mathf.Abs(_vertices[a].y - _vertices[b].y);
                }
                if (j != ZSize)
                {
                    int c = i * (XSize + 1) + (j + 1);
                    G.adjMatrix[a][c] = Mathf.Abs(_vertices[a].y - _vertices[c].y);
                    G.adjMatrix[c][a] = Mathf.Abs(_vertices[a].y - _vertices[c].y);
                }
            }
        }

        for (int i = 0; i < G.adjMatrix.Count; i++)
        {
            for (int j = 0; j < G.adjMatrix[i].Count; j++)
            {
                if (G.adjMatrix[i][j] > Magnitude / 4f)
                    G.adjMatrix[i][j] = 0f;
            }
        }
    }



    private int[] Dijkstra(int start)
    {
        List<int> poids = new List<int>();
        float[] distance = new float[(XSize+1) * (ZSize+1)];
        int[] pred = new int[(XSize + 1) * (ZSize + 1)];
        for (int i = 0; i < (XSize + 1) * (ZSize + 1); ++i)
        {
            pred[i] = -1;
        }

        for (int i = 0; i < (XSize + 1) * (ZSize + 1); ++i)
        {
            distance[i] = float.PositiveInfinity;
        }
        distance[start] = 0;
        int cursor = start;

        KeyValuePair<float, int> min;
        while (poids.Count < (XSize + 1) * (ZSize + 1))
        {
            min = new KeyValuePair<float, int> (float.PositiveInfinity, 0 );
            for (int i = 0; i < (XSize + 1) * (ZSize + 1); ++i)
            {
                if (!poids.Contains(i))
                    if (distance[i] < min.Key) { min = new KeyValuePair<float, int>(distance[i], i); break; }
            }

            cursor = min.Value;
            poids.Add(cursor);

            for (int i = 0; i < (XSize + 1) * (ZSize + 1); ++i)
            {
                if (!poids.Contains(i) && G.adjMatrix[cursor][i] != 0)
                {
                    if (distance[i] > distance[cursor] + G.adjMatrix[cursor][i])
                    {
                        distance[i] = distance[cursor] + G.adjMatrix[cursor][i];
                        pred[i] = cursor;
                    }
                }
            }
        }
        return pred;
    }



    void Start()
    {
        _mesh = new Mesh();
        G = new Graphe();
        GetComponent<MeshFilter>().mesh = _mesh;

        CreateShape();
        UpdateMesh();
        GenerateGraphe();
        //GetComponent<RouteGenerator>().InitVertices(_vertices);
        
    }

    void CreateShape()
    {
        _vertices = new Vector3[(XSize + 1) * (ZSize + 1)];

        for (int i = 0, z = 0; z <= ZSize; z++)
        {
            for (int x = 0; x <= XSize; x++)
            {
                float y = Mathf.PerlinNoise(x * Variation, z * Variation) * Magnitude;
                _vertices[i] = new Vector3(x, y, z);

                if (y > _maxHeight)
                    _maxHeight = y;
                if (y < _minHeight)
                    _minHeight = y;

                i++;
            }
        }

        _triangles = new int[XSize * ZSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < ZSize; z++)
        {
            for (int x = 0; x < XSize; x++)
            {
                _triangles[tris + 0] = vert + 0;
                _triangles[tris + 1] = vert + XSize + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1;
                _triangles[tris + 4] = vert + XSize + 1;
                _triangles[tris + 5] = vert + XSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        _colors = new Color[_vertices.Length];

        for (int i = 0, z = 0; z <= ZSize; z++)
        {
            for (int x = 0; x <= XSize; x++)
            {
                float height = Mathf.InverseLerp(_minHeight, _maxHeight, _vertices[i].y);
                _colors[i] = TerrainGradient.Evaluate(height);
                i++;
            }
        }
    }

    void UpdateMesh()
    {
        _mesh.Clear();

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.colors = _colors;

        _mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (_vertices == null) return;

        for (int i = 0; i < _vertices.Length; i++)
        {
            //UnityEditor.Handles.Label(_vertices[i] + Vector3.up, $"{i}");
            Gizmos.DrawSphere(_vertices[i], .1f);
        }


        //for (int i = 0; i < G.adjMatrix.Count; i++)
        //{
        //    for (int j = 0; j < G.adjMatrix[i].Count; j++)
        //    {

        //        if (G.adjMatrix[i][j] == 0f) { continue; }
        //        if (G.adjMatrix[i][j] < Magnitude / 4f)
        //        {
        //            if (G.adjMatrix[i][j] > (Magnitude / 4f) * 0.9f)
        //                Gizmos.color = new Color(1f, 0.5f, 0f);
        //            Gizmos.DrawLine(G.vertices[i], G.vertices[j]);
        //            Gizmos.color = Color.white;
        //        }
        //    }
        //}
        //Gizmos.color = Color.blue;

        //List<int> road2 = GenerateRoad(49, 32);
        //for (int i = 0; i < road2.Count - 1; i++)
        //{
        //    Gizmos.DrawLine(G.vertices[road2[i]], G.vertices[road2[i + 1]]);
        //}

        //List<int> road3 = GenerateRoad(82, 99);
        //for (int i = 0; i < road3.Count - 1; i++)
        //{
        //    Gizmos.DrawLine(G.vertices[road3[i]], G.vertices[road3[i + 1]]);
        //}

        Gizmos.color = Color.red;

        List<int> road1 = GenerateRoad(3, 118);
        for (int i = 0; i < road1.Count - 1; i++)
        {
            Gizmos.DrawLine(G.vertices[road1[i]] + Vector3.up * 0.1f, G.vertices[road1[i + 1]] + Vector3.up * 0.1f);
        }

        
    }

}
