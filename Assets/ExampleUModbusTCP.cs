using UnityEngine;
using UnityEngine.UI;
using System;

public class ExampleUModbusTCP : MonoBehaviour
{

    //Public vars
    public InputField inputIP;
    public InputField inputPort;
    public InputField inputAddress;
    public InputField inputResponseValue;

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

    void Start()
    {
        //Fast values
        inputIP.text = "192.168.1.33";
        inputPort.text = "502";
        inputAddress.text = "3";
    }


    void Update()
    {
        if (m_bUpdateResponse)
        {
            m_bUpdateResponse = false;
            inputResponseValue.text = m_iResponseValue.ToString();
        }
    }

    public void ButtonReadCoils()
    {

        //Reset response
        m_bUpdateResponse = false;
        m_iResponseValue = -1;
        inputResponseValue.text = string.Empty;

        //Connection values
        string sIp = inputIP.text;
        ushort usPort = Convert.ToUInt16(inputPort.text);
        ushort usAddress = Convert.ToUInt16(Int32.Parse(inputAddress.text) - 1);

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

        //Read coils
        m_oUModbusTCP.ReadCoils(1, 1, usAddress, 1);

    }

    public void ButtonReadInputs()
    {

        //Reset response
        m_bUpdateResponse = false;
        m_iResponseValue = -1;
        inputResponseValue.text = string.Empty;

        //Connection values
        string sIp = inputIP.text;
        ushort usPort = Convert.ToUInt16(inputPort.text);
        ushort usAddress = Convert.ToUInt16(Int32.Parse(inputAddress.text) - 1);

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

        //Read coils
        m_oUModbusTCP.ReadInputRegister(2, 1, usAddress, 1);

    }

    void UModbusTCPOnResponseData(ushort _oID, byte _oUnit, byte _oFunction, byte[] _oValues)
    {

        //Number of values
        int iNumberOfValues = _oValues[8];

        /*
        //Get values pair with 2
        int oCounter = 0;
        for(int i = 0; i < iNumberOfValues; i += 2) {
            byte[] oResponseFinalValues = new byte[2];
            for(int j = 0; j < 2; ++j) {
                oResponseFinalValues[j] = _oValues[9 + i + j];
            }
            ++oCounter; //More address
        }
        */

        //Get values
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
