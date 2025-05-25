using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GrassHandlerSPawner : MonoBehaviour
{
    public uint count = 1000; // Number of grass blades
    public bool regenerate;

    public float closeDistance = 50;
    public float farDistance = 150;

    public float density;

    public int handlerCount;
    public float handlerSpacing;

    public GameObject GrassHandler;

    List<GrassSpawnHandler> handlers = new();
    void Start()
    {
        for(int x = 0; x < handlerCount; x++)
        {
            for(int y = 0; y < handlerCount; y++)
            {
                float xPos = x * handlerSpacing - ((handlerCount - 1f) * handlerSpacing / 2f);
                float yPos = y * handlerSpacing - ((handlerCount - 1f) * handlerSpacing / 2f);
                var spawner = Instantiate(GrassHandler, transform);
                spawner.transform.position = transform.position + new Vector3(xPos, 0, yPos);
                var handlerScript = spawner.GetComponent<GrassSpawnHandler>();
                handlerScript.count = (uint)(count / Mathf.Pow(handlerCount, 2f));
                UpdateSettings(handlerScript);
                handlers.Add(handlerScript);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GrassSpawnHandler handler in handlers) 
            UpdateSettings(handler);
    }

    void UpdateSettings(GrassSpawnHandler handler)
    {
        handler.size = handlerSpacing/2f;
        handler.density = density;
        handler.closeDistance = closeDistance;
        handler.farDistance = farDistance;
        handler.regenerate = regenerate;
        regenerate = false;
    }
}
