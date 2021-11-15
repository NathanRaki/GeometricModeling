using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotation : MonoBehaviour
{
    float rotationSpeed = 2f;

    void Update()
    {
        transform.Rotate(new Vector3(rotationSpeed * Time.deltaTime, 0f, 0f));
    }
}
