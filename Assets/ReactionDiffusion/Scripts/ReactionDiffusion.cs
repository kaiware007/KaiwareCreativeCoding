using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public struct RDData
{
    public float a;
    public float b;
}

public class ReactionDiffusion : MonoBehaviour {
    const int THREAD_NUM_X = 32;

    public int texWidth = 256;
    public int texHeight = 256;

    public float da = 1;
    public float db = 0.5f;
    [Range(0,0.1f)]
    public float f = 0.055f;
    [Range(0, 0.1f)]
    public float k = 0.062f;
    public float speed = 1;

    public int seedSize = 10;

    public ComputeShader cs;

    public RenderTexture outputTexture;
    
    private int kernelUpdate = -1;
    private int kernelDraw = -1;

    private ComputeBuffer[] buffers;
    
    void Initialize()
    {
        kernelUpdate = cs.FindKernel("Update");
        kernelDraw = cs.FindKernel("Draw");

        outputTexture = new RenderTexture(texWidth, texHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        int wh = texWidth * texHeight;
        buffers = new ComputeBuffer[2];
        
        cs.SetInt("_TexWidth", texWidth);
        cs.SetInt("_TexHeight", texHeight);

        for (int i = 0; i < buffers.Length; i++)
        {
            buffers[i] = new ComputeBuffer(wh, Marshal.SizeOf(typeof(RDData)));
        }

        var bufData = new RDData[texWidth * texHeight];
        var bufData2 = new RDData[texWidth * texHeight];
        for (int x = 0; x < texWidth; x++)
        {
            for (int y = 0; y < texHeight; y++)
            {
                int idx = x + y * texWidth;
                bufData[idx].a = 1;
                bufData[idx].b = 0;

                bufData2[idx].a = 1;
                bufData2[idx].b = 0;

            }
        }

        // 中心あたりに点
        int w = seedSize;
        int h = seedSize;
        int centerX = texWidth / 2 - w / 2;
        int centerY = texHeight / 2 - h / 2;
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                int idx = (centerX + x) + (centerY + y) * texWidth;
                bufData[idx].b = 1;
            }
        }

        buffers[0].SetData(bufData);
        buffers[1].SetData(bufData2);

        var ren = GetComponent<Renderer>();
        if (ren != null)
        {
            ren.material.SetTexture("_MainTex", outputTexture);
        }
    }

    void UpdateBuffer()
    {
        cs.SetInt("_TexWidth", texWidth);
        cs.SetInt("_TexHeight", texHeight);
        cs.SetFloat("_DA", da);
        cs.SetFloat("_DB", db);
        cs.SetFloat("_Feed", f);
        cs.SetFloat("_K", k);
        cs.SetBuffer(kernelUpdate, "_BufferRead", buffers[0]);
        cs.SetBuffer(kernelUpdate, "_BufferWrite", buffers[1]);
        cs.Dispatch(kernelUpdate, Mathf.CeilToInt((float)texWidth / THREAD_NUM_X), Mathf.CeilToInt((float)texHeight / THREAD_NUM_X), 1);

        SwapBuffer();
    }

    void DrawTexture()
    {
        cs.SetInt("_TexWidth", texWidth);
        cs.SetInt("_TexHeight", texHeight);
        cs.SetBuffer(kernelDraw, "_BufferRead", buffers[0]);
        cs.SetTexture(kernelDraw, "_DistTex", outputTexture);
        cs.Dispatch(kernelDraw, Mathf.CeilToInt((float)texWidth / THREAD_NUM_X), Mathf.CeilToInt((float)texHeight / THREAD_NUM_X), 1);
    }

    void SwapBuffer()
    {
        ComputeBuffer temp = buffers[0];
        buffers[0] = buffers[1];
        buffers[1] = temp; 
    }

    // Use this for initialization
    void Start () {
        Initialize();
	}
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < speed; i++)
        {
            UpdateBuffer();
        }

        DrawTexture();
    }

    private void OnDestroy()
    {
        if(buffers != null)
        {
            for(int i = 0; i < buffers.Length; i++)
            {
                buffers[i].Release();
                buffers[i] = null;
            }
        }
    }
}
