using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform target;
    public float camSpeed = 10;
    void Start()
    {
        pos = target.position;
    }

    Vector3 pos;

    void Update()
    {
        pos = Vector3.MoveTowards(pos, target.position, camSpeed*Time.deltaTime);
        transform.LookAt(pos, Vector3.up);
    }
}
