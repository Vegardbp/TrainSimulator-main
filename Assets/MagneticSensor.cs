using System.Collections.Generic;
using UnityEngine;

public class MagneticSensor : MonoBehaviour
{
    public float sensorRangeCoefficient = 1.0f;
    public bool debug = false;

    public bool active;

    private void Start()
    {
        PowerPro.Singleton.AddSensor(this);
    }

    public void Update()
    {
        if (active && debug)
            Debug.DrawRay(transform.position, Vector3.up * 5, Color.red);
    }
}
