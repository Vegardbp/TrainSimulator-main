using UnityEngine;
using UnityEngine.UI;
using System;
using static UnityEngine.Rendering.ReloadAttribute;

public class TestModbus : MonoBehaviour
{
    public string inputIP = "10.0.0.67";
    public string inputPort = "5020";
    public ushort address = 1;
    public string inputResponseValue;
    public ushort writeData;

    //Private vars
    //UModbusTCP
    UModbusTCP m_oUModbusTCP;
    UModbusTCP.ResponseData m_oUModbusTCPResponse;
    UModbusTCP.ExceptionData m_oUModbusTCPException;

    bool m_bUpdateResponse;
    int m_iResponseValue;

    void Awake()
    {
        //UModbusTCP
        m_oUModbusTCP = null;
        m_oUModbusTCPResponse = null;
        m_oUModbusTCPException = null;
        m_bUpdateResponse = false;
        m_iResponseValue = -1;

        m_oUModbusTCP = UModbusTCP.Instance;
    }


    void Update()
    {
        if (m_bUpdateResponse)
        {
            m_bUpdateResponse = false;
            inputResponseValue = m_iResponseValue.ToString();
            Debug.Log("Update my balls");
        }
    }

    void PreapareModbus()
    { //Reset response
        m_bUpdateResponse = false;
        m_iResponseValue = -1;
        inputResponseValue = string.Empty;

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
        m_oUModbusTCPResponse = new UModbusTCP.ResponseData(UModbusTCPOnResponseData);
        m_oUModbusTCP.OnResponseData += m_oUModbusTCPResponse;

        //Exception callback
        if (m_oUModbusTCPException != null)
        {
            m_oUModbusTCP.OnException -= m_oUModbusTCPException;
        }
        m_oUModbusTCPException = new UModbusTCP.ExceptionData(UModbusTCPOnException);
        m_oUModbusTCP.OnException += m_oUModbusTCPException;
    }

    public void WriteHolding()
    {
        PreapareModbus();
        var bytes = BitConverter.GetBytes(writeData);
        Array.Reverse(bytes);  // Ensure Big-Endian
        m_oUModbusTCP.WriteSingleRegister(1, 1, address, bytes);
    }

    public void ReadHolding()
    {
        PreapareModbus();
        m_oUModbusTCP.ReadHoldingRegister(2, 1, address, 1);
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
        m_iResponseValue = iValues[0];
        m_bUpdateResponse = true;
    }

    void UModbusTCPOnException(ushort _oID, byte _oUnit, byte _oFunction, byte _oException)
    {
        Debug.Log("Exception: " + _oException);
    }
}
