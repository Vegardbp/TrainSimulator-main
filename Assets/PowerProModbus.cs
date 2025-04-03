using UnityEngine;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;

public class PowerProModbus : MonoBehaviour
{
    public string inputIP;
    public string inputPort = "502";

    public ushort modbusStartIndex = 0;
    public ushort maxTrains = 5;

    public float updateFrequency = 10;

    [HideInInspector]
    public bool connected = false;

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
            ReadHolding(modbusStartIndex, (ushort)(maxTrains * 2)); //read everything for debug and update
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
        m_oUModbusTCP.ReadHoldingRegister(2, 1, startAddress, addresses);
    }

    bool IntToBool(int i)
    {
        return (i != 0);
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

        PowerPro.Singleton.SetTrainCount(iValues.Length / 2);

        for (int i = 0; i < iValues.Length; i += 2)
        {
            int trainIndex = i / 2;
            PowerPro.Singleton.trains[trainIndex].position = iValues[i];
            PowerPro.Singleton.trains[trainIndex].isOnAltTrack = IntToBool(iValues[i + 1]);
        }
    }

    void UModbusTCPOnException(ushort _oID, byte _oUnit, byte _oFunction, byte _oException)
    {
        if (_oException != 3)
            connected = false;
        Debug.Log("Exception: " + _oException);
    }
}
