using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    class _Mesh
    {
        protected List<Vector3> vertices;
        protected List<int> triangles;

        public Vector3[] getVertices() { return vertices.ToArray(); }
        public int[] getTriangles() { return triangles.ToArray(); }

        public _Mesh()
        {
            vertices = new List<Vector3>();
            triangles = new List<int>();
        }
    }

    class _Grid : _Mesh
    {
        public _Grid(int rows, int cols)
        {
            for (int y = 0; y <= cols; ++y)
            {
                for (int x = 0; x <= rows; ++x)
                {
                    Vector3 tmp = new Vector3(x, y, 0.0f);
                    vertices.Add(tmp);
                }
            }

            for (int y = 0; y < cols; ++y)
            {
                for (int x = 0; x < rows; ++x)
                {
                    triangles.Add(y * (rows + 1) + x);
                    triangles.Add((y + 1) * (rows + 1) + x);
                    triangles.Add(y * (rows + 1) + x + 1);
                    triangles.Add(y * (rows + 1) + x + 1);
                    triangles.Add((y + 1) * (rows + 1) + x);
                    triangles.Add((y + 1) * (rows + 1) + x + 1);
                }
            }
        }
    }

    class Circle : _Mesh
    {
        public Circle(int p, int y, bool bot = false)
        {
            double inc = 2 * Math.PI / p;
            Vector3 Center = new Vector3(0.0f, y, 0.0f);
            vertices.Add(Center);

            for (double a = -Math.PI; a <= Math.PI; a += inc)
            {
                Vector3 Point = new Vector3((float)Math.Cos(a), y, (float)Math.Sin(a));
                vertices.Add(Point);
            }

            if (bot)
            {
                for (int i = 1; i <= p; i++)
                {
                    triangles.Add(0);
                    triangles.Add(i);
                    triangles.Add(i + 1);
                }
            }
            else
            {
                for (int i = 1; i <= p; i++)
                {
                    triangles.Add(0);
                    triangles.Add(i + 1);
                    triangles.Add(i);
                }
            }

        }
    }

    class Cylinder : _Mesh
    {
        public Circle BottomCircle;
        public Circle TopCircle;
        public Cylinder(int p)
        {
            BottomCircle = new Circle(p, 0, true);
            TopCircle = new Circle(p, 5, false);

            int t = 0;
            foreach (Vector3 vertex in BottomCircle.getVertices())
            {
                vertices.Add(vertex);
            }
            foreach (int triangle in BottomCircle.getTriangles())
            {
                triangles.Add(triangle + t);
            }
            t = vertices.Count;
            foreach (Vector3 vertex in TopCircle.getVertices())
            {
                vertices.Add(vertex);
            }
            foreach (int triangle in TopCircle.getTriangles())
            {
                triangles.Add(triangle + t);
            }

            int max = BottomCircle.getVertices().Length - 1;
            for (int i = 1; i < max; i++)
            {
                triangles.Add(i);
                triangles.Add(i + (max + 1));
                triangles.Add(i + (max + 1) + 1);
                triangles.Add(i);
                triangles.Add(i + (max + 1) + 1);
                triangles.Add(i + 1);
            }

        }
    }

    class Sphere : _Mesh
    {
        public Sphere(int p)
        {
            double inc = 2 * Math.PI / p;
            int n = 0;
            for (double b = -Math.PI / 2.0f + inc; b < Math.PI / 2.0f; b += inc)
            {
                vertices.Add(new Vector3(0, (float)Math.Sin(b), 0));
                for (double a = -Math.PI; a <= Math.PI; a += inc)
                {
                    Vector3 Point = new Vector3((float)Math.Cos(a) * (float)Math.Cos(b), (float)Math.Sin(b), (float)Math.Sin(a) * (float)Math.Cos(b));
                    vertices.Add(Point);
                }
                // Drawing inner circles
                //for (int i = 0; i < p + 1; i++)
                //{
                //    triangles.Add(0 + n * (p + 2));
                //    triangles.Add(i + 1 + n * (p + 2));
                //    triangles.Add(i + n * (p + 2));
                //}
                n++;
            }
            for (int i = 0; i < vertices.Count / (p+2) - 1; i++)
            {
                for (int j = i*(p+2)+1; j < (i+1)*(p+2)-1; j++)
                {
                    Debug.Log(j);
                    triangles.Add(j);
                    triangles.Add(j + (p + 2));
                    triangles.Add(j + (p + 2) + 1);
                    triangles.Add(j);
                    triangles.Add(j + (p + 2) + 1);
                    triangles.Add(j + 1);
                }
            }

            // Top
            int top = vertices.Count / (p + 2) - 1;
            vertices.Add(new Vector3(0, 1, 0));
            for (int i = top * (p + 2) + 1; i < (top + 1) * (p + 2); i++)
            {
                triangles.Add(vertices.Count - 1);
                triangles.Add(i + 1);
                triangles.Add(i);
            }

            // Bottom
            int bot = 0;
            vertices.Add(new Vector3(0, -1, 0));
            for (int i = 0; i < p+2; i++)
            {
                triangles.Add(vertices.Count - 1);
                triangles.Add(i);
                triangles.Add(i + 1);
            }
        }
    }
}
