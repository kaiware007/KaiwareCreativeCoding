using UnityEngine;
using System.Runtime.InteropServices;

public class VectorField : MonoBehaviour {
    #region define
    const int THREAD_GROUP_X = 32;
    #endregion

    #region public
    public float noiseScale = 1f;
    public float noiseSpeed = 1f;

    public int gridNumX = 8;
    public int gridNumY = 8;
    public int gridNumZ = 8;

    public Vector3 fieldSize = Vector3.one;
    public Vector3 fieldCenter = Vector3.zero;

    public ComputeShader cs;

    public int bufferSize { get { return _bufferSize; } }
    public ComputeBuffer flowGridBuffer { get { return _flowGridBuffer; } }
    #endregion

    #region private
    private ComputeBuffer _flowGridBuffer;
    //private ComputeBuffer centeringFlowBuffer;  // 中央に寄せる力

    private int threadGroupX = 0;
    private int _kernelUpdate;
    private int _bufferSize;
    #endregion

    void Initialize()
    {
        _kernelUpdate = cs.FindKernel("Update");

        _bufferSize = gridNumX * gridNumY * gridNumZ;
        threadGroupX = Mathf.CeilToInt((float)_bufferSize / THREAD_GROUP_X);
        _flowGridBuffer = new ComputeBuffer(_bufferSize, Marshal.SizeOf(typeof(Vector3)));

    }

    void UpdateFlow()
    {
        cs.SetVector("_FieldSize", fieldSize);
        cs.SetVector("_FieldCenter", fieldCenter);

        cs.SetFloat("_NoiseScale", noiseScale);

        cs.SetInt("_GridNumX", gridNumX);
        cs.SetInt("_GridNumY", gridNumY);
        cs.SetInt("_GridNumZ", gridNumZ);

        cs.SetFloat("_Time", Time.time * noiseSpeed);
        cs.SetBuffer(_kernelUpdate, "_FlowGridBuffer", flowGridBuffer);

        cs.Dispatch(_kernelUpdate, threadGroupX, 1, 1);
    }

    void ReleaseBuffer()
    {
        ComputeBuffer[] array = { flowGridBuffer };

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                array[i].Release();
                array[i] = null;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFlow();
    }

    void OnDestroy()
    {
        ReleaseBuffer();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(fieldCenter, fieldSize);
    }
}
