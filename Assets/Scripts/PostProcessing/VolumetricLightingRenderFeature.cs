using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class VolumetricLightingRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material passMaterial = null;
    }

    public Settings settings = new Settings();
    private VolumetricLightingPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new VolumetricLightingPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.passMaterial == null)
            return;

        var stack = VolumeManager.instance.stack;
        if (stack == null)
            return;

        var volume = stack.GetComponent<VolumetricLightingVolumeComponent>();
        if (volume == null || !volume.IsActive())
            return;

        renderer.EnqueuePass(m_ScriptablePass);
    }

    class VolumetricLightingPass : ScriptableRenderPass
    {
        private Settings m_Settings;
        private RTHandle m_CopiedColor;

        private class PassData
        {
            public Material material;
            public TextureHandle src;
            public TextureHandle dst;
        }

        public VolumetricLightingPass(Settings settings)
        {
            m_Settings = settings;
            renderPassEvent = settings.renderPassEvent;
        }

        [System.Obsolete]
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var stack = VolumeManager.instance.stack;
            if (stack == null) return;

            var volume = stack.GetComponent<VolumetricLightingVolumeComponent>();
            if (volume == null || !volume.IsActive()) return;

            Material material = m_Settings.passMaterial;
            if (material == null) return;

            material.SetFloat("_StepCount", volume.stepCount.value);
            material.SetFloat("_Density", volume.density.value);
            material.SetColor("_FogColor", volume.fogColor.value);
            material.SetFloat("_Anisotropy", volume.anisotropy.value);
            material.SetFloat("_JitterIntensity", volume.jitterIntensity.value);

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            TextureHandle activeColor = resourceData.activeColorTexture;
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            TextureHandle tempTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_VolumetricTempTexture", false);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("VolumetricLightingVolumePass", out var passData))
            {
                passData.material = material;
                passData.src = activeColor;
                passData.dst = tempTexture;

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.SetRenderAttachment(passData.dst, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("VolumetricLightingVolumeCopyBackPass", out var passData))
            {
                passData.src = tempTexture;
                passData.dst = activeColor;

                builder.UseTexture(passData.src, AccessFlags.Read);
                builder.SetRenderAttachment(passData.dst, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), 0, false);
                });
            }
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var stack = VolumeManager.instance.stack;
            if (stack == null) return;

            var volume = stack.GetComponent<VolumetricLightingVolumeComponent>();
            if (volume == null || !volume.IsActive()) return;

            Material material = m_Settings.passMaterial;
            if (material == null) return;

            material.SetFloat("_StepCount", volume.stepCount.value);
            material.SetFloat("_Density", volume.density.value);
            material.SetColor("_FogColor", volume.fogColor.value);
            material.SetFloat("_Anisotropy", volume.anisotropy.value);
            material.SetFloat("_JitterIntensity", volume.jitterIntensity.value);

            CommandBuffer cmd = CommandBufferPool.Get("VolumetricLightingVolumePass");
            RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            RenderingUtils.ReAllocateIfNeeded(ref m_CopiedColor, renderingData.cameraData.cameraTargetDescriptor, name: "_VolumetricTempTexture");

            Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_CopiedColor, material, 0);
            Blitter.BlitCameraTexture(cmd, m_CopiedColor, cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            m_CopiedColor?.Release();
        }
    }

    protected override void Dispose(bool disposing)
    {
        m_ScriptablePass?.Dispose();
    }
}
