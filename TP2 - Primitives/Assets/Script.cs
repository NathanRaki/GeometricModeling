using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets;

public class Script : MonoBehaviour
{
    public Material Mat;

    // Start is called before the first frame update
    void Start()
    {
        _Grid g = new _Grid(10, 10);
        Circle c = new Circle(16, 0, true);
        Cylinder cyl = new Cylinder(16);
        Sphere sph = new Sphere(16);

        /*
        foreach (Vector3 v in sph.getVertices())
        {
            Debug.Log(v.ToString());
        }
        //*/

        /*
        foreach (int t in sph.getTriangles())
        {
            Debug.Log(t);
        }
        //*/

        Mesh msh = new Mesh();
        msh.vertices = sph.getVertices();
        msh.triangles = sph.getTriangles();

        gameObject.GetComponent<MeshFilter>().mesh = msh;
        gameObject.GetComponent<MeshRenderer>().material = Mat;
    }

    // Update is called once per frame
    void Update()
    {

    }
}