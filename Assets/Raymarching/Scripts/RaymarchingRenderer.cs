using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

//[ExecuteInEditMode]
public abstract class RaymarchingRenderer : MonoBehaviour
{

    public bool useCommandBuffer = true;

    protected Dictionary<Camera, CommandBuffer> cameras_ = new Dictionary<Camera, CommandBuffer>();
    protected Mesh quad_;

    [SerializeField]
    protected Material material = null;
    [SerializeField]
    protected CameraEvent pass = CameraEvent.BeforeGBuffer;

    protected Mesh GenerateQuad()
    {
        var mesh = new Mesh();
        mesh.vertices = new Vector3[4] {
            new Vector3( 1.0f , 1.0f,  0.0f),
            new Vector3(-1.0f , 1.0f,  0.0f),
            new Vector3(-1.0f ,-1.0f,  0.0f),
            new Vector3( 1.0f ,-1.0f,  0.0f),
        };
        mesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
        return mesh;
    }

    protected void CleanUp()
    {
        foreach (var pair in cameras_) {
            var camera = pair.Key;
            var buffer = pair.Value;
            if (camera) {
                camera.RemoveCommandBuffer(pass, buffer);
            }
        }
        cameras_.Clear();
    }

    //protected void OnEnable()
    //{
    //    CleanUp();
    //}

    //protected void OnDisable()
    //{
    //    CleanUp();
    //}

    void Start()
    {
        CleanUp();

        //if (!quad_) quad_ = GenerateQuad();
    }

    void OnWillRenderObject()
    {
        if (useCommandBuffer)
        {
            UpdateCommandBuffer();
        }
        //else
        //{
        //    var act = gameObject.activeInHierarchy && enabled;
        //    if (!act)
        //    {
        //        //OnDisable();
        //        return;
        //    }

        //    var camera = Camera.current;
        //    if (!camera || ((Camera.current.cullingMask & (1 << gameObject.layer)) == 0)) return;
            
        //    if (!quad_) quad_ = GenerateQuad();
        //    Graphics.DrawMesh(quad_, transform.localToWorldMatrix, material, gameObject.layer);
        //}
    }

    private void OnRenderObject()
    {
        if (!useCommandBuffer)
        {
            var act = gameObject.activeInHierarchy && enabled;
            if (!act)
            {
                //OnDisable();
                return;
            }

            var camera = Camera.current;
            if (!camera || ((Camera.current.cullingMask & (1 << gameObject.layer)) == 0)) return;

            if (!quad_) quad_ = GenerateQuad();

            material.SetPass(0);
            Graphics.DrawMeshNow(quad_, transform.localToWorldMatrix, 0);
            //buffer.DrawMesh(quad_, Matrix4x4.identity, material, 0, 0);

        }
    }

    void UpdateCommandBuffer()
    {
        var act = gameObject.activeInHierarchy && enabled;
        if (!act) {
            //OnDisable();
            return;
        }

        var camera = Camera.current;
        if (!camera || cameras_.ContainsKey(camera) || ((Camera.current.cullingMask & (1 << gameObject.layer)) == 0)) return;

        if (!quad_) quad_ = GenerateQuad();

        CreateCommandBuffer(camera);
    }

    protected virtual void CreateCommandBuffer(Camera camera)
    {
        var buffer = new CommandBuffer();
        buffer.name = "Raymarching";
        buffer.DrawMesh(quad_, Matrix4x4.identity, material, 0, 0);
        camera.AddCommandBuffer(pass, buffer);
        cameras_.Add(camera, buffer);
    }
}
