using System.Collections.Generic;
using UnityEngine;

public class TrainSelector : MonoBehaviour
{
    [Header("LayerMask for train selection")]
    public LayerMask trainLayerMask;

    public float rayRadius = 2f;

    LookAtTarget lookTarget;

    List<OutlineEffect> outlines = new();

    private void Start()
    {
        lookTarget = GetComponent<LookAtTarget>();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetButtonDown("Cancel") || Input.GetMouseButtonDown(1))
            lookTarget.target = null;

        if (Physics.SphereCast(ray, rayRadius, out RaycastHit hit, Mathf.Infinity, trainLayerMask))
        {
            hit.collider.gameObject.GetComponents<OutlineEffect>()[0].EnableOutline();

            if (Input.GetMouseButtonDown(0))
                lookTarget.target = hit.collider.transform;

            return;
        }
    }
}
