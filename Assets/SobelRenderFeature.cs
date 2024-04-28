using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SobelRenderFeature : ScriptableRendererFeature
{
    class SobelPass : ScriptableRenderPass
    {
        ComputeShader sobelShader;
        RenderTargetIdentifier source;
        RenderTargetHandle temporaryColorTexture;

        public SobelPass(ComputeShader shader)
        {
            sobelShader = shader;
            temporaryColorTexture.Init("_TemporaryColorTexture");
        }

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("SobelFilter");

            int kernelHandle = sobelShader.FindKernel("CSMain");

            // Get the camera texture descriptor for creating temporary textures
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;  // Ensure depth buffer bits are zero for a 2D effect

            // Set input and output textures for your compute shader
            cmd.GetTemporaryRT(temporaryColorTexture.id, descriptor, FilterMode.Point); // Avoid smoothing with Point filtering
            cmd.SetComputeTextureParam(sobelShader, kernelHandle, "Input", source);
            cmd.SetComputeTextureParam(sobelShader, kernelHandle, "Output", temporaryColorTexture.Identifier());

            // Dispatch the compute shader
            int groupX = Mathf.CeilToInt(descriptor.width / 16.0f);
            int groupY = Mathf.CeilToInt(descriptor.height / 16.0f);

            cmd.DispatchCompute(sobelShader, kernelHandle, groupX, groupY, 1);

            // Blit the output texture back to the camera target
            cmd.Blit(temporaryColorTexture.Identifier(), source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            // Cleanup temporary resources
            cmd.ReleaseTemporaryRT(temporaryColorTexture.id);
        }
    }

    SobelPass sobelPass;
    public ComputeShader sobelShader;

    public override void Create()
    {
        sobelPass = new SobelPass(sobelShader)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        sobelPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(sobelPass);
    }
}
