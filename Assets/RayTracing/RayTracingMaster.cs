using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour {
    [SerializeField] private ComputeShader rayTracingShader;

    private RenderTexture target;
    private new Camera camera;

    private void Awake() {
        camera = GetComponent<Camera>();
    }

    private void SetShaderParameters() {
        rayTracingShader.SetMatrix("_CameraToWorld", camera.cameraToWorldMatrix);
        rayTracingShader.SetMatrix("_CameraInverseProjection", camera.projectionMatrix.inverse);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        SetShaderParameters();
        Render(destination);
    }

    private void Render(RenderTexture destination) {
        InitRenderTexture();

        rayTracingShader.SetTexture(0, "Result", target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8f);

        var offsetBuffer = SeupOffsetBuffer();

        rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        offsetBuffer.Dispose();

        Graphics.Blit(target, destination);
    }

    private void InitRenderTexture() {
        if(target == null || target.width != Screen.width || target.height != Screen.height) {
            if (target != null) target.Release();

            target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    }

    private ComputeBuffer SeupOffsetBuffer() {
        var size = 5;
        var offsets = new Vector2[size * size];
        var idx = 0;
        for (float x = 0; x < size; x++) {
            for (float y = 0; y < size; y++) {
                offsets[idx] = new Vector2(x / size, y / size);
                idx++;
            }
        }
        var offsetBuffer = new ComputeBuffer(offsets.Length, 2 * 4);
        offsetBuffer.SetData(offsets);

        rayTracingShader.SetBuffer(0, "offsets", offsetBuffer);
        rayTracingShader.SetFloat("offsetLength", offsets.Length);

        return offsetBuffer;
    }
}
