using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public struct CircleData
{
    public bool isActive;       // 有効フラグ
    public Vector2 position;    // 座標
    public float radius;        // 半径
    public float maxRadius;     // 最大半径
    public float duration;      // 生存時間
}

public class Circle : GPUParticleBase<CircleData>
{

    #region public
    public float lifeTime = 1;
    public Vector2 radiusRange = Vector2.one;

    //public Camera camera;
    //public List<Vector2> emitPoints;
    public float emitRange = 1;
    public int onePositionEmitNum = 32; // emitPointsごとのパーティクル発生数
    public float emitInterval = 1;
    #endregion

    #region pirvate
    private ComputeBuffer emitBuffer;
    private Vector2[] emitArray;
    private int emitCountIndex;
    private float emitDuration = 0;
    #endregion

    /// <summary>
    /// 初期化
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();

        emitBuffer = new ComputeBuffer(emitNum, Marshal.SizeOf(typeof(Vector2)), ComputeBufferType.Default);
        emitArray = new Vector2[emitNum];
    }

    /// <summary>
    /// パーティクルの更新
    /// </summary>
    protected override void UpdateParticle()
    {
        particleActiveBuffer.SetCounterValue(0);

        cs.SetFloat("_DT", Time.deltaTime);
        cs.SetFloat("_LifeTime", lifeTime);
        cs.SetBuffer(updateKernel, "_Particles", particleBuffer);
        cs.SetBuffer(updateKernel, "_DeadList", particlePoolBuffer);
        cs.SetBuffer(updateKernel, "_ActiveList", particleActiveBuffer);

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
        cs.SetFloat("_LifeTime", lifeTime);
        cs.SetFloat("_Time", Time.time);
        cs.SetFloat("_ScaleMin", radiusRange.x);
        cs.SetFloat("_ScaleMax", radiusRange.y);
        cs.SetBuffer(emitKernel, "_ParticlePool", particlePoolBuffer);
        cs.SetBuffer(emitKernel, "_Particles", particleBuffer);
        cs.SetBuffer(emitKernel, "_EmitBuffer", emitBuffer);
        
        //cs.Dispatch(emitKernel, particleCounts[0] / THREAD_NUM_X, 1, 1);
        cs.Dispatch(emitKernel, Mathf.CeilToInt((float)emitCountIndex / THREAD_NUM_X), 1, 1);   // emitNumの数だけ発生

        emitCountIndex = 0;
    }

    void SetEmitParticle(Vector2 pos)
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
        if (Input.GetMouseButtonDown(0))
        {
            //Vector2 transPos = transform.position;
            //Vector2 tmp = Vector3.zero;
            Matrix4x4 mat = transform.localToWorldMatrix;
            for (int j = 0; j < onePositionEmitNum; j++)
            {
                Vector2 rad = Random.insideUnitCircle * emitRange * 0.5f + Vector2.one * 0.5f;
                //Vector2 rad = new Vector2(emitRange, 0);
                //tmp.x = pos.x + rad.x;
                //tmp.y = pos.y + rad.y;
                //pos = transform.rotation * tmp;
                //tmp = mat * tmp;
                SetEmitParticle(rad);
            }
            EmitParticle();
        }


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

//    private void OnDrawGizmos()
//    {
//#if UNITY_EDITOR
//        UnityEditor.Handles.color = Color.red;
//        //Quaternion q = transform.rotation;
//        UnityEditor.Handles.matrix = transform.localToWorldMatrix;
//        for (int i = 0; i < emitPoints.Count; i++)
//        {
//            //Gizmos.draw
//            UnityEditor.Handles.DrawWireDisc(emitPoints[i], Vector3.up, emitRange);
//        }
//#endif
//    }
}
