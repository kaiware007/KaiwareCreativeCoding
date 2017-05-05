using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct StarData
{
    public Vector3 position;    // 座標
    public Color color;         // 色
    public float scale;         // サイズ
}

public class StartParticle : GPUParticleBase<StarData> {

    public float emitRadius = 1000;
    public Vector2 scaleMinMax = Vector2.zero;

    StarData[] starDataArray = null;

    // Use this for initialization
    void Start () {
        
        // 初期化
        starDataArray = new StarData[particleNum];
        for (int i = 0; i < particleNum; i++)
        {
            starDataArray[i].position = UnityEngine.Random.insideUnitSphere * emitRadius;
            starDataArray[i].color = Color.white;
            starDataArray[i].scale = UnityEngine.Random.Range(scaleMinMax.x, scaleMinMax.y);
        }

        particleBuffer.SetData(starDataArray);

        cs.SetBuffer(initKernel, "_Particles", particleBuffer);
        cs.SetBuffer(initKernel, "_ActiveList", particleActiveBuffer);
        cs.Dispatch(initKernel, particleNum / THREAD_NUM_X, 1, 1);

        particleActiveCountBuffer.SetData(particleCounts);
        ComputeBuffer.CopyCount(particleActiveBuffer, particleActiveCountBuffer, 0);
        particleActiveCountBuffer.GetData(particleCounts);
        particleActiveNum = particleCounts[0];
        Debug.Log("particleActiveNum " + particleActiveNum);
    }

    // Update is called once per frame
    override protected void Update () {
        //UpdateParticle();
    }

    protected override void UpdateParticle()
    {
        //cs.Dispatch(updateKernel, particleNum / THREAD_NUM_X, 1, 1);
    }
}
