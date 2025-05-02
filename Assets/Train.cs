using UnityEngine;
using System.Collections.Generic;

public class Train : MonoBehaviour
{
    public int CodesysPos;
    public float position = 0f; // Position between 0 and 60 (always relative to the main track)
    public bool isOnAltTrack = false; // Whether the train is on an alternate track

    public float speed;

    public float turnspeed = 45f;

    void FixedUpdate()
    {
        position = Mathf.Clamp(position, 1f, 59f);
        // Convert position (0-60) to normalized (0-1) for the main track
        float normalizedPosition = position / 60f;

        // Get the position on the main track
        var (mainTrackPosition, mainTrackForward) = PowerPro.Singleton.mainTrack.GetPositionAtNormalized(normalizedPosition);
        position += speed/64.0f * Time.deltaTime;
        if (!isOnAltTrack)
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