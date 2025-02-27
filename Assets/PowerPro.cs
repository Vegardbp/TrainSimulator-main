using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PowerPro : MonoBehaviour
{
    public static PowerPro Singleton { get; private set; }

    public float globalSensorRange;

    List<NodePathSwitch> switches = new();
    public List<bool> switchStates;

    List<MagneticSensor> sensors = new();
    public List<bool> sensorStates;

    List<Train> trains = new();

    public bool debugSensorOrder = true;

    [Header("train command")]
    public int trainIndex;
    public bool accelerateForward;
    public bool accelerateBackward;
    public bool brake;
    public bool acceptModbusCommand = true;


    public void AddTrain(Train train)
    {
        trains.Add(train);
    }

    public void AddSensor(MagneticSensor sensor)
    {
        sensors.Add(sensor);
        sensorStates.Add(false);
    }

    public void AddSwitch(NodePathSwitch pathSwitch)
    {
        switches.Add(pathSwitch);
        switchStates.Add(false);
    }

    private void Update()
    {
        if (debugSensorOrder)
        {
            float i = 1;
            foreach(MagneticSensor sensor in sensors)
            {
                Debug.DrawLine(sensor.transform.position,sensor.transform.position+Vector3.up*i);
                i++;
            }
        }
        for (int i = 0; i < switches.Count; i++)
            switches[i].switchPath = switchStates[i];

        for(int i = 0;i < sensors.Count; i++)
        {
            sensors[i].active = false;
            float sensorRange = sensors[i].sensorRangeCoefficient * globalSensorRange;
            foreach(var train in trains)
            {
                if (Vector3.Distance(sensors[i].transform.position, train.transform.position) < sensorRange)
                    sensors[i].active = true;
            }
            sensorStates[i] = sensors[i].active;
        }

        if (acceptModbusCommand)
        {
            if (trains.Count >= trainIndex)
            {
                trains[trainIndex].accelerateBackward = accelerateBackward;
                trains[trainIndex].accelerateForward = accelerateForward;
                trains[trainIndex].brake = brake;
            }
            else
                Debug.Log("train not in list");
        }
    }

    private void Awake()
    {
        Singleton = this;
    }
}
