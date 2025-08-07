using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrayscalePass : ScriptableRenderPass
{
    private Material grayscaleMaterial;
    private RTHandle tempTexture;
    private RTHandle source;

    public GrayscalePass(Material material)
    {
        this.grayscaleMaterial = material;
    }

    public void Setup(RTHandle source)
    {
        this.source = source;
    }

    [Obsolete]
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cameraData = renderingData.cameraData;
        CommandBuffer cmd = CommandBufferPool.Get("Grayscale Pass");

        var descriptor = cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;

        // RTHandle 초기화
        RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_TemporaryColorTexture");

        // Blit: 원본 → 임시 버퍼 (후처리 적용)
        Blit(cmd, source, tempTexture, grayscaleMaterial, 0);
        // Blit: 다시 원본 버퍼로 복사
        Blit(cmd, tempTexture, source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        tempTexture?.Release();
    }
}
