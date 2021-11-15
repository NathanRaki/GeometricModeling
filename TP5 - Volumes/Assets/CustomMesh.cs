using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public Cell() { vertices = new List<Vector3>(); }
    public bool Contains(Vector3 vertex)
    {
        if (vertex.x < min.x) return false;
        if (vertex.y < min.y) return false;
        if (vertex.z < min.z) return false;
        if (vertex.x > max.x) return false;
        if (vertex.y > max.y) return false;
        if (vertex.z > max.z) return false;
        return true;
    }

    public List<Vector3> vertices;
    public Vector3 min;
    public Vector3 max;
    public Vector3 mean;
}

public class CustomMesh : MonoBehaviour
{
    public Material Mat;
    public string file;

    protected int nbVertices;
    protected int nbFaces;
    protected List<Vector3> vertices = new List<Vector3>();
    protected List<int> triangles = new List<int>();
    protected Vector3[] normals;

    protected Vector3 gridMin;
    protected Vector3 gridMax;
    List<Cell> cells;

    void loadOFF(string filePath, bool trace = false)
    {
        string[] lines = File.ReadAllLines(filePath);
        List<string[]> terms = new List<string[]>();
        foreach (string line in lines)
        {
            terms.Add(line.Split(' '));
        }

        nbVertices = Int32.Parse(terms[1][0]);
        vertices = new List<Vector3>();
        if (trace) Debug.Log("Nombre de sommets: " + nbVertices);

        nbFaces = Int32.Parse(terms[1][1]);
        if (trace) Debug.Log("Nombre de faces: " + nbFaces);

        for (int i = 0; i < nbVertices; ++i)
        {
            float x = float.Parse(terms[i + 2][0], CultureInfo.InvariantCulture);
            float y = float.Parse(terms[i + 2][1], CultureInfo.InvariantCulture);
            float z = float.Parse(terms[i + 2][2], CultureInfo.InvariantCulture);
            vertices.Add(new Vector3(x, y, z));
            if (trace) Debug.Log("Sommet " + i + ": " + vertices[i].ToString("G", CultureInfo.InvariantCulture));
        }
        for (int i = 0; i < nbFaces; ++i)
        {
            int a = Int32.Parse(terms[i + nbVertices + 2][1]);
            int b = Int32.Parse(terms[i + nbVertices + 2][2]);
            int c = Int32.Parse(terms[i + nbVertices + 2][3]);
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            if (trace) Debug.Log("Face " + i + ": " + a + ", " + b + ", " + c);
        }
        simplifyMesh(10);
        center();
        normalize();
        generateNormals();
        export();
    }

    void center()
    {
        Vector3 center = new Vector3();
        foreach(Vector3 v in vertices)
        {
            center += v;
        }
        center /= vertices.Count;
        for (int i = 0; i < vertices.Count; ++i)
        {
            vertices[i] -= center;
        }
    }

    void normalize()
    {
        Vector3 max = new Vector3(0.0f, 0.0f, 0.0f);
        foreach(Vector3 v in vertices)
        {
            if (v.magnitude > max.magnitude) { max = v; }
        }
        for (int i = 0; i < vertices.Count; ++i)
        {
            vertices[i] /= max.magnitude;
        }
    }

    void generateNormals()
    {
        normals = new Vector3[nbVertices];
        Array.Clear(normals, 0, normals.Length);
        float x, y, z;
        for(int i = 0; i < triangles.Count; i += 3)
        {
            x = vertices[triangles[i + 1]].x - vertices[triangles[i]].x;
            y = vertices[triangles[i + 1]].y - vertices[triangles[i]].y;
            z = vertices[triangles[i + 1]].z - vertices[triangles[i]].z;
            Vector3 a = new Vector3(x, y, z);
            x = vertices[triangles[i + 2]].x - vertices[triangles[i + 1]].x;
            y = vertices[triangles[i + 2]].y - vertices[triangles[i + 1]].y;
            z = vertices[triangles[i + 2]].z - vertices[triangles[i + 1]].z;
            Vector3 b = new Vector3(x, y, z);
            normals[triangles[i]] += Vector3.Cross(a, b);
            normals[triangles[i+1]] += Vector3.Cross(a, b);
            normals[triangles[i+2]] += Vector3.Cross(a, b);
        }
        for (int i = 0; i < normals.Length; ++i)
        {
            normals[i] = normals[i].normalized;
        }
    }

