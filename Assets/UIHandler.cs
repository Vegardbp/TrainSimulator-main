using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public TMP_InputField PLCIP;
    public Button connectButton;
    public PowerProModbus modbus;
    public GameObject connectedCheckmark;

    public Toggle simpleVisualsToggle;
    public List<GameObject> visualsObjects;

    bool IntToBool(int a)
    {
        return (a == 1);
    }

    int BoolToInt(bool a)
    {
        if (a)
            return 1;
        return 0;
    }

    void Start()
    {
        PLCIP.text = PlayerPrefs.GetString("PLCIP", "");
        modbus.inputIP = PLCIP.text;
        simpleVisualsToggle.isOn = IntToBool(PlayerPrefs.GetInt("simp", 0));
        UpdateShadowQuality();
        PLCIP.onEndEdit.AddListener((string text) =>
        {
            PlayerPrefs.SetString("PLCIP", text);
            modbus.inputIP = text;
        });
        connectButton.onClick.AddListener(() =>
        {
            modbus.connected = true;
        });
        simpleVisualsToggle.onValueChanged.AddListener((bool val) =>
        {
            PlayerPrefs.SetInt("simp", BoolToInt(val));
            UpdateShadowQuality();
        });
    }

    void Update()
    {
        connectedCheckmark.SetActive(modbus.connected);
        foreach (var obj in visualsObjects)
            obj.SetActive(!simpleVisualsToggle.isOn);
    }

    void UpdateShadowQuality()
    {
        UniversalRenderPipelineAsset pipelineAsset = (UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;
        if (simpleVisualsToggle.isOn)
        {
            pipelineAsset.shadowDistance = 0;
            pipelineAsset.shadowCascadeCount = 1;
        }
        else
        {
            pipelineAsset.shadowDistance = 100;
            pipelineAsset.shadowCascadeCount = 2;
        }
    }
}
