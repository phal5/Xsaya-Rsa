Shader "Custom/Cel_rev2_ViewVortex_Tessellation"
{
    Properties
    {
        [Header(Tessellation Settings)]
        _Tessellation ("Tessellation Factor", Range(1, 32)) = 2.0
        _TessMinDist ("Min Distance", Float) = 2.0
        _TessMaxDist ("Max Distance", Float) = 25.0

        [Header(View Axis Vortex Deformation Settings)]
        _T ("t (Deform Parameter)", Range(-10, 10)) = 0.0
        _N ("n (Start Threshold Depth)", Float) = 0.0
        _SmoothWidth ("Smooth Transition Width", Float) = 2.0
        _AxisOffsetX ("View Axis Offset X (Screen Right)", Float) = 0.0
        _AxisOffsetY ("View Axis Offset Y (Screen Up)", Float) = 0.0
        _UseSquaredLimit ("Use (v)^2+1 Limit (1=On, 0=Max(abs,1))", Float) = 1.0

        [Header(Flat Color and Light Tint Settings)]
        _Color ("Base Color", Color) = (0.9, 0.9, 0.9, 1.0)
        _Albedo ("Base Map", 2D) = "white" {}
        _LightInfluence ("Light Tint Influence", Range(0, 1)) = 0.5
        _ShadowInfluence ("Shadow Influence", Range(0, 1)) = 0.0
        _AmbientInfluence ("Ambient Tint Influence", Range(0, 1)) = 0.5

        [Header(Manifold Garden Outline Settings)]
        [Toggle] _ConstantPixelMode ("Constant Screen Pixel Mode", Float) = 1.0
        _PixelThickness ("Screen Pixel Thickness", Range(0.5, 10.0)) = 1.5
        _OutlineWidth ("World Outline Width", Range(0, 0.2)) = 0.01
        _OutlineDistScale ("World Outline Distance Scale", Float) = 0.1
        _OutlineColor ("Outline Color", Color) = (0.0, 0.0, 0.0, 1.0)
        _OutlineLightInfluence ("Outline Light Tint Influence", Range(0, 1)) = 0.0

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline" 
            "UniversalMaterialType"="Lit" 
            "IgnoreProjector"="True" 
        }
        LOD 300

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Assets/Art/Shaders/VortexDeform.hlsl"
        #include "Assets/Art/Shaders/CelTessellation.hlsl"

        UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
            UNITY_DEFINE_INSTANCED_PROP(float, _Tessellation)
            UNITY_DEFINE_INSTANCED_PROP(float, _TessMinDist)
            UNITY_DEFINE_INSTANCED_PROP(float, _TessMaxDist)

            UNITY_DEFINE_INSTANCED_PROP(float, _T)
            UNITY_DEFINE_INSTANCED_PROP(float, _N)
            UNITY_DEFINE_INSTANCED_PROP(float, _SmoothWidth)
            UNITY_DEFINE_INSTANCED_PROP(float, _AxisOffsetX)
            UNITY_DEFINE_INSTANCED_PROP(float, _AxisOffsetY)
            UNITY_DEFINE_INSTANCED_PROP(float, _UseSquaredLimit)

            UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
            UNITY_DEFINE_INSTANCED_PROP(float, _LightInfluence)
            UNITY_DEFINE_INSTANCED_PROP(float, _ShadowInfluence)
            UNITY_DEFINE_INSTANCED_PROP(float, _AmbientInfluence)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Albedo_ST)

            UNITY_DEFINE_INSTANCED_PROP(float, _ConstantPixelMode)
            UNITY_DEFINE_INSTANCED_PROP(float, _PixelThickness)
            UNITY_DEFINE_INSTANCED_PROP(float, _OutlineWidth)
            UNITY_DEFINE_INSTANCED_PROP(float, _OutlineDistScale)
            UNITY_DEFINE_INSTANCED_PROP(half4, _OutlineColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _OutlineLightInfluence)
        UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

        TEXTURE2D(_Albedo); SAMPLER(sampler_Albedo);

        struct Attributes
        {
            float4 positionOS   : POSITION;
            float3 normalOS     : NORMAL;
            float2 uv           : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct TessControlPoint
        {
            float4 positionOS   : INTERNALTESSPOS;
            float3 normalOS     : NORMAL;
            float2 uv           : TEXCOORD0;
            float3 positionWS   : TEXCOORD1;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS   : SV_POSITION;
            float3 positionWS   : TEXCOORD0;
            float3 normalWS     : TEXCOORD1;
            float2 uv           : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        // 1. Vertex Shader (Pass through to Hull)
        TessControlPoint TessVert(Attributes input)
        {
            TessControlPoint output;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);

            output.positionOS = input.positionOS;
            output.normalOS = input.normalOS;
            output.uv = input.uv;
            output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
            return output;
        }

        // 2. Hull Shader Patch Constant
        TessellationFactors PatchConstantFunc(InputPatch<TessControlPoint, 3> patch)
        {
            UNITY_SETUP_INSTANCE_ID(patch[0]);
            float tessFactor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Tessellation);
            float minD = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _TessMinDist);
            float maxD = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _TessMaxDist);

            return CalcDistanceTessFactors(
                patch[0].positionWS,
                patch[1].positionWS,
                patch[2].positionWS,
                minD, maxD, tessFactor
            );
        }

        // 3. Hull Shader
        [domain("tri")]
        [partitioning("fractional_odd")]
        [outputtopology("triangle_cw")]
        [patchconstantfunc("PatchConstantFunc")]
        [outputcontrolpoints(3)]
        TessControlPoint Hull(InputPatch<TessControlPoint, 3> patch, uint id : SV_OutputControlPointID)
        {
            return patch[id];
        }

        // 4. Domain Interpolation Helper
        #define INTERPOLATE(fieldName) (barycentric.x * patch[0].fieldName + barycentric.y * patch[1].fieldName + barycentric.z * patch[2].fieldName)

        ENDHLSL

        // ------------------------------------------------------------------------
        // Pass 1: UniversalForward (Flat Base Color with Light Tint)
        // ------------------------------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 4.6
            #pragma require tessellation
            #pragma multi_compile_instancing

            #pragma vertex TessVert
            #pragma hull Hull
            #pragma domain DomainForward
            #pragma fragment FragForward

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _FORWARD_PLUS

            [domain("tri")]
            Varyings DomainForward(TessellationFactors factors, OutputPatch<TessControlPoint, 3> patch, float3 barycentric : SV_DomainLocation)
            {
                UNITY_SETUP_INSTANCE_ID(patch[0]);
                Varyings output;
                UNITY_TRANSFER_INSTANCE_ID(patch[0], output);

                float4 posOS = INTERPOLATE(positionOS);
                float3 normOS = normalize(INTERPOLATE(normalOS));
                float2 uv = INTERPOLATE(uv);

                float tVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _T);
                float nVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _N);
                float swVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _SmoothWidth);
                float axOff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _AxisOffsetX);
                float ayOff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _AxisOffsetY);
                float sqVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _UseSquaredLimit);

                float3 deformedPosOS;
                float theta;
                float3 axisDir;
                ViewVortexDeform_float(posOS.xyz, tVal, nVal, swVal, axOff, ayOff, sqVal, deformedPosOS, theta, axisDir);

                output.positionWS = TransformObjectToWorld(deformedPosOS);
                output.positionCS = TransformWorldToHClip(output.positionWS);

                float3 unrotNormalWS = normalize(TransformObjectToWorldNormal(normOS));
                output.normalWS = normalize(RotateVectorAxis(unrotNormalWS, axisDir, theta));
                output.uv = uv;

                return output;
            }

            half4 FragForward(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float2 uv = input.uv;
                float4 albedoST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Albedo_ST);
                half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Color);
                float lightInf = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _LightInfluence);
                float shadowInf = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _ShadowInfluence);
                float ambientInf = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _AmbientInfluence);

                half4 baseTex = SAMPLE_TEXTURE2D(_Albedo, sampler_Albedo, uv * albedoST.xy + albedoST.zw);
                half3 baseSurfaceColor = baseTex.rgb * baseColor.rgb;

                half3 ambientTint = SampleSH(float3(0, 1, 0)) * ambientInf;

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord, input.positionWS, half4(1, 1, 1, 1));
                float shadowAtten = lerp(1.0, mainLight.shadowAttenuation, shadowInf);
                half3 lightTint = mainLight.color * shadowAtten;

                #if defined(_ADDITIONAL_LIGHTS) || defined(_FORWARD_PLUS)
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light addLight = GetAdditionalLight(lightIndex, input.positionWS, half4(1, 1, 1, 1));
                    float addShadowAtten = lerp(1.0, addLight.shadowAttenuation, shadowInf);
                    lightTint += addLight.color * (addLight.distanceAttenuation * addShadowAtten);
                }
                #endif

                half3 totalTint = lerp(half3(1.0, 1.0, 1.0), (lightTint + ambientTint), lightInf);
                half3 finalColor = baseSurfaceColor * totalTint;

                return half4(finalColor, baseTex.a * baseColor.a);
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------------
        // Pass 2: Outline (Constant Screen Pixel Thickness / Distance Proportional)
        // ------------------------------------------------------------------------
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 4.6
            #pragma require tessellation
            #pragma multi_compile_instancing

            #pragma vertex TessVert
            #pragma hull Hull
            #pragma domain DomainOutline
            #pragma fragment FragOutline

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _FORWARD_PLUS

            struct VaryingsOutline
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            [domain("tri")]
            VaryingsOutline DomainOutline(TessellationFactors factors, OutputPatch<TessControlPoint, 3> patch, float3 barycentric : SV_DomainLocation)
            {
                UNITY_SETUP_INSTANCE_ID(patch[0]);
                VaryingsOutline output;
                UNITY_TRANSFER_INSTANCE_ID(patch[0], output);

                float4 posOS = INTERPOLATE(positionOS);
                float3 normOS = normalize(INTERPOLATE(normalOS));

                float tVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _T);
                float nVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _N);
                float swVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _SmoothWidth);
                float axOff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _AxisOffsetX);
                float ayOff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _AxisOffsetY);
                float sqVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _UseSquaredLimit);

                float constPixelMode = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _ConstantPixelMode);
                float pixelThick = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _PixelThickness);
                float outWidth = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _OutlineWidth);
                float distScale = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _OutlineDistScale);

                float3 deformedPosOS;
                float theta;
                float3 axisDir;
                ViewVortexDeform_float(posOS.xyz, tVal, nVal, swVal, axOff, ayOff, sqVal, deformedPosOS, theta, axisDir);

                float3 positionWS = TransformObjectToWorld(deformedPosOS);
                float3 unrotNormalWS = normalize(TransformObjectToWorldNormal(normOS));
                float3 normalWS = normalize(RotateVectorAxis(unrotNormalWS, axisDir, theta));

                output.positionWS = positionWS;

                if (constPixelMode > 0.5)
                {
                    // Manifold Garden Style: Constant screen-space pixel width
                    float4 posCS = TransformWorldToHClip(positionWS);
                    float3 normCS = TransformWorldToHClipDir(normalWS);
                    float2 aspect = float2(_ScreenParams.y / max(_ScreenParams.x, 1.0), 1.0);
                    float2 offsetDir = normalize(normCS.xy * aspect) / aspect;
                    
                    float pixelScale = (pixelThick * 2.0) / max(_ScreenParams.y, 1.0);
                    posCS.xy += offsetDir * (posCS.w * pixelScale);
                    output.positionCS = posCS;
                }
                else
                {
                    // World Space Distance-Proportional Mode
                    float distToCam = distance(positionWS, GetCameraPositionWS());
                    float effectiveWidth = outWidth * (1.0 + distToCam * distScale);
                    float3 extrudedWS = positionWS + normalWS * effectiveWidth;
                    output.positionCS = TransformWorldToHClip(extrudedWS);
                }

                return output;
            }

            half4 FragOutline(VaryingsOutline input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float constPixelMode = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _ConstantPixelMode);
                float pixelThick = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _PixelThickness);
                float outWidth = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _OutlineWidth);
                half4 outColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _OutlineColor);
                float outLightInf = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _OutlineLightInfluence);
                float lightInf = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _LightInfluence);
                float shadowInf = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _ShadowInfluence);
                float ambientInf = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _AmbientInfluence);

                if (constPixelMode > 0.5 && pixelThick <= 0.01)
                {
                    discard;
                }
                if (constPixelMode <= 0.5 && outWidth <= 0.0001)
                {
                    discard;
                }

                half3 baseOutlineColor = outColor.rgb;

                // Collect ambient and light tint for outline
                half3 ambientTint = SampleSH(float3(0, 1, 0)) * ambientInf;

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord, input.positionWS, half4(1, 1, 1, 1));
                float shadowAtten = lerp(1.0, mainLight.shadowAttenuation, shadowInf);
                half3 lightTint = mainLight.color * shadowAtten;

                #if defined(_ADDITIONAL_LIGHTS) || defined(_FORWARD_PLUS)
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light addLight = GetAdditionalLight(lightIndex, input.positionWS, half4(1, 1, 1, 1));
                    float addShadowAtten = lerp(1.0, addLight.shadowAttenuation, shadowInf);
                    lightTint += addLight.color * (addLight.distanceAttenuation * addShadowAtten);
                }
                #endif

                half3 totalLightTint = lerp(half3(1.0, 1.0, 1.0), (lightTint + ambientTint), lightInf);
                half3 finalOutlineTint = lerp(half3(1.0, 1.0, 1.0), totalLightTint, outLightInf);
                half3 finalOutlineColor = baseOutlineColor * finalOutlineTint;

                return half4(finalOutlineColor, outColor.a);
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------------
        // Pass 3: ShadowCaster
        // ------------------------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 4.6
            #pragma require tessellation
            #pragma multi_compile_instancing

            #pragma vertex TessVert
            #pragma hull Hull
            #pragma domain DomainShadow
            #pragma fragment FragShadow

            float3 _LightDirection;
            float3 _LightPosition;

            struct VaryingsShadow
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            [domain("tri")]
            VaryingsShadow DomainShadow(TessellationFactors factors, OutputPatch<TessControlPoint, 3> patch, float3 barycentric : SV_DomainLocation)
            {
                UNITY_SETUP_INSTANCE_ID(patch[0]);
                VaryingsShadow output;
                UNITY_TRANSFER_INSTANCE_ID(patch[0], output);

                float4 posOS = INTERPOLATE(positionOS);
                float3 normOS = normalize(INTERPOLATE(normalOS));

                float tVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _T);
                float nVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _N);
                float swVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _SmoothWidth);
                float axOff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _AxisOffsetX);
                float ayOff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _AxisOffsetY);
                float sqVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _UseSquaredLimit);

                float3 deformedPosOS;
                float theta;
                float3 axisDir;
                ViewVortexDeform_float(posOS.xyz, tVal, nVal, swVal, axOff, ayOff, sqVal, deformedPosOS, theta, axisDir);

                float3 positionWS = TransformObjectToWorld(deformedPosOS);
                float3 unrotNormalWS = normalize(TransformObjectToWorldNormal(normOS));
                float3 normalWS = normalize(RotateVectorAxis(unrotNormalWS, axisDir, theta));

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                output.positionCS = positionCS;
                return output;
            }

            half4 FragShadow(VaryingsShadow input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------------
        // Pass 4: DepthOnly
        // ------------------------------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 4.6
            #pragma require tessellation
            #pragma multi_compile_instancing

            #pragma vertex TessVert
            #pragma hull Hull
            #pragma domain DomainDepth
            #pragma fragment FragDepth

            struct VaryingsDepth
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            [domain("tri")]
            VaryingsDepth DomainDepth(TessellationFactors factors, OutputPatch<TessControlPoint, 3> patch, float3 barycentric : SV_DomainLocation)
            {
                UNITY_SETUP_INSTANCE_ID(patch[0]);
                VaryingsDepth output;
                UNITY_TRANSFER_INSTANCE_ID(patch[0], output);

                float4 posOS = INTERPOLATE(positionOS);

                float tVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _T);
                float nVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _N);
                float swVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _SmoothWidth);
                float axOff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _AxisOffsetX);
                float ayOff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _AxisOffsetY);
                float sqVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _UseSquaredLimit);

                float3 deformedPosOS;
                ViewVortexDeform_float(posOS.xyz, tVal, nVal, swVal, axOff, ayOff, sqVal, deformedPosOS);

                float3 positionWS = TransformObjectToWorld(deformedPosOS);
                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 FragDepth(VaryingsDepth input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------------
        // Pass 5: DepthNormals
        // ------------------------------------------------------------------------
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 4.6
            #pragma require tessellation
            #pragma multi_compile_instancing

            #pragma vertex TessVert
            #pragma hull Hull
            #pragma domain DomainDepthNormals
            #pragma fragment FragDepthNormals

            struct VaryingsDepthNormals
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            [domain("tri")]
            VaryingsDepthNormals DomainDepthNormals(TessellationFactors factors, OutputPatch<TessControlPoint, 3> patch, float3 barycentric : SV_DomainLocation)
            {
                UNITY_SETUP_INSTANCE_ID(patch[0]);
                VaryingsDepthNormals output;
                UNITY_TRANSFER_INSTANCE_ID(patch[0], output);

                float4 posOS = INTERPOLATE(positionOS);
                float3 normOS = normalize(INTERPOLATE(normalOS));

                float tVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _T);
                float nVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _N);
                float swVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _SmoothWidth);
                float axOff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _AxisOffsetX);
                float ayOff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _AxisOffsetY);
                float sqVal = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _UseSquaredLimit);

                float3 deformedPosOS;
                float theta;
                float3 axisDir;
                ViewVortexDeform_float(posOS.xyz, tVal, nVal, swVal, axOff, ayOff, sqVal, deformedPosOS, theta, axisDir);

                float3 positionWS = TransformObjectToWorld(deformedPosOS);
                float3 unrotNormalWS = normalize(TransformObjectToWorldNormal(normOS));
                float3 normalWS = normalize(RotateVectorAxis(unrotNormalWS, axisDir, theta));

                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = normalWS;
                return output;
            }

            half4 FragDepthNormals(VaryingsDepthNormals input) : SV_Target
            {
                return half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
