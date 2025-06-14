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

    public ushort startWriteAddress = 4;

    //Private vars
    //UModbusTCP
    UModbusTCP m_oUModbusTCP;
    UModbusTCP.ResponseData m_oUModbusTCPResponse;
    UModbusTCP.ExceptionData m_oUModbusTCPException;

    void Awake()
    {
        //UModbusTCP
        m_oUModbusTCP = null;
        m_oUModbusTCPResponse = null;
        m_oUModbusTCPException = null;

        m_oUModbusTCP = UModbusTCP.Instance;
    }

    float t = 0;

    void Update()
    {
        t += Time.deltaTime;
        if(t >= 1.0 / (float)updateFrequency)
        {
            t = 0;
            if (connected)
            {
                List<ushort> writeData = new();
                var trains = PowerPro.Singleton.trains;
                for (int i = 0; i < trains.Count; i++)
                {
                    if (trains[i].targetFlag != null)
                    {
                        writeData.Add((ushort)trains[i].targetFlag.index);
                        writeData.Add((ushort)1);
                    }
                    else
                    {
                        writeData.Add((ushort)1000);
                        writeData.Add((ushort)0);
                    }
                }
                WriteHolding(startWriteAddress, writeData);
                ReadHolding(modbusStartIndex, (ushort)(maxTrains * 2));
            }
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

    void UModbusTCPOnResponseData(ushort _oID, byte _oUnit, byte _oFunction, byte[] _oValues)
    {
        int iNumberOfValues = _oValues[8];

        byte[] oResponseFinalValues = new byte[iNumberOfValues];
        for (int i = 0; i < iNumberOfValues; ++i)
        {
            oResponseFinalValues[i] = _oValues[9 + i];
        }

        int[] iValues = UModbusTCPHelpers.GetIntsOfBytes(oResponseFinalValues);

        Debug.Log("Noge");
        Debug.Log(string.Join(", ", iValues)); //debug for clairity

        PowerPro.Singleton.iValues = iValues;
        PowerPro.Singleton.update = true;
    }

    void UModbusTCPOnException(ushort _oID, byte _oUnit, byte _oFunction, byte _oException)
    {
        if (_oException != 3 && _oException != 2)
            connected = false;
        Debug.Log("Exception: " + _oException);
    }
}
