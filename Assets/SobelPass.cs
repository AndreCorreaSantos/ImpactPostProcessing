using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class SobelPass : ScriptableRenderPass
{
    ComputeShader sobelShader;
    RTHandle temporaryColorTexture;
    RTHandle source;



    public SobelPass(ComputeShader shader)
    {
        sobelShader = shader;
    }

    public void Setup(RTHandle source)
    {
        this.source = source;
        if (temporaryColorTexture == null)
        {
            temporaryColorTexture = RTHandles.Alloc(
                scaleFactor: Vector2.one,
                slices: TextureXR.slices,
                dimension: TextureDimension.Tex2D,
                colorFormat: GraphicsFormat.R8G8B8A8_UNorm,
                enableRandomWrite: true,
                useDynamicScale: true,
                name: "SobelFilterOutputTexture"
            );
        }
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (sobelShader == null || temporaryColorTexture == null ) return;

        CommandBuffer cmd = CommandBufferPool.Get("SobelFilter");
        int kernelHandle = sobelShader.FindKernel("CSMain");
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;

        // Set the compute shader parameters
        cmd.SetComputeTextureParam(sobelShader, kernelHandle, "Input", source);
        cmd.SetComputeTextureParam(sobelShader, kernelHandle, "Output", temporaryColorTexture);
        cmd.SetComputeFloatParam(sobelShader,"Time",Time.time);
        cmd.SetComputeFloatParam(sobelShader,"Threshold",0.2f);
        // Dispatch the compute shader
        int groupX = Mathf.CeilToInt(descriptor.width / 16.0f);
        int groupY = Mathf.CeilToInt(descriptor.height / 16.0f);
        cmd.DispatchCompute(sobelShader, kernelHandle, groupX, groupY, 1);

        // Blit the result from temporaryColorTexture to the source
        cmd.Blit(temporaryColorTexture, source);

        // Execute the command buffer
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        if (temporaryColorTexture != null)
        {
            RTHandles.Release(temporaryColorTexture);
            temporaryColorTexture = null;
        }
    }
}