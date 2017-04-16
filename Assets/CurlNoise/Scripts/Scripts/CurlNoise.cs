using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public struct CurlParticleData
{
    public bool isActive;       // 有効フラグ
    public Vector3 position;    // 座標
    public Vector3 velocity;    // 加速度
    public Color color;         // 色
    public float duration;      // 生存時間
    public float scale;         // サイズ
}

public class CurlNoise : GPUParticleBase<CurlParticleData>
{

    #region public
    public float velocityMax = 1000f;
    public float lifeTime = 1;
    public float scaleMin = 1;
    public float scaleMax = 2;
    public float gravity = 9.8f;

    [Range(0,1)]
    public float flowPower = 1;
    [Range(0, 1)]
    public float sai = 1;   // 彩度
    [Range(0, 1)]
    public float val = 1;   // 明るさ

    //public Camera camera;
    public List<Vector3> emitPoints;
    public float emitRange = 1;
    public int onePositionEmitNum = 32; // emitPointsごとのパーティクル発生数
    public Color color = Color.red;

    public VectorField vf;
    #endregion

    #region pirvate
    private ComputeBuffer emitBuffer;
    private Vector3[] emitArray;
    private int emitCountIndex;
    #endregion

    /// <summary>
    /// 初期化
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();

        emitBuffer = new ComputeBuffer(emitNum, Marshal.SizeOf(typeof(Vector3)), ComputeBufferType.Default);
        emitArray = new Vector3[emitNum];
    }

    /// <summary>
    /// パーティクルの更新
    /// </summary>
    protected override void UpdateParticle()
    {
        particleActiveBuffer.SetCounterValue(0);

        cs.SetInt("_GridNumX", vf.gridNumX);
        cs.SetInt("_GridNumY", vf.gridNumY);
        cs.SetInt("_GridNumZ", vf.gridNumZ);
        cs.SetFloat("_DT", Time.deltaTime);
        cs.SetFloat("_LifeTime", lifeTime);
        cs.SetFloat("_Gravity", gravity);
        cs.SetFloat("_FlowPower", flowPower);
        cs.SetVector("_FieldSize", vf.fieldSize);
        cs.SetVector("_FieldCenter", vf.fieldCenter);
        cs.SetBuffer(updateKernel, "_Particles", particleBuffer);
        cs.SetBuffer(updateKernel, "_DeadList", particlePoolBuffer);
        cs.SetBuffer(updateKernel, "_ActiveList", particleActiveBuffer);
        cs.SetBuffer(updateKernel, "_FlowGridBuffer", vf.flowGridBuffer);

        cs.Dispatch(updateKernel, particleNum / THREAD_NUM_X, 1, 1);

        particleActiveCountBuffer.SetData(particleCounts);
        ComputeBuffer.CopyCount(particleActiveBuffer, particleActiveCountBuffer, 0);
        particleActiveCountBuffer.GetData(particleCounts);
        particleActiveNum = particleCounts[0];
    }

    /// <summary>
    /// パーティクルの発生
    /// THREAD_NUM_X分発生
    /// </summary>
    protected override void EmitParticle()
    {
        particlePoolCountBuffer.SetData(particleCounts);
        ComputeBuffer.CopyCount(particlePoolBuffer, particlePoolCountBuffer, 0);
        particlePoolCountBuffer.GetData(particleCounts);
        //Debug.Log("EmitParticle Pool Num " + particleCounts[0] + " position " + position);
        particlePoolNum = particleCounts[0];

        if (particleCounts[0] < emitCountIndex)
        {
            emitCountIndex = 0;
            return;   // emitNum未満なら発生させない
        }

        emitBuffer.SetData(emitArray);

        //cs.SetVector("_EmitPosition", position);
        cs.SetInt("_EmitCount", emitCountIndex);
        cs.SetFloat("_VelocityMax", velocityMax);
        cs.SetFloat("_LifeTime", lifeTime);
        cs.SetFloat("_ScaleMin", scaleMin);
        cs.SetFloat("_ScaleMax", scaleMax);
        cs.SetFloat("_Sai", sai);
        cs.SetFloat("_Val", val);
        cs.SetFloat("_Time", Time.time);
        cs.SetVector("_EmitColor", color);
        cs.SetBuffer(emitKernel, "_ParticlePool", particlePoolBuffer);
        cs.SetBuffer(emitKernel, "_Particles", particleBuffer);
        cs.SetBuffer(emitKernel, "_EmitBuffer", emitBuffer);

        //cs.Dispatch(emitKernel, particleCounts[0] / THREAD_NUM_X, 1, 1);
        cs.Dispatch(emitKernel, Mathf.CeilToInt((float)emitCountIndex / THREAD_NUM_X), 1, 1);   // emitNumの数だけ発生

        emitCountIndex = 0;
    }

    void SetEmitParticle(Vector3 pos)
    {
        if (emitCountIndex >= emitNum) return;
        emitArray[emitCountIndex] = pos;
        emitCountIndex++;
    }

    // Update is called once per frame
    protected override void Update()
    {
        //if (Input.GetMouseButton(0))
        //{
        //    //Vector3 mpos = Input.mousePosition;
        //    //mpos.z = 10;
        //    //Vector3 pos = camera.ScreenToWorldPoint(mpos);
        //    EmitParticle(pos);
        //}
        Vector3 transPos = transform.position;
        Vector3 tmp = Vector3.zero;
        Matrix4x4 mat = transform.localToWorldMatrix;
        for(int i = 0; i < emitPoints.Count; i++)
        {
            Vector3 pos = emitPoints[i];
            for (int j = 0; j < onePositionEmitNum; j++)
            {
                Vector2 rad = Random.insideUnitCircle * emitRange;
                //Vector2 rad = new Vector2(emitRange, 0);
                tmp.x = pos.x + rad.x;
                tmp.y = pos.y;
                tmp.z = pos.z + rad.y;
                //pos = transform.rotation * tmp;
                tmp = mat * tmp;
                SetEmitParticle(tmp + transPos);
            }
        }

        EmitParticle();

        UpdateParticle();
    }

    protected override void ReleaseBuffer()
    {
        base.ReleaseBuffer();
        if(emitBuffer != null)
        {
            emitBuffer.Release();
            emitBuffer = null;
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.red;
        //Quaternion q = transform.rotation;
        UnityEditor.Handles.matrix = transform.localToWorldMatrix;
        for (int i = 0; i < emitPoints.Count; i++)
        {
            //Gizmos.draw
            UnityEditor.Handles.DrawWireDisc(emitPoints[i], Vector3.up, emitRange);
        }
    }
#endif
}
