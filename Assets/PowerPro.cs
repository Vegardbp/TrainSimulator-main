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

    public float speedStep = 1;

    [Header("train command")]
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

    public void InterpretCommand(byte[] bytes)
    {
        if (acceptModbusCommand)
        {
            if (bytes[0] == 72)  //selectLoco
            {
                var loco = trains[bytes[1]];
                if (bytes[2] == 74)
                    loco.targetSpeed += speedStep;
                else if (bytes[2] == 75)
                    loco.targetSpeed -= speedStep;
                else if (bytes[2] == 90)
                    loco.targetSpeed += speedStep*4.0f/10.0f;
                else if (bytes[2] == 91)
                    loco.targetSpeed -= speedStep * 4.0f / 10.0f;
                if (bytes[3] == 106)
                    loco.targetSpeed = Mathf.Abs(loco.targetSpeed);
                else if (bytes[3] == 107)
                    loco.targetSpeed = -Mathf.Abs(loco.targetSpeed);
            }
        }
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
    }

    private void Awake()
    {
        Singleton = this;
    }
}
