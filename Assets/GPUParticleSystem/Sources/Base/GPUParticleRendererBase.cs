﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GPUParticleのレンダリング処理
/// </summary>
/// <typeparam name="T"></typeparam>
public class GPUParticleRendererBase <T> : MonoBehaviour where T : struct {

    #region public
    public Material material;
    #endregion

    #region protected
    protected GPUParticleBase<T> particle;
    protected int particleNum;
    protected ComputeBuffer particleBuffer;
    protected ComputeBuffer activeIndexBuffer;
    protected ComputeBuffer activeCountBuffer;
    #endregion

    #region virtual
    protected virtual void SetMaterialParam() { }

    protected virtual void Start()
    {
        particle = GetComponent<GPUParticleBase<T>>();
        if (particle != null)
        {
            particleNum = particle.GetParticleNum();
            particleBuffer = particle.GetParticleBuffer();
            activeIndexBuffer = particle.GetActiveParticleBuffer();
            activeCountBuffer = particle.GetParticleCountBuffer();
            Debug.Log("particleNum " + particleNum);
        }else
        {
            Debug.LogError("Particle Class Not Found!!" + typeof(GPUParticleBase<T>).FullName);
        }
    }

    protected virtual void OnRenderObjectInternal()
    {
        SetMaterialParam();

        material.DisableKeyword("GPUPARTICLE_CULLING_ON");

        Graphics.DrawProceduralIndirect(MeshTopology.Points, activeCountBuffer);
    }
    #endregion

    #region private
    void OnRenderObject()
    {
        OnRenderObjectInternal();
    }
    #endregion
}
