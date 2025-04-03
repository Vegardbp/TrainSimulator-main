using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform target;
    public float camSpeed = 10;
    void Start()
    {
        if(target != null)
            pos = target.position;
    }

    Vector3 pos;

    void Update()
    {
        if(target != null)
        {
            pos = Vector3.MoveTowards(pos, target.position, camSpeed * Time.deltaTime);
            transform.LookAt(pos, Vector3.up);
        }
        else
        {
            pos = Vector3.MoveTowards(pos, Vector3.zero, camSpeed * Time.deltaTime);
            transform.LookAt(pos, Vector3.up);
        }
    }
}
