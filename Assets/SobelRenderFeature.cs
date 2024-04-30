using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class SobelRenderFeature : ScriptableRendererFeature
{

    private SobelPass sobelPass;
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

}