    void export()
    {
        List<string> lines = new List<string>();
        lines.Add("OFF");
        lines.Add(nbVertices + " " + nbFaces + " 0");
        foreach(Vector3 v in vertices)
        {
            lines.Add(v.x.ToString("G", CultureInfo.InvariantCulture) + " " + v.y.ToString("G", CultureInfo.InvariantCulture) + " " + v.z.ToString("G", CultureInfo.InvariantCulture));
        }
        for (int i = 0; i < triangles.Count; i += 3)
        {
            lines.Add("3 " + triangles[i] + " " + triangles[i + 1] + " " + triangles[i + 2]);
        }
        File.WriteAllLines("srcDST/" + file, lines.ToArray());
    }

    void simplifyMesh(int r)
    {
        // Finding min and max to surround the mesh
        gridMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        gridMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        foreach (Vector3 vertex in vertices)
        {
            if (vertex.x < gridMin.x) { gridMin.x = vertex.x; }
            if (vertex.y < gridMin.y) { gridMin.y = vertex.y; }
            if (vertex.z < gridMin.z) { gridMin.z = vertex.z; }
            if (vertex.x > gridMax.x) { gridMax.x = vertex.x; }
            if (vertex.y > gridMax.y) { gridMax.y = vertex.y; }
            if (vertex.z > gridMax.z) { gridMax.z = vertex.z; }
        }

        cells = new List<Cell>();

        float xStep = (gridMax.x - gridMin.x) / r;
        float yStep = (gridMax.y - gridMin.y) / r;
        float zStep = (gridMax.z - gridMin.z) / r;
        for (float z = gridMin.z; z < gridMax.z; z += zStep)
        {
            for (float y = gridMin.y; y < gridMax.y; y += yStep)
            {
                for (float x = gridMin.x; x < gridMax.x; x += xStep)
                {
                    cells.Add(new Cell());
                    cells[cells.Count - 1].min = new Vector3(x, y, z);
                    cells[cells.Count - 1].max = new Vector3(x + xStep, y + yStep, z + zStep);
                }
            }
        }


        foreach (Vector3 vertex in vertices)
        {
            foreach (Cell cell in cells)
            {
                if (cell.Contains(vertex))
                {
                    cell.vertices.Add(vertex);
                    break;
                }
            }
        }

        Debug.Log(cells.Count);

        foreach (Cell cell in cells)
        {
            Debug.Log(cell.vertices.Count);
            if (cell.vertices.Count == 0) { break; }
            cell.mean = new Vector3();
            foreach (Vector3 vertex in cell.vertices)
            {
                cell.mean += vertex;
            }
            cell.mean /= cell.vertices.Count;
        }

        List<Vector3> new_vertices = new List<Vector3>();
        foreach (Cell cell in cells)
        {
            if (cell.vertices.Count == 0) { break; }
            new_vertices.Add(cell.mean);
            foreach(Vector3 vertex in cell.vertices)
            {
                int index = vertices.IndexOf(vertex);
                for (int i = 0; i < triangles.Count; i++)
                {
                    if (triangles[i] == index) { triangles[i] = new_vertices.Count - 1; }
                }
            }
        }
        vertices = new_vertices;

        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    foreach(Cell cell in cells)
        //    {
        //        if (cell.vertices.Contains(vertices[i])) { vertices[i] = cell.mean; }
        //    }
        //}

        //List<Vector3> new_vertices = new List<Vector3>();
        //List<int> new_triangles = new List<int>();

        //for (int i = 0; i < triangles.Count; i += 3)
        //{
        //    Vector3 p1 = vertices[triangles[i]];
        //    Vector3 p2 = vertices[triangles[i+1]];
        //    Vector3 p3 = vertices[triangles[i+2]];
        //    int count = 0;
        //    foreach(Cell cell in cells)
        //    {
        //        count = 0;
        //        if (cell.vertices.Contains(p1)) { count++; }
        //        if (cell.vertices.Contains(p2)) { count++; }
        //        if (cell.vertices.Contains(p3)) { count++; }
        //        if (count > 1) { break; }
        //    }
        //    if (count > 1) { continue; }
        //    else
        //    {
        //        new_triangles.Add(triangles[i]);
        //        new_triangles.Add(triangles[i+1]);
        //        new_triangles.Add(triangles[i+2]);
        //    }
        //}


        //List<int> distinct = new_triangles.Distinct().ToList();

        //foreach(int i in distinct)
        //{
        //    Vector3 point = vertices[i];
        //    Vector3 new_point = new Vector3();
        //    foreach(Cell cell in cells)
        //    {
        //        if (cell.vertices.Count == 0) { continue; }
        //        if (cell.vertices.Contains(point)) { new_point = cell.mean; break; }
        //    }
        //    new_vertices.Add(new_point);
        //    for (int j = 0; j < new_triangles.Count; ++j)
        //    {
        //        if (new_triangles[j] == i) { new_triangles[j] = new_vertices.Count - 1; }
        //    }
        //}

        //Debug.Log(triangles.Max());
        //vertices = new_vertices;
        //triangles = new_triangles;
    }

