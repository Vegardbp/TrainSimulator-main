using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodePath : MonoBehaviour
{
    List<Transform> nodes = new List<Transform>();
    public Transform firstNode;
    public Transform lastNode;

    public bool autoSort = true;

    public bool debugOrder = false;

    public bool debugSegment = true;

    void AddNode(Transform node)
    {
        if(!nodes.Contains(node))
            nodes.Add(node);
    }

    void Start()
    {
        if(firstNode != null)
            AddNode(firstNode);
        foreach (Transform t in transform)
            AddNode(t);
        if(lastNode != null)
            AddNode(lastNode);
        if (autoSort)
            nodes = SortedNodes();
    }

    private void Update()
    {
        if(debugOrder)
            for (int i = 0; i < nodes.Count - 1; i++)
                Debug.DrawLine(nodes[i].position + Vector3.up * (i-1) * 0.5f, nodes[i+1].position+Vector3.up*i*0.5f);
    }

    List<Transform> SortedNodes()
    {
        var newNodes = new List<Transform>{nodes[0]};
        var newNodeIndicies = new List<int> { 0 };
        for(int i = 0; i < nodes.Count; i++)
        {
            float closest = Mathf.Infinity;
            int nextNodeIndex = 0;
            Transform currentNode = newNodes[i];
            for (int j = 0; j < nodes.Count; j++)
            {
                var node = nodes[j];
                float dst = Vector3.Distance(node.position, currentNode.position);
                if (dst < closest && !newNodeIndicies.Contains(j))
                {
                    nextNodeIndex = j;
                    closest = dst;
                }
            }
            if(nextNodeIndex != 0)
            {
                newNodeIndicies.Add(nextNodeIndex);
                newNodes.Add(nodes[nextNodeIndex]);
            }
        }
        return newNodes;
    }

    public Transform nextNode;
    public Transform prevNode;

    public Vector3 ForwardDirection(Transform train)
    {
        var segment = CurrentLineSegment(train);
        nextNode = segment[1];
        prevNode = segment[0];
        return (segment[1].position - train.position).normalized;
    }

    public float TrackAngle(Transform train)
    {
        int segmentIndex = CurrentLineIndex(train);
        float angle = 0;
        float angleCount = 0;
        for(int i = 0; i < 1; i++)
        {
            int index = segmentIndex + i;
            if (index >= 0 && index < nodes.Count - 1)
            {
                angle += SegmentAngle(index);
                angleCount++;
            }
        }
        return angle/angleCount;
    }

    public float SegmentAngle(int index)
    {
        Vector3 trackForward = (nodes[index].position - nodes[index+1].position).normalized;
        float angle = Vector3.Angle(Vector3.forward, trackForward);
        angle *= Mathf.Sign(Vector3.Dot(Vector3.right, trackForward));
        return angle;
    }

    public Vector3 ReverseDirection(Transform train)
    {
        var segment = CurrentLineSegment(train);
        nextNode = segment[0];
        prevNode = segment[1];
        return (segment[0].position - train.position).normalized;
    }

    List<Transform> CurrentLineSegment(Transform train)
    {
        int index = CurrentLineIndex(train);
        if (debugSegment)
        {
            Debug.DrawLine(nodes[index].position + Vector3.up, nodes[index + 1].position + Vector3.up);
            Debug.DrawLine(nodes[index].position, nodes[index].position + Vector3.up);
            Debug.DrawLine(nodes[index + 1].position, nodes[index + 1].position + Vector3.up);
        }
        return new List<Transform>{nodes[index],nodes[index + 1]};
    }

    int CurrentLineIndex(Transform train)
    {
        float closest = Mathf.Infinity;
        int closestIndex = 0;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            float dst = GetClosestDistance(nodes[i].position, nodes[i + 1].position, train.position);
            if (dst < closest)
            {
                closest = dst;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    float GetClosestDistance(Vector3 start, Vector3 end, Vector3 point)
    {
        return Vector3.Distance(GetClosestPointOnLineSegment(start,end,point),point);
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
}
