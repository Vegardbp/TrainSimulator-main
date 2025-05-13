using UnityEngine;
using System.Collections.Generic;

public class Train : MonoBehaviour
{
    List<Vector2Int> altRanges = new List<Vector2Int> { 
        new Vector2Int(0,10),
        new Vector2Int(27,37),
        new Vector2Int(51,60)};
    List<int> trackBlocks = new List<int> {1, 1, 15, 25, 32, 32, 41, 47, 59, 59};
    List<bool> trackBlockAlt = new List<bool> {false, true, false, false,  true, false, false, false, false, true};

    public bool active = true;

    public int targetBlock;
    public int currentBlock;

    public float position = 0f; // Position between 0 and 60 (always relative to the main track)
    public bool isOnAltTrack = false; // Whether the train is on an alternate track

    public float topSpeed;
    public float speed;

    public float turnspeed = 45f;

    bool VectorInRange(float t, Vector2Int range)
    {
        if (t > range.x && t < range.y)
            return true;
        return false;
    }

    bool InAltTrackRange()
    {
        foreach (var range in altRanges)
            if (VectorInRange(position, range))
                return true;
        return false;
    }

    private void Start()
    {
        position = trackBlocks[currentBlock - 1];
        isOnAltTrack = trackBlockAlt[currentBlock - 1];
    }

    void FixedUpdate()
    {
        position = Mathf.Clamp(position, 1f, 59f);
        // Convert position (0-60) to normalized (0-1) for the main track
        float normalizedPosition = position / 60f;
        float targetPos = trackBlocks[targetBlock-1];
        bool targetAltState = trackBlockAlt[targetBlock - 1];
        float currentPos = trackBlocks[currentBlock-1];
        bool currentAltState = trackBlockAlt[currentBlock-1];


        float targDelta = targetPos - position;
        float dstToTarg = Mathf.Abs(targDelta);
        float dstToCurrent = Mathf.Abs(position - currentPos);

        if (active)
        {
            if (dstToCurrent > 12.5f) // Resync position if neccesarry
                position = currentPos;
            if (dstToCurrent < 4) // Resync alt track state if neccesary
                isOnAltTrack = currentAltState;
            isOnAltTrack = currentAltState;
            if (dstToTarg < dstToCurrent)
                isOnAltTrack = targetAltState;
        }

        // Get the position on the main track
        var (mainTrackPosition, mainTrackForward) = PowerPro.Singleton.mainTrack.GetPositionAtNormalized(normalizedPosition);
        if(active)
            position += topSpeed*Mathf.Clamp01(dstToTarg)*Mathf.Sign(targDelta) /100.0f * Time.deltaTime;
        if (!isOnAltTrack || !InAltTrackRange())
        {
            // If on the main track, use the main track position directly
            transform.position = mainTrackPosition;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(mainTrackForward, Vector3.up), turnspeed*Time.deltaTime);
        }
        else
        {
            // If on an alternate track, find the closest alternate track and project the main track position onto it
            NodePath closestAltTrack = null;
            float closestDistance = Mathf.Infinity;
            Vector3 closestPointOnAltTrack = Vector3.zero;
            Vector3 closestForward = Vector3.forward;

            foreach (var altTrack in PowerPro.Singleton.altTracks)
            {
                var (altNormalizedPosition, altClosestPoint, altForward) = altTrack.ProjectPointOntoPath(mainTrackPosition);
                float distance = Vector3.Distance(mainTrackPosition, altClosestPoint);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestAltTrack = altTrack;
                    closestPointOnAltTrack = altClosestPoint;
                    closestForward = altForward;
                }
            }

            if (closestAltTrack != null)
            {
                // Use the closest point on the alternate track
                transform.position = closestPointOnAltTrack;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(closestForward, Vector3.up), turnspeed * Time.deltaTime);
            }
        }
    }
}