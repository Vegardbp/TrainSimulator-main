using UnityEngine;

public class NodePathSwitch : MonoBehaviour
{
    public NodePath defaultPath;
    public NodePath altPath;
    public NodePath incomingPath;
    public bool switchPath = false;

    private void Start()
    {
        PowerPro.Singleton.AddSwitch(this);
    }

    public NodePath NextPath(NodePath currentPath)
    {
        if (incomingPath.gameObject == currentPath.gameObject)
        {
            if (!switchPath)
                return defaultPath;
            else
                return altPath;
        }
        else
            return incomingPath;
    }
}
