using UnityEngine;

public class Train : MonoBehaviour
{
    public NodePath track;

    public float topSpeed;
    public float timeToTopSpeed;

    public float targetSpeed;

    public float speed = 0;

    private void Start()
    {
        PowerPro.Singleton.AddTrain(this);
    }
    void Update()
    {
        speed += Mathf.Sign(targetSpeed - speed) * topSpeed / timeToTopSpeed * Time.deltaTime;

        transform.eulerAngles = new Vector3(0, track.TrackAngle(transform), 0);

        if (speed > 0)
            transform.position += Mathf.Abs(speed) * track.ForwardDirection(transform) * Time.deltaTime;
        else if(speed < 0)
            transform.position += Mathf.Abs(speed) * track.ReverseDirection(transform) * Time.deltaTime;
        speed = Mathf.Clamp(speed,-topSpeed, topSpeed);

        if (track.nextNode != null)
            if(track.nextNode.TryGetComponent(out NodePathSwitch switchComponent))
                track = switchComponent.NextPath(track);
    }
}
