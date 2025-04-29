using System.Collections.Generic;
using UnityEngine;

public class GrassSpawnHandler : MonoBehaviour
{
    ComputeBuffer grassBuffer;

    GraphicsBuffer commandBuf;
    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    ComputeBuffer highLODGrassBuffer;
    ComputeBuffer highLODGrassCount;
    public uint count = 1000; // Number of grass blades
    public bool regenerate;
    public LayerMask ground;
    public float size = 200; // Terrain area size
    public float heightAdder = 0.2f;
    public Mesh grassMesh; // Assign in inspector
    public Material grassMaterial; // Assign in inspector

    public float closeDistance = 50;
    public float farDistance = 150;

    public float density;

    public ComputeShader GrassCull;
    const int commandCount = 1;
    void Start()
    {
        FillBuffer();
    }

    void Update()
    {
        if (regenerate)
        {
            FillBuffer();
            regenerate = false;
        }
        RenderGrass();
    }

    void FillBuffer()
    {
        Release();

        grassBuffer = new ComputeBuffer((int)(count*2), sizeof(float) * 8);
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
        int i = 0;
        while (bufferData.Count < count * 8)
        {
            i++;
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
                }
            }
            if(i > count * 10)
            {
                AddGrass(Vector3.zero ,Quaternion.identity);
                break;
            }
        }

        grassBuffer.SetData(bufferData);

        commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];

        highLODGrassBuffer = new ComputeBuffer((int)count, sizeof(int), ComputeBufferType.Append);
        highLODGrassCount = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    }

    void RenderGrass()
    {
        if (grassMesh == null || grassMaterial == null) return;

        highLODGrassBuffer.SetCounterValue(0);

        GrassCull.SetBuffer(0, "highLODGrassBuffer", highLODGrassBuffer);
        GrassCull.SetBuffer(0, "allGrassPositions", grassBuffer);

        int localGrassCount = (int)(count * density);

        GrassCull.SetInt("count", localGrassCount);
        GrassCull.SetVector("camPos", Camera.main.transform.position);
        GrassCull.SetFloat("alpha", density);

        GrassCull.SetFloat("closeDst", closeDistance * density);
        GrassCull.SetFloat("farDst", farDistance * density);

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Vector4[] frustumPlanes = new Vector4[6];
        for (int i = 0; i < 6; i++)
        {
            planes[i].distance += 3;
            frustumPlanes[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
        }

        GrassCull.SetVectorArray("frustumPlanes", frustumPlanes);

        GrassCull.Dispatch(0, Mathf.Max(Mathf.CeilToInt(localGrassCount / 528),1), 1, 1);

        ComputeBuffer.CopyCount(highLODGrassBuffer, highLODGrassCount, 0);
        int[] highCount = { 0 };
        highLODGrassCount.GetData(highCount);
        int highLODInstanceCount = highCount[0];

        grassMaterial.SetBuffer("_GrassBuffer", grassBuffer);
        grassMaterial.SetBuffer("_highLODGrassIndicies", highLODGrassBuffer);
        RenderParams rp = new RenderParams(grassMaterial);
        rp.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one);
        rp.matProps = new MaterialPropertyBlock();
        rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));
        commandData[0].indexCountPerInstance = grassMesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)highLODInstanceCount;
        commandBuf.SetData(commandData);

        UnityEngine.Graphics.RenderMeshIndirect(rp, grassMesh, commandBuf, commandCount);
    }

    void OnDestroy()
    {
        Release();
    }

    void Release()
    {
        grassBuffer?.Release();
        commandBuf?.Release();
        highLODGrassBuffer?.Release();
        highLODGrassCount?.Release();
    }
}
