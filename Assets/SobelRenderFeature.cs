using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class SobelRenderFeature : ScriptableRendererFeature
{
    public bool isEnabled = false;

    SobelPass sobelPass;
    public ComputeShader sobelShader;

    public override void Create()
    {
        sobelPass = new SobelPass(sobelShader)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(sobelPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        var stack = VolumeManager.instance.stack;
        var customSobel = stack.GetComponent<CustomSobel>();
        if (customSobel != null && customSobel.IsActive())
        {
            sobelPass.Setup(renderer.cameraColorTargetHandle);
        }
    }
}
