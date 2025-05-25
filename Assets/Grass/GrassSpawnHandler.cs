using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

public class GrassSpawnHandler : MonoBehaviour
{
    ComputeBuffer grassBuffer;

    GraphicsBuffer highCommandBuf;
    GraphicsBuffer.IndirectDrawIndexedArgs[] highCommandData;
    ComputeBuffer highLODBuffer;
    public uint count = 1000; // Number of grass blades
    public bool regenerate;
    public LayerMask ground;
    public float size = 200; // Terrain area size
    public float heightAdder = 0.2f;
    public Mesh highLODGrassMesh; // Assign in inspector
    public Material grassMaterial; // Assign in inspector


    public float closeDistance = 50;
    public float farDistance = 150;

    public float density;

    public int framesForGeneration = 60;

    public bool useCPUFrustrum = true;

    public ComputeShader GrassCull;

    public int frustrumLeeway = 6;

    private Bounds bounds;

    void Start()
    {
        StartCoroutine(FillBuffer());

        bounds = new Bounds(transform.position, new Vector3(size * 2, 1000f, size * 2));
    }

    int localCount = 0;

    void Update()
    {
        if (regenerate)
            StartCoroutine(FillBuffer());
        regenerate = false;
        RenderGrass();
    }

    RenderParams highRp;

    bool buffersGenerated = false;

    IEnumerator FillBuffer()
    {
        buffersGenerated = false;
        Release();

        highRp = new RenderParams(grassMaterial)
        {
            worldBounds = new Bounds(transform.position, new Vector3(size * 2, 10000, size * 2)),
            matProps = new MaterialPropertyBlock()
        };

        List<float> bufferData = new();

        void AddGrass(Vector3 pos, Quaternion rot)
        {
            float verticalRotation = Random.Range(0f, 360f);
            bufferData.Add(pos.x);
            bufferData.Add(pos.y);
            bufferData.Add(pos.z);
            bufferData.Add(0);
            bufferData.Add(rot.x);
            bufferData.Add(rot.y);
            bufferData.Add(rot.z);
            bufferData.Add(rot.w);
        }

        int attempts = 0;
        int validHits = 0;
        int attemptsThisFrame = 0;

        while (attempts < count && validHits < count)
        {
            attempts++;
            attemptsThisFrame++;
            Vector3 rayStart = transform.position + new Vector3(Random.Range(-size, size), 500, Random.Range(-size, size));
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity, ground))
            {
                if (hit.collider.tag == "terrain")
                {
                    Vector3 pos = hit.point + Vector3.up * heightAdder;
                    Vector3 normal = hit.normal;
                    Vector3 forward = Vector3.Cross(normal, Vector3.right);
                    if (forward.sqrMagnitude < 0.01f)
                        forward = Vector3.Cross(normal, Vector3.forward);
                    forward.Normalize();
                    float randomAngle = Random.Range(0f, 360f);
                    forward = Quaternion.AngleAxis(randomAngle, normal) * forward;
                    Quaternion rotation = Quaternion.LookRotation(forward, normal);
                    AddGrass(pos, rotation);
                    validHits++;
                }
            }
            if (attemptsThisFrame > count / framesForGeneration)
            {
                attemptsThisFrame = 0;
                yield return null;
            }
        }

        // If we have fewer hits than count, adjust count here (optional)
        localCount = validHits;

        if (localCount >= 10)
        {
            grassBuffer = new ComputeBuffer((int)(localCount), sizeof(float) * 8);
            grassBuffer.SetData(bufferData);

            highCommandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            highCommandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];

            highLODBuffer = new ComputeBuffer(localCount + 1, sizeof(int), ComputeBufferType.Append);

            highCommandData[0].indexCountPerInstance = highLODGrassMesh.GetIndexCount(0);
            highCommandData[0].instanceCount = 1;
            highCommandBuf.SetData(highCommandData);

            buffersGenerated = true;
        }
    }

    void RenderGrass()
    {
        if (!buffersGenerated) return;
        if (highLODGrassMesh == null || grassMaterial == null) return;
        if (density <= 0.05f) return;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Vector4[] frustumPlanes = new Vector4[6];
        for (int i = 0; i < 6; i++)
        {
            float leeway = (i == 0 || i == 1 || i == 2 || i == 3) ? frustrumLeeway : 0f;
            planes[i].distance += leeway;
            frustumPlanes[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
        }

        if (useCPUFrustrum && !GeometryUtility.TestPlanesAABB(planes, bounds))
            return;


        highLODBuffer.SetCounterValue(0);

        GrassCull.SetBuffer(0, "highLODGrassBuffer", highLODBuffer);
        GrassCull.SetBuffer(0, "allGrassPositions", grassBuffer);

        int localGrassCount = (int)(localCount * density);
        GrassCull.SetInt("count", localGrassCount);
        GrassCull.SetVector("camPos", Camera.main.transform.position);
        GrassCull.SetFloat("alpha", density);
        GrassCull.SetFloat("closeDst", closeDistance * density);
        GrassCull.SetFloat("farDst", farDistance * density);

        GrassCull.SetVectorArray("frustumPlanes", frustumPlanes);

        int threadGroups = Mathf.FloorToInt(localGrassCount / 528f) + 1;
        threadGroups = Mathf.Max(threadGroups, 1);
        GrassCull.Dispatch(0, threadGroups, 1, 1);

        highRp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));
        highRp.matProps.SetBuffer("_GrassBuffer", grassBuffer);
        highRp.matProps.SetBuffer("_highLODGrassIndicies", highLODBuffer);

        GraphicsBuffer.CopyCount(highLODBuffer, highCommandBuf, sizeof(uint));

        Graphics.RenderMeshIndirect(highRp, highLODGrassMesh, highCommandBuf, 1);

    }

    void OnDestroy()
    {
        Release();
    }

    void Release()
    {
        grassBuffer?.Release();
        highCommandBuf?.Release();
        highLODBuffer?.Release();
    }
}
