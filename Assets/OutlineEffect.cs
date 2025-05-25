using UnityEngine;

public class OutlineEffect : MonoBehaviour
{
    public Material outlineMaterial;

    private GameObject model;
    private GameObject outlineObject;

    void Start()
    {
        model = gameObject;
        if (model == null || outlineMaterial == null)
        {
            Debug.LogError("Model or Outline Material not assigned.");
            return;
        }

        // Get the MeshFilter and MeshRenderer from the target model
        MeshFilter meshFilter = model.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = model.GetComponent<MeshRenderer>();

        if (meshFilter == null || meshRenderer == null)
        {
            Debug.LogError("Target model must have both MeshFilter and MeshRenderer.");
            return;
        }

        // Create a new GameObject for the outline
        outlineObject = new GameObject(model.name + "_Outline");
        outlineObject.transform.SetParent(model.transform, false);
        outlineObject.layer = LayerMask.NameToLayer("Outline");

        // Copy MeshFilter
        MeshFilter outlineFilter = outlineObject.AddComponent<MeshFilter>();
        outlineFilter.sharedMesh = meshFilter.sharedMesh;

        // Add MeshRenderer with outline material
        MeshRenderer outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
        outlineRenderer.material = outlineMaterial;
        outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        outlineRenderer.receiveShadows = false;
    }

    private void Update()
    {
        outlineObject.SetActive(outlineFrames > 0);
        outlineFrames--;
    }

    float outlineFrames = 0;
    public void EnableOutline()
    {
        outlineFrames = 3;
    }
}
