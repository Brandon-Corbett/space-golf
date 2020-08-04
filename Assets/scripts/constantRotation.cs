using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class constantRotation : MonoBehaviour
{
    public Vector3 rotation;
    public Rigidbody rb;

    private void Start()
    {
        rb.maxAngularVelocity = 20;
    }
    void Update()
    {
        rb.angularVelocity = rotation;
    }
}
