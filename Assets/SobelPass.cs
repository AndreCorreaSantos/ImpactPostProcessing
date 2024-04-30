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

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) 
    {
        this.source = renderingData.cameraData.renderer.cameraColorTargetHandle; // getting source image from camera
        if (temporaryColorTexture == null)
        {
            temporaryColorTexture = RTHandles.Alloc( // creating texture where the resulting sobel filter will be written to
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

        // getting settings defined by user in the post processing menu
        VolumeStack volumes = VolumeManager.instance.stack;
        CustomSobel customSobel = volumes.GetComponent<CustomSobel>();
        Debug.Log(customSobel.threshold.value);
        if (customSobel.isEnabled.value)
        {
            // Set the compute shader parameters
            cmd.SetComputeTextureParam(sobelShader, kernelHandle, "Input", source);
            cmd.SetComputeTextureParam(sobelShader, kernelHandle, "Output", temporaryColorTexture);
            cmd.SetComputeFloatParam(sobelShader,"Time",Time.time);
            cmd.SetComputeFloatParam(sobelShader,"Threshold",customSobel.threshold.value);
            // Dispatch the compute shader
            int groupX = Mathf.CeilToInt(descriptor.width / 16.0f);
            int groupY = Mathf.CeilToInt(descriptor.height / 16.0f);
            cmd.DispatchCompute(sobelShader, kernelHandle, groupX, groupY, 1);

            // Blit the result from temporaryColorTexture to the source
            cmd.Blit(temporaryColorTexture, source);

            // Execute the command buffer
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        if (temporaryColorTexture != null) // releasing the temporary texture where the sobel filter was stored
        {
            RTHandles.Release(temporaryColorTexture);
            temporaryColorTexture = null;
        }
    }
}