using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PowerPro : MonoBehaviour
{
    public NodePath mainTrack; // The red track (oval loop)
    public List<NodePath> altTracks; // The blue tracks (three separate straight paths)
    public static PowerPro Singleton { get; private set; }

    public float globalSensorRange;

    public List<Train> trains = new();

    public GameObject trainPrefab;

    public void SetTrainCount(int count)
    {
        int currentCount = trains.Count;
        if (count < currentCount)
        {
            // Remove excess trains
            int toRemove = currentCount - count;
            for (int i = 0; i < toRemove; i++)
            {
                Train train = trains[trains.Count - 1];
                Destroy(train.gameObject);
                trains.RemoveAt(trains.Count - 1);
            }
        }
        else if (count > currentCount)
        {
            // Add new trains
            int toAdd = count - currentCount;
            for (int i = 0; i < toAdd; i++)
                trains.Add(Instantiate(trainPrefab).GetComponent<Train>());
        }
        // If count equals currentCount, do nothing
    }

    private void Awake()
    {
        Singleton = this;
    }

    public Transform ClosestTrain(Vector3 pos)
    {
        float closest = Mathf.Infinity;
        Transform closestTrain = null;
        foreach(var train in trains)
        {
            float d = Vector3.Distance(train.transform.position, pos);
            if (d < closest)
            {
                closest = d;
                closestTrain = train.transform;
            }
        }
        return closestTrain;
    }
}
