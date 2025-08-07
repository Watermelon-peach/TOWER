using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrayscaleFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class GrayscaleSettings
    {
        public Material grayscaleMaterial;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public GrayscaleSettings settings = new GrayscaleSettings();

    class GrayscalePass : ScriptableRenderPass
    {
        private Material grayscaleMaterial;
        private RTHandle tempTexture;
        private RTHandle source;

        public GrayscalePass(Material material)
        {
            grayscaleMaterial = material;
        }

        public void Setup(RTHandle sourceHandle)
        {
            this.source = sourceHandle;
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (grayscaleMaterial == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("Grayscale Pass");

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            // 임시 렌더 텍스처 할당
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, desc, name: "_TemporaryColorTexture");

            // Blit 처리
            Blit(cmd, source, tempTexture, grayscaleMaterial, 0);
            Blit(cmd, tempTexture, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (tempTexture != null)
                tempTexture.Release();
        }
    }

    GrayscalePass grayscalePass;

    public override void Create()
    {
        grayscalePass = new GrayscalePass(settings.grayscaleMaterial)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
        grayscalePass.Setup(cameraColorTargetHandle);
        renderer.EnqueuePass(grayscalePass);
    }
}
