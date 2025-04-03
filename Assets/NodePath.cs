using System.Collections.Generic;
using UnityEngine;

public class NodePath : MonoBehaviour
{
    List<Transform> nodes = new List<Transform>();
    public Transform firstNode;
    public Transform lastNode;
    public bool autoSort = true;
    public bool debugSegment = true;

    private float totalLength = 0f;
    private List<float> segmentLengths = new List<float>();

    void Start()
    {
        // Populate nodes
        if (firstNode != null)
            nodes.Add(firstNode);
        foreach (Transform t in transform)
            if (!nodes.Contains(t))
                nodes.Add(t);
        if (lastNode != null)
            nodes.Add(lastNode);

        if (autoSort)
            nodes = SortedNodes();

        // Calculate total length and segment lengths
        CalculatePathLength();
    }

    List<Transform> SortedNodes()
    {
        var newNodes = new List<Transform> { nodes[0] };
        var newNodeIndices = new List<int> { 0 };
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            float closest = Mathf.Infinity;
            int nextNodeIndex = 0;
            Transform currentNode = newNodes[i];
            for (int j = 0; j < nodes.Count; j++)
            {
                float dst = Vector3.Distance(nodes[j].position, currentNode.position);
                if (dst < closest && !newNodeIndices.Contains(j))
                {
                    nextNodeIndex = j;
                    closest = dst;
                }
            }
            if (nextNodeIndex != 0)
            {
                newNodeIndices.Add(nextNodeIndex);
                newNodes.Add(nodes[nextNodeIndex]);
            }
        }
        return newNodes;
    }

    void CalculatePathLength()
    {
        totalLength = 0f;
        segmentLengths.Clear();
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            float segmentLength = Vector3.Distance(nodes[i].position, nodes[i + 1].position);
            segmentLengths.Add(segmentLength);
            totalLength += segmentLength;
        }
    }

    // Get the position and forward direction at a normalized position (0-1)
    public (Vector3 position, Vector3 forward) GetPositionAtNormalized(float normalizedPosition)
    {
        if (nodes.Count < 2) return (Vector3.zero, Vector3.forward);

        // Convert normalized position (0-1) to a distance along the path
        float targetDistance = normalizedPosition * totalLength;
        float currentDistance = 0f;

        // Find the segment we're on
        for (int i = 0; i < segmentLengths.Count; i++)
        {
            float segmentLength = segmentLengths[i];
            if (currentDistance + segmentLength >= targetDistance)
            {
                // We're in this segment
                float segmentProgress = (targetDistance - currentDistance) / segmentLength;
                Vector3 start = nodes[i].position;
                Vector3 end = nodes[i + 1].position;
                Vector3 position = Vector3.Lerp(start, end, segmentProgress);
                Vector3 forward = (end - start).normalized;
                return (position, forward);
            }
            currentDistance += segmentLength;
        }

        // If we get here, return the last position
        return (nodes[nodes.Count - 1].position, (nodes[nodes.Count - 1].position - nodes[nodes.Count - 2].position).normalized);
    }

    // Project a point onto the path and return the normalized position (0-1), the closest point, and the forward direction
    public (float normalizedPosition, Vector3 closestPoint, Vector3 forward) ProjectPointOntoPath(Vector3 point)
    {
        if (nodes.Count < 2) return (0f, Vector3.zero, Vector3.forward);

        float closestDistance = Mathf.Infinity;
        Vector3 closestPoint = Vector3.zero;
        float closestNormalizedPosition = 0f;
        Vector3 closestForward = Vector3.forward;
        float currentDistance = 0f;

        for (int i = 0; i < segmentLengths.Count; i++)
        {
            Vector3 start = nodes[i].position;
            Vector3 end = nodes[i + 1].position;
            Vector3 pointOnSegment = GetClosestPointOnLineSegment(start, end, point);
            float distanceToPoint = Vector3.Distance(pointOnSegment, point);

            if (distanceToPoint < closestDistance)
            {
                closestDistance = distanceToPoint;
                closestPoint = pointOnSegment;

                // Calculate the normalized position
                float segmentDistance = Vector3.Distance(start, pointOnSegment);
                float segmentStartDistance = currentDistance;
                closestNormalizedPosition = (segmentStartDistance + segmentDistance) / totalLength;
                closestForward = (end - start).normalized;
            }

            currentDistance += segmentLengths[i];
        }

        return (closestNormalizedPosition, closestPoint, closestForward);
    }

    Vector3 GetClosestPointOnLineSegment(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 line = end - start;
        float lineLengthSquared = line.sqrMagnitude;

        if (lineLengthSquared == 0)
            return start;

        float t = Vector3.Dot(point - start, line) / lineLengthSquared;
        t = Mathf.Clamp01(t); // Clamp t to the range [0, 1]

        return start + t * line; // Get the closest point on the line segment
    }

    // Optional: Debug drawing
    private void Update()
    {
        if (debugSegment)
        {
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Debug.DrawLine(nodes[i].position + Vector3.up, nodes[i + 1].position + Vector3.up, Color.green);
            }
        }
    }

    public float GetTotalLength()
    {
        return totalLength;
    }
}