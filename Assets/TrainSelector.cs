using System.Collections.Generic;
using UnityEngine;

public class TrainSelector : MonoBehaviour
{
    [Header("LayerMask for train selection")]
    public LayerMask trainLayerMask;

    public LayerMask flagLayerMask;

    public float rayRadius = 2f;

    LookAtTarget lookTarget;

    private void Start()
    {
        lookTarget = GetComponent<LookAtTarget>();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetButtonDown("Cancel") || Input.GetMouseButtonDown(1))
            lookTarget.target = null;

        LayerMask mask = trainLayerMask;
        if (lookTarget.target != null)
            mask = flagLayerMask;

        if (Physics.SphereCast(ray, rayRadius, out RaycastHit hit, Mathf.Infinity, mask))
        {
            hit.collider.gameObject.GetComponents<OutlineEffect>()[0].EnableOutline();

            if (Input.GetMouseButtonDown(0))
            {
                if (lookTarget.target == null)
                    lookTarget.target = hit.collider.transform;
                else
                {
                    if (lookTarget.target.parent.gameObject.GetComponent<Train>().targetFlag == null)
                        lookTarget.target.parent.gameObject.GetComponent<Train>().targetFlag = hit.collider.transform.parent.parent.GetComponent<Flag>();
                    else
                        lookTarget.target.parent.gameObject.GetComponent<Train>().targetFlag = null;
                }
                    
            }
            return;
        }


    }
}