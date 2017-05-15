using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

public class CloudRaymarching : RaymarchingRenderer {
    #region define
    public struct ShaderData
    {
        public float noiseScale;
        public float clipThreashold;
        public float noiseRange;
    }
    #endregion

    #region public    
    [Range(0,1000)]
    public float noiseScale = 1;

    /// <summary>
    /// クリッピングする閾値
    /// </summary>
    [Range(0,1)]
    public float clipThreashold = 0.5f;

    public float noiseRange = 1;
    #endregion

    ShaderData[] shaderData;
    ComputeBuffer shaderDataBuffer;

    protected override void CreateCommandBuffer(Camera camera)
    {
        var buffer = new CommandBuffer();
        buffer.name = "Raymarching";
        if (Application.isPlaying && shaderData != null)
        {
            Debug.Log("SetBuffer ShaderData");
            material.SetBuffer("_ShaderData", shaderDataBuffer);
        }
        buffer.DrawMesh(quad_, Matrix4x4.identity, material, 0, 0);
        camera.AddCommandBuffer(pass, buffer);
        cameras_.Add(camera, buffer);
    }

    void Init()
    {
        shaderData = new ShaderData[1];
        shaderDataBuffer = new ComputeBuffer(shaderData.Length, Marshal.SizeOf(typeof(ShaderData)));
    }

    void UpdateShaderData()
    {
        if (shaderData != null)
        {
            shaderData[0].noiseScale = noiseScale;
            shaderData[0].clipThreashold = clipThreashold;
            shaderData[0].noiseRange = noiseRange;
            shaderDataBuffer.SetData(shaderData);

            if (!useCommandBuffer)
            {
                //if (!quad_) quad_ = GenerateQuad();
                material.SetBuffer("_ShaderData", shaderDataBuffer);
                
                //Graphics.DrawMesh(quad_, transform.localToWorldMatrix, material, 0);
            }
        }
    }

    // Use this for initialization
    void Start () {
        Init();
        CleanUp();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateShaderData();
    }

    private void OnDestroy()
    {
        if (shaderDataBuffer != null)
        {
            shaderDataBuffer.Release();
            shaderDataBuffer = null;
        }
    }
}
