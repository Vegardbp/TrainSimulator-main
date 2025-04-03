using System.Collections.Generic;
using UnityEngine;

public class MagneticSensor : MonoBehaviour
{
    public float sensorRangeCoefficient = 1.0f;
    public bool debug = true;

    public void Update()
    {
        var train = PowerPro.Singleton.ClosestTrain(transform.position);
        if (train == null)
            return;
        bool active = Vector3.Distance(train.position, transform.position) < sensorRangeCoefficient*PowerPro.Singleton.globalSensorRange;
        if (active && debug)
            Debug.DrawRay(transform.position, Vector3.up * 5, Color.red);
    }
}
