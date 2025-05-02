using UnityEngine;
using UnityEngine.UI;

public class LookAtTarget : MonoBehaviour
{
    public Transform target;
    public Transform center;
    public float camSpeed = 10f;       // Speed at which the focus point moves
    public float rotSpeed = 5f;        // Speed of rotation interpolation
    public float posCoefficient = 0.5f;  // 0 = looks at origin, 1 = full target, in between = offset

    public GameObject TrainUI;
    public Slider SpeedSlider;

    private Vector3 pos;

    void Start()
    {
        Vector3 rawTarget = target != null ? target.position : center.position;
        pos = Vector3.Lerp(center.position, rawTarget, posCoefficient);
    }

    Transform prevTarget = null;

    void Update()
    {
        if (target != null)
        {
            OutlineEffect[] outlines = target.gameObject.GetComponents<OutlineEffect>();
            if(outlines.Length > 1)
                outlines[1].EnableOutline();
        }


        Vector3 rawTarget = target != null ? target.position : center.position;
        Vector3 adjustedTarget = Vector3.Lerp(center.position, rawTarget, posCoefficient);

        // Smooth position movement
        pos = Vector3.MoveTowards(pos, adjustedTarget, camSpeed * Time.deltaTime);

        // Rotation toward the smoothed position
        Vector3 dir = pos - transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed * Time.deltaTime);
        }



        TrainUI.SetActive(target != null);
        if(target != null)
        {
            Train targetTrain = target.transform.parent.gameObject.GetComponent<Train>();
            if (prevTarget != target)
                SpeedSlider.value = targetTrain.speed;
            targetTrain.speed = SpeedSlider.value;
        }
        prevTarget = target;
    }
}
