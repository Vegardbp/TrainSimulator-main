using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
public class RandomizeRotationOnSpawn : MonoBehaviour
{
    public Vector2 randomRangeRight = new Vector2(-5f, 5f);
    public Vector2 randomRangeUp = new Vector2(0f, 360f);
    public Vector2 randomRangeForward = new Vector2(-5f, 5f);

    public Vector2 sizeRange = new Vector2(0.8f, 1.2f);

    public bool alignToGroundNormal = true;
    public float raycastDistance = 5f;
    public LayerMask groundLayers = ~0; // Default to everything

#if UNITY_EDITOR
    private int _framesWaited = 0;
    private bool _finalized = false;

    private void OnEnable()
    {
        if (Application.isPlaying)
            return;

        EditorApplication.update += EditorUpdate;


        initialRandomRotation = Quaternion.identity;
        initialRandomRotation *= Quaternion.AngleAxis(Random.Range(randomRangeUp.x, randomRangeUp.y), Vector3.up);
        initialRandomRotation *= Quaternion.AngleAxis(Random.Range(randomRangeRight.x, randomRangeRight.y), Vector3.right);
        initialRandomRotation *= Quaternion.AngleAxis(Random.Range(randomRangeForward.x, randomRangeForward.y), Vector3.forward);

        float s = Random.Range(sizeRange.x, sizeRange.y);
        transform.localScale = new Vector3(s,s,s);
    }

    private void EditorUpdate()
    {
        if (this == null)
        {
            EditorApplication.update -= EditorUpdate;
            return;
        }

        if (_finalized)
            return;

        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            return; // Still editing prefab

        _framesWaited++;

        if (alignToGroundNormal)
            AlignToGround();

        if (Selection.activeGameObject == gameObject && _framesWaited > 5)
        {
            FinalizePlacement();
            return;
        }
    }

    private Quaternion initialRandomRotation;

    private void AlignToGround()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 10, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, raycastDistance + 10f, groundLayers);

        foreach (var hit in hits.OrderBy(h => h.distance))
        {
            if (hit.collider != null && hit.collider.transform.root != transform.root)
            {
                Quaternion alignRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                transform.rotation = alignRotation * initialRandomRotation;
                transform.position = hit.point;
                break;
            }
        }
    }

    private void FinalizePlacement()
    {
        _finalized = true;
        EditorApplication.update -= EditorUpdate;
        DestroyImmediate(this);
    }
#endif
}
