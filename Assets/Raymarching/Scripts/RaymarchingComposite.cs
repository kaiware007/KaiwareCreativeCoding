using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaymarchingComposite : MonoBehaviour {

    public RenderTexture cloudTex;
    public Material material;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetTexture("_CloudTex", cloudTex);
        Graphics.Blit(source, destination, material);
    }
}
