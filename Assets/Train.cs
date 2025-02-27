using UnityEngine;

public class Train : MonoBehaviour
{
    public NodePath track;

    public float topSpeed;
    public float timeToTopSpeed;
    public float timeToStop;

    public bool accelerateForward;
    public bool accelerateBackward;
    public bool brake;

    float speedCoefficient = 0;

    private void Start()
    {
        PowerPro.Singleton.AddTrain(this);
    }
    void Update()
    {
        if (accelerateForward)
            speedCoefficient += Time.deltaTime / timeToTopSpeed;
        if (accelerateBackward)
            speedCoefficient -= Time.deltaTime / timeToTopSpeed;

        if (brake)
        {
            if(speedCoefficient < 0)
            {
                speedCoefficient += Time.deltaTime / timeToStop;
                speedCoefficient = Mathf.Clamp(speedCoefficient, -1.0f, 0.0f);
            }
            else if(speedCoefficient > 0)
            {
                speedCoefficient -= Time.deltaTime / timeToStop;
                speedCoefficient = Mathf.Clamp(speedCoefficient, 0.0f, 1.0f);
            }
        }

        speedCoefficient = Mathf.Clamp(speedCoefficient, -1.0f, 1.0f);

        float speed = speedCoefficient * topSpeed;

        transform.eulerAngles = new Vector3(0, track.TrackAngle(transform), 0);

        if (speed > 0)
        {
            transform.position += Mathf.Abs(speed) * track.ForwardDirection(transform) * Time.deltaTime;
        }
        else if(speed < 0)
        {
            transform.position += Mathf.Abs(speed) * track.ReverseDirection(transform) * Time.deltaTime;
        }

        if (track.nextNode != null)
            if(track.nextNode.TryGetComponent(out NodePathSwitch switchComponent))
                track = switchComponent.NextPath(track);
    }
}
