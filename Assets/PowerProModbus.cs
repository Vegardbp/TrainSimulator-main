using UnityEngine;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;

public class PowerProModbus : MonoBehaviour
{
    public string inputIP = "10.0.0.67";
    public string inputPort = "5020";
    
    public int accelerateForwardCommand = 1;
    public int accelerateBackwardCommand = 2;
    public int brakeCommand = 3;

    public float updateFrequency = 10;

    [HideInInspector]
    public bool connected = false;

    PowerPro powerPro;

    //first 4 registers: switch states from codesys

    //Private vars
    //UModbusTCP
    UModbusTCP m_oUModbusTCP;
    UModbusTCP.ResponseData m_oUModbusTCPResponse;
    UModbusTCP.ExceptionData m_oUModbusTCPException;

    float updateDelay;

    void Awake()
    {
        updateDelay = 1.0f/updateFrequency;
        powerPro = GetComponent<PowerPro>();
        //UModbusTCP
        m_oUModbusTCP = null;
        m_oUModbusTCPResponse = null;
        m_oUModbusTCPException = null;

        m_oUModbusTCP = UModbusTCP.Instance;
    }

    float t = 0;

    void Update()
    {
        if (connected)
            TryRead();
    }

    void TryRead()
    {
        t += Time.deltaTime;
        if (t > updateDelay)
        {
            List<ushort> data = new();
            foreach (bool state in powerPro.sensorStates)
                if (state)
                    data.Add(1);
                else
                    data.Add(0);
            ushort switchCount = (ushort)powerPro.switchStates.Count;
            ushort sensorCount = (ushort)powerPro.sensorStates.Count;
            ushort trainCommandCount = 1;
            WriteHolding((ushort)(switchCount + trainCommandCount), data); //publish sensor states
            ReadHolding(0, (ushort)(switchCount + trainCommandCount + sensorCount)); //read everything for debug and update
            t = 0;
        }
    }

    void PreapareModbus(bool write = false)
    {
        //Connection values
        string sIp = inputIP;
        ushort usPort = Convert.ToUInt16(inputPort);

        if (!m_oUModbusTCP.connected)
        {
            m_oUModbusTCP.Connect(sIp, usPort);
        }

        if (m_oUModbusTCPResponse != null)
        {
            m_oUModbusTCP.OnResponseData -= m_oUModbusTCPResponse;
        }
        if (!write)
        {
            m_oUModbusTCPResponse = new UModbusTCP.ResponseData(UModbusTCPOnResponseData);
            m_oUModbusTCP.OnResponseData += m_oUModbusTCPResponse;
        }

        //Exception callback
        if (m_oUModbusTCPException != null)
        {
            m_oUModbusTCP.OnException -= m_oUModbusTCPException;
        }
        m_oUModbusTCPException = new UModbusTCP.ExceptionData(UModbusTCPOnException);
        m_oUModbusTCP.OnException += m_oUModbusTCPException;
    }

    public void WriteHolding(ushort stateAddress,List<ushort> data)
    {
        PreapareModbus(true);
        m_oUModbusTCP.WriteMultipleRegister(1, 1, stateAddress, ConvertUShortListToBytesBigEndian(data));
    }

    static byte[] ConvertUShortListToBytesBigEndian(List<ushort> values) //from chatGPT
    {
        byte[] result = new byte[values.Count * sizeof(ushort)];

        for (int i = 0; i < values.Count; i++)
        {
            BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(i * 2), values[i]);
        }

        return result;
    }

    public void ReadHolding(ushort startAddress, ushort addresses)
    {
        PreapareModbus();
        m_oUModbusTCP.ReadHoldingRegister(2, 1, startAddress, addresses); //Read 7 registers starting at 0
    }

    void UModbusTCPOnResponseData(ushort _oID, byte _oUnit, byte _oFunction, byte[] _oValues)
    {
        int iNumberOfValues = _oValues[8];

        byte[] oResponseFinalValues = new byte[iNumberOfValues];
        for (int i = 0; i < iNumberOfValues; ++i)
        {
            oResponseFinalValues[i] = _oValues[9 + i];
        }

        int[] iValues = UModbusTCPHelpers.GetIntsOfBytes(oResponseFinalValues);

        Debug.Log(string.Join(", ", iValues)); //debug for clairity

        for (int i = 0; i < 4; i++)
            powerPro.switchStates[i] = iValues[i] != 0; //update all switch states

        int trainCommand = iValues[4]; //update command
        powerPro.accelerateForward = trainCommand == accelerateForwardCommand;
        powerPro.accelerateBackward = trainCommand == accelerateBackwardCommand;
        powerPro.brake = trainCommand == brakeCommand;
    }

    void UModbusTCPOnException(ushort _oID, byte _oUnit, byte _oFunction, byte _oException)
    {
        if (_oException != 3)
            connected = false;
        Debug.Log("Exception: " + _oException);
    }
}