    // Start is called before the first frame update
    void Start()
    {
        Mesh msh = new Mesh();
        loadOFF("srcOFF/" + file);
        msh.vertices = vertices.ToArray();
        msh.triangles = triangles.ToArray();
        msh.normals = normals;

        transform.gameObject.GetComponent<MeshFilter>().mesh = msh;
        transform.gameObject.GetComponent<MeshRenderer>().material = Mat;

        //vertices.Add(new Vector3(-1, -3, -1));
        //vertices.Add(new Vector3(-1, -3, 1));
        //vertices.Add(new Vector3(1, -3, -1));
        //vertices.Add(new Vector3(1, -3, 1));

        //vertices.Add(new Vector3(-1, -2, -1));
        //vertices.Add(new Vector3(-1, -2, 1));
        //vertices.Add(new Vector3(1, -2, -1));
        //vertices.Add(new Vector3(1, -2, 1));

        //vertices.Add(new Vector3(-1, -1, -1));
        //vertices.Add(new Vector3(-1, -1, 1));
        //vertices.Add(new Vector3(1, -1, -1));
        //vertices.Add(new Vector3(1, -1, 1));

        //simplifyMesh(3);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if (cells != null)
        {
            foreach (Cell cell in cells)
            {
                Vector3 p1 = new Vector3(cell.min.x, cell.min.y, cell.min.z);
                Vector3 p2 = new Vector3(cell.min.x, cell.min.y, cell.max.z);
                Vector3 p3 = new Vector3(cell.min.x, cell.max.y, cell.max.z);
                Vector3 p4 = new Vector3(cell.max.x, cell.min.y, cell.min.z);
                Vector3 p5 = new Vector3(cell.max.x, cell.max.y, cell.min.z);
                Vector3 p6 = new Vector3(cell.min.x, cell.max.y, cell.min.z);
                Vector3 p7 = new Vector3(cell.max.x, cell.min.y, cell.max.z);
                Vector3 p8 = new Vector3(cell.max.x, cell.max.y, cell.max.z);
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p1, p4);
                Gizmos.DrawLine(p1, p6);
                Gizmos.DrawLine(p2, p7);
                Gizmos.DrawLine(p2, p3);
                Gizmos.DrawLine(p3, p8);
                Gizmos.DrawLine(p3, p6);
                Gizmos.DrawLine(p4, p7);
                Gizmos.DrawLine(p4, p5);
                Gizmos.DrawLine(p5, p8);
                Gizmos.DrawLine(p5, p6);
                Gizmos.DrawLine(p7, p8);
            }
            foreach (Cell cell in cells)
            {
                Gizmos.DrawSphere(cell.mean, (gridMax - gridMin).magnitude / 64);
            }
        }
    }
}
