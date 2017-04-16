using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowGridDebugRendering : MonoBehaviour {

    public VectorField vf;
    public Material material;
    public bool idDraw = false;

    private Vector3 gridCenter;

    private void Start()
    {
        gridCenter.x = vf.fieldSize.x / vf.gridNumX / 2f;
        gridCenter.y = vf.fieldSize.y / vf.gridNumY / 2f;
        gridCenter.z = vf.fieldSize.z / vf.gridNumZ / 2f;
    }

    void OnRenderObject()
    {
        if (idDraw)
        {
            var buffer = vf.flowGridBuffer;
            int num = vf.bufferSize;

            material.SetPass(0);

            material.SetInt("_GridNumX", vf.gridNumX);
            material.SetInt("_GridNumY", vf.gridNumY);
            material.SetInt("_GridNumZ", vf.gridNumZ);

            material.SetVector("_WallSize", vf.fieldSize);
            material.SetVector("_WallCenter", vf.fieldCenter);
            material.SetVector("_GridCenter", gridCenter);
            material.SetBuffer("_BufferRead", buffer);

            Graphics.DrawProcedural(MeshTopology.Points, num);
        }
    }
}
