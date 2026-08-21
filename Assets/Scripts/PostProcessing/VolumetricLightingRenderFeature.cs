using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

/// <summary>
/// Box shaped volumes placed in the scene, resolved in screen space:
///
///   <see cref="ShadowVolume"/>          darkens whatever is rendered inside the box.
///   <see cref="VolumetricFogVolume"/>   raymarches in-scattered light inside the box.
///
/// Both live in one feature so the shade always resolves before the fog is added, without
/// depending on the order of the renderer's feature list. Neither pass is enqueued when no
/// matching volume is in view.
/// </summary>
public class VolumetricLightingRenderFeature : ScriptableRendererFeature
{
    // Must match MAX_FOG_VOLUMES / MAX_SHADOW_VOLUMES in the shaders.
    private const int kMaxVolumes = 8;

    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        [Tooltip("Raymarch material, uses Hidden/Custom/VolumetricRaymarch.")]
        public Material passMaterial = null;

        [Tooltip("Offsets the first raymarch sample to trade banding for noise.")]
        [Range(0f, 1f)]
        public float jitterIntensity = 1f;

        [Tooltip("Shadow volume material, uses Hidden/Custom/ShadowVolume.")]
        public Material shadowVolumeMaterial = null;
    }

    public Settings settings = new Settings();

    private VolumetricLightingPass m_ScriptablePass;
    private ShadowVolumePass m_ShadowVolumePass;

    public override void Create()
    {
        m_ScriptablePass = new VolumetricLightingPass(settings);
        m_ShadowVolumePass = new ShadowVolumePass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Enqueued first so surfaces are shaded before the fog adds light on top of them.
        if (settings.shadowVolumeMaterial != null && ShadowVolume.ActiveVolumes.Count > 0)
        {
            m_ShadowVolumePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
            renderer.EnqueuePass(m_ShadowVolumePass);
        }

        if (settings.passMaterial != null && VolumetricFogVolume.ActiveVolumes.Count > 0)
        {
            m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }

    private class PassData
    {
        public Material material;
        public TextureHandle src;
        public TextureHandle dst;
    }

    /// <summary>Blits <paramref name="material"/> over the camera colour and copies the result back.</summary>
    private static void BlitThroughMaterial(RenderGraph renderGraph, ContextContainer frameData, Material material,
        string passName, bool needsGlobalTextures)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        TextureHandle activeColor = resourceData.activeColorTexture;
        RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;

        TextureHandle tempTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_" + passName + "Temp", false);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
        {
            passData.material = material;
            passData.src = activeColor;
            passData.dst = tempTexture;

            builder.UseTexture(passData.src, AccessFlags.Read);
            builder.SetRenderAttachment(passData.dst, 0, AccessFlags.Write);

            // The raymarch samples _MainLightShadowmapTexture and the additional light
            // shadow atlas. Without this they are not guaranteed to be bound here, and
            // every step reads as fully lit, which kills the light shafts.
            if (needsGlobalTextures)
                builder.UseAllGlobalTextures(true);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName + "CopyBack", out var passData))
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

    /// <summary>
    /// Frustum culls the placed volumes, keeps the nearest <see cref="kMaxVolumes"/> of them
    /// and reports how many survived. Sorting only kicks in past the limit.
    /// </summary>
    private static int CollectVisible<T>(IReadOnlyList<T> volumes, Camera camera, Plane[] frustumPlanes,
        List<T> visible, System.Func<T, Bounds> boundsOf, System.Func<T, bool> isValid,
        System.Func<T, Vector3, float> distanceFade) where T : Object
    {
        visible.Clear();
        if (volumes.Count == 0)
            return 0;

        Vector3 cameraPos = camera.transform.position;
        GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);

        for (int i = 0; i < volumes.Count; i++)
        {
            T volume = volumes[i];
            if (volume == null || !isValid(volume))
                continue;

            if (!GeometryUtility.TestPlanesAABB(frustumPlanes, boundsOf(volume)))
                continue;

            if (distanceFade(volume, cameraPos) <= 0f)
                continue;

            visible.Add(volume);
        }

        if (visible.Count > kMaxVolumes)
        {
            visible.Sort((a, b) =>
                boundsOf(a).SqrDistance(cameraPos).CompareTo(boundsOf(b).SqrDistance(cameraPos)));
            visible.RemoveRange(kMaxVolumes, visible.Count - kMaxVolumes);
        }

        return visible.Count;
    }

    class ShadowVolumePass : ScriptableRenderPass
    {
        private static readonly int s_CountId = Shader.PropertyToID("_ShadowVolumeCount");
        private static readonly int s_WorldToLocalId = Shader.PropertyToID("_ShadowVolumeWorldToLocal");
        private static readonly int s_TintId = Shader.PropertyToID("_ShadowVolumeTint");

        private readonly Settings m_Settings;
        private readonly Matrix4x4[] m_WorldToLocal = new Matrix4x4[kMaxVolumes];
        private readonly Vector4[] m_Tints = new Vector4[kMaxVolumes];
        private readonly Plane[] m_FrustumPlanes = new Plane[6];
        private readonly List<ShadowVolume> m_Visible = new List<ShadowVolume>(kMaxVolumes);

        public ShadowVolumePass(Settings settings)
        {
            m_Settings = settings;
            renderPassEvent = settings.renderPassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            Material material = m_Settings.shadowVolumeMaterial;
            if (material == null)
                return;

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            int count = CollectVisible(ShadowVolume.ActiveVolumes, cameraData.camera, m_FrustumPlanes, m_Visible,
                v => v.WorldBounds, v => v.IsValid(), (v, pos) => v.DistanceFade(pos));
            if (count == 0)
                return;

            Vector3 cameraPos = cameraData.camera.transform.position;
            for (int i = 0; i < count; i++)
            {
                ShadowVolume volume = m_Visible[i];
                Color multiplier = volume.ResolveMultiplier(cameraPos);

                m_WorldToLocal[i] = volume.LocalToWorld.inverse;
                m_Tints[i] = new Vector4(multiplier.r, multiplier.g, multiplier.b, volume.edgeFade);
            }

            // Keep the unused slots neutral in case a stale array element is ever read.
            for (int i = count; i < kMaxVolumes; i++)
            {
                m_WorldToLocal[i] = Matrix4x4.identity;
                m_Tints[i] = new Vector4(1f, 1f, 1f, 1f);
            }

            material.SetInt(s_CountId, count);
            material.SetMatrixArray(s_WorldToLocalId, m_WorldToLocal);
            material.SetVectorArray(s_TintId, m_Tints);

            BlitThroughMaterial(renderGraph, frameData, material, "ShadowVolumePass", false);
        }
    }

    class VolumetricLightingPass : ScriptableRenderPass
    {
        private static readonly int s_JitterIntensityId = Shader.PropertyToID("_JitterIntensity");
        private static readonly int s_VolumeCountId = Shader.PropertyToID("_VolumeCount");
        private static readonly int s_VolumeWorldToLocalId = Shader.PropertyToID("_VolumeWorldToLocal");
        private static readonly int s_VolumeColorId = Shader.PropertyToID("_VolumeColor");
        private static readonly int s_VolumeParamsId = Shader.PropertyToID("_VolumeParams");
        private static readonly int s_VolumeParams2Id = Shader.PropertyToID("_VolumeParams2");

        private readonly Settings m_Settings;

        private readonly Matrix4x4[] m_WorldToLocal = new Matrix4x4[kMaxVolumes];
        private readonly Vector4[] m_Colors = new Vector4[kMaxVolumes];
        private readonly Vector4[] m_Params = new Vector4[kMaxVolumes];
        private readonly Vector4[] m_Params2 = new Vector4[kMaxVolumes];
        private readonly Plane[] m_FrustumPlanes = new Plane[6];
        private readonly List<VolumetricFogVolume> m_Visible = new List<VolumetricFogVolume>(kMaxVolumes);

        public VolumetricLightingPass(Settings settings)
        {
            m_Settings = settings;
            renderPassEvent = settings.renderPassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            Material material = m_Settings.passMaterial;
            if (material == null)
                return;

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            int count = CollectVisible(VolumetricFogVolume.ActiveVolumes, cameraData.camera, m_FrustumPlanes, m_Visible,
                v => v.WorldBounds, v => v.IsValid(), (v, pos) => v.DistanceFade(pos));
            if (count == 0)
                return;

            Vector3 cameraPos = cameraData.camera.transform.position;
            for (int i = 0; i < count; i++)
            {
                VolumetricFogVolume volume = m_Visible[i];

                m_WorldToLocal[i] = volume.LocalToWorld.inverse;
                m_Colors[i] = volume.color;
                m_Params[i] = new Vector4(
                    volume.density * volume.DistanceFade(cameraPos),
                    volume.anisotropy,
                    Mathf.Clamp(volume.stepCount, 2, 128),
                    volume.edgeFade);
                m_Params2[i] = new Vector4(
                    volume.mainLightShadows ? 1f : 0f,
                    volume.additionalLights ? 1f : 0f,
                    0f,
                    0f);
            }

            // Keep the unused slots harmless in case a stale array element is ever read.
            for (int i = count; i < kMaxVolumes; i++)
            {
                m_WorldToLocal[i] = Matrix4x4.identity;
                m_Colors[i] = Vector4.zero;
                m_Params[i] = new Vector4(0f, 0f, 2f, 1f);
                m_Params2[i] = Vector4.zero;
            }

            material.SetFloat(s_JitterIntensityId, m_Settings.jitterIntensity);
            material.SetInt(s_VolumeCountId, count);
            material.SetMatrixArray(s_VolumeWorldToLocalId, m_WorldToLocal);
            material.SetVectorArray(s_VolumeColorId, m_Colors);
            material.SetVectorArray(s_VolumeParamsId, m_Params);
            material.SetVectorArray(s_VolumeParams2Id, m_Params2);

            BlitThroughMaterial(renderGraph, frameData, material, "VolumetricLightingVolumePass", true);
        }
    }
}
