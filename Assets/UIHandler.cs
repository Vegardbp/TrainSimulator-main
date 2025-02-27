using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public TMP_InputField PLCIP;
    public Button connectButton;
    public PowerProModbus modbus;
    public GameObject connectedCheckmark;

    void Start()
    {
        PLCIP.text = PlayerPrefs.GetString("PLCIP", "");
        modbus.inputIP = PLCIP.text;
        PLCIP.onEndEdit.AddListener((string text) =>
        {
            PlayerPrefs.SetString("PLCIP", text);
            modbus.inputIP = text;
        });
        connectButton.onClick.AddListener(() =>
        {
            modbus.connected = true;
        });
    }

    void Update()
    {
        connectedCheckmark.SetActive(modbus.connected);
    }
}
