using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Sphere
{
    public Sphere() { center = new Vector3(); radius = 1.0f; }
    public Sphere(Vector3 c, float r) { center = c; radius = r; }
    public Vector3 center;
    public float radius;
}

public class Voxels : MonoBehaviour
{
    public float sqr(float n) { return n * n; }
 
    public GameObject prefab;
    List<GameObject> cubes = new List<GameObject>();
    List<float> values = new List<float>();

    public void MinMax(List<Sphere> spheres, ref Vector3 min, ref Vector3 max)
    {
        min.Set(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        max.Set(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        foreach (Sphere s in spheres)
        {
            if (min.x > s.center.x - s.radius) { min.x = s.center.x - s.radius; }
            if (min.y > s.center.y - s.radius) { min.y = s.center.y - s.radius; }
            if (min.z > s.center.z - s.radius) { min.z = s.center.z - s.radius; }
            if (max.x < s.center.x + s.radius) { max.x = s.center.x + s.radius; }
            if (max.y < s.center.y + s.radius) { max.y = s.center.y + s.radius; }
            if (max.z < s.center.z + s.radius) { max.z = s.center.z + s.radius; }
        }
    }

    public void Draw(List<Sphere> spheres, ref List<GameObject> cubes, ref List<float> values, float taille = 0.1f)
    {
        Vector3 min = new Vector3();
        Vector3 max = new Vector3();
        MinMax(spheres, ref min, ref max);

        for (float z = min.z + taille / 2f; z < max.z; z += taille)
        {
            for (float y = min.y + taille / 2f; y < max.y; y += taille)
            {
                for (float x = min.x + taille / 2f; x < max.x; x += taille)
                {
                    foreach (Sphere s in spheres)
                    {
                        if (sqr(x - s.center.x) + sqr(y - s.center.y) + sqr(z - s.center.z) - sqr(s.radius) < 0)
                        {
                            cubes.Add(Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity));
                            cubes[cubes.Count - 1].transform.localScale *= taille;
                            float a = max.x - (max.x - x) + (x - min.x);
                            float b = max.y - (max.y - y) + (y - min.y);
                            float c = max.z - (max.z - z) + (z - min.z);
                            values.Add(Mathf.Abs(a + b + c));
                        }
                    }
                }
            }
        }
    }

    public void Intersection(List<Sphere> spheres, float taille = 0.1f)
    {
        Vector3 min = new Vector3();
        Vector3 max = new Vector3();
        MinMax(spheres, ref min, ref max);

        int count = 0;
        GameObject go;
        for (float z = min.z + taille / 2f; z < max.z; z += taille)
        {
            for (float y = min.y + taille / 2f; y < max.y; y += taille)
            {
                for (float x = min.x + taille / 2f; x < max.x; x += taille)
                {
                    foreach (Sphere s in spheres)
                    {
                        if (sqr(x - s.center.x) + sqr(y - s.center.y) + sqr(z - s.center.z) - sqr(s.radius) < 0)
                        {
                            count++;
                        }
                    }
                    if (count == spheres.Count)
                    {
                        go = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
                        go.transform.localScale *= taille;
                    }
                    count = 0;
                }
            }
        }
    }

    public void Minus(Sphere a, Sphere b, float taille = 0.1f)
    {
        Vector3 min = new Vector3();
        Vector3 max = new Vector3();
        MinMax(new List<Sphere> { a, b }, ref min, ref max);

        int count = 0;
        GameObject go;
        for (float z = min.z + taille / 2f; z < max.z; z += taille)
        {
            for (float y = min.y + taille / 2f; y < max.y; y += taille)
            {
                for (float x = min.x + taille / 2f; x < max.x; x += taille)
                {
                    if (sqr(x - a.center.x) + sqr(y - a.center.y) + sqr(z - a.center.z) - sqr(a.radius) < 0)
                    {
                        count++;
                    }
                    if (sqr(x - b.center.x) + sqr(y - b.center.y) + sqr(z - b.center.z) - sqr(b.radius) < 0)
                    {
                        count--;
                    }
                    if (count == 1)
                    {
                        go = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
                        go.transform.localScale *= taille;
                    }
                    count = 0;
                }
            }
        }
    }

    public void MakeDiscrete(ref List<GameObject> cubes, ref List<float> values, float rule)
    {
        for (int i=0; i < cubes.Count; ++i)
        {
            if (values[i] < rule)
            {
                Destroy(cubes[i]);
            }
        }
    }

    void Start()
    {
        List<Sphere> spheres = new List<Sphere>();

        //Draw(spheres, ref cubes, ref values);
        //Intersection(spheres);
        //Minus(spheres[0], spheres[1]);

        //Intersection
        spheres.Add(new Sphere(new Vector3(2.5f, 0f, 0f), 1f));
        spheres.Add(new Sphere(new Vector3(2.5f, 0.5f, 0f), 1f));
        Intersection(spheres);
        spheres.Clear();

        //Substraction
        spheres.Add(new Sphere(new Vector3(0f, 0.5f, 0f), 1f));
        spheres.Add(new Sphere(new Vector3(0f, 0f, 0f), 1f));
        Minus(spheres[1], spheres[0]);
        spheres.Clear();

        //Discrete
        spheres.Add(new Sphere(new Vector3(-2.5f, 0f, 0f), 1f));
        Draw(spheres, ref cubes, ref values);
        MakeDiscrete(ref cubes, ref values, 2.0f);
        spheres.Clear();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
