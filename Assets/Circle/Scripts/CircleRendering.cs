using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleRendering : MonoBehaviour {
    
    #region protected
    protected GPUParticleBase<CircleData> particle;
    protected int particleNum;
    protected ComputeBuffer particleBuffer;
    protected ComputeBuffer activeIndexBuffer;
    protected ComputeBuffer activeCountBuffer;
    protected Material material;
    #endregion

    protected virtual void Start()
    {
        particle = GetComponent<GPUParticleBase<CircleData>>();
        if (particle != null)
        {
            particleNum = particle.GetParticleNum();
            particleBuffer = particle.GetParticleBuffer();
            activeIndexBuffer = particle.GetActiveParticleBuffer();
            activeCountBuffer = particle.GetParticleCountBuffer();
            Debug.Log("particleNum " + particleNum);
        }
        else
        {
            Debug.LogError("Particle Class Not Found!!" + typeof(GPUParticleBase<CircleData>).FullName);
        }

        material = GetComponent<Renderer>().material;

        SetMaterialParam();
    }

    protected void SetMaterialParam()
    {
        material.SetBuffer("_Particles", particleBuffer);
        //material.SetBuffer("_ParticleActiveList", activeIndexBuffer);
        material.SetInt("_ParticleNum", particleNum);
        //material.SetPass(0);
    }

    private void Update()
    {
        //SetMaterialParam();
    }
}
