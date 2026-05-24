// Made with Amplify Shader Editor v1.9.9.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ANGRYMESH/Stylized Pack/Tree Leaf"
{
	Properties
	{
		[Header(Base)][Toggle( _ENABLEFLIPNORMALS_ON )] _EnableFlipNormals( "Enable Flip Normals", Float ) = 0
		[Toggle( _ENABLEGLANCINGANGLECUT_ON )] _EnableGlancingAngleCut( "Enable Glancing Angle Cut [ForwardOnly]", Float ) = 0
		_BaseGlancingAngleCut( "Base Glancing Angle Cut", Range( 0, 1 ) ) = 0.8
		_CutOff( "Base Opacity Cutoff", Range( 0, 1 ) ) = 0.3
		[HDR] _BaseAlbedoColor( "Base Albedo Color", Color ) = ( 0.5019608, 0.5019608, 0.5019608 )
		_BaseAlbedoBrightness( "Base Albedo Brightness", Range( 0, 5 ) ) = 1
		_BaseAlbedoDesaturation( "Base Albedo Desaturation", Range( 0, 1 ) ) = 0
		_BaseNormalIntensity( "Base Normal Intensity", Range( 0, 5 ) ) = 1
		_BaseSmoothnessMin( "Base Smoothness Min", Range( 0, 5 ) ) = 0
		_BaseSmoothnessMax( "Base Smoothness Max", Range( 0, 5 ) ) = 1
		_BaseTreeAOIntensity( "Base Tree AO Intensity", Range( 0, 1 ) ) = 0.5
		_BaseLerpBetweenColorandTexture( "Base Lerp Between Color and Texture", Range( 0, 1 ) ) = 1
		[NoScaleOffset] _BaseAlbedo( "Base Albedo", 2D ) = "gray" {}
		[NoScaleOffset] _BaseNormal( "Base Normal", 2D ) = "bump" {}
		[NoScaleOffset] _BaseSSE( "Base SSE", 2D ) = "white" {}
		[HDR][Header(Base SSS)] _BaseSSSColor( "Base SSS Color", Color ) = ( 1, 1, 1 )
		_BaseSSSIntensity( "Base SSS Intensity", Range( 0, 50 ) ) = 1
		_BaseSSSAOInfluence( "Base SSS AO Influence", Range( 0, 1 ) ) = 0.8
		_BaseSSSNormalDistortion( "Base SSS Normal Distortion", Range( 0, 1 ) ) = 0.5
		_BaseSSSScattering( "Base SSS Scattering", Range( 1, 50 ) ) = 2
		_BaseSSSDirect( "Base SSS Direct", Range( 0, 1 ) ) = 0.9
		_BaseSSSAmbiet( "Base SSS Ambiet", Range( 0, 1 ) ) = 0.1
		_BaseSSSShadow( "Base SSS Shadow", Range( 0, 1 ) ) = 0.9
		[HDR][Header(Base Emissive)] _BaseEmissiveColor( "Base Emissive Color", Color ) = ( 0, 0, 0, 0 )
		_BaseEmissiveIntensity( "Base Emissive Intensity", Range( 0, 50 ) ) = 1
		_BaseEmissiveMaskContrast( "Base Emissive Mask Contrast", Range( 0, 50 ) ) = 1
		_BaseEmissiveAOMask( "Base Emissive AO Mask", Range( 0, 1 ) ) = 0
		[Header(Base Gradient Color)][Toggle( _ENABLEGRADIENTCOLOR_ON )] _EnableGradientColor( "Enable Gradient Color", Float ) = 0
		[HDR] _GradientColor( "Gradient Color", Color ) = ( 0.5019608, 0.5019608, 0.5019608 )
		_GradientColorIntensity( "Gradient Color Intensity", Range( 0, 1 ) ) = 1
		_GradientColorOffset( "Gradient Color Offset", Range( 0, 5 ) ) = 1
		_GradientColorContrast( "Gradient Color Contrast", Range( 0, 30 ) ) = 1
		[IntRange] _GradientColorInvertMask( "Gradient Color Invert Mask", Range( 0, 1 ) ) = 0
		[Header(Base Second Color)][Toggle( _ENABLESECONDCOLOR_ON )] _EnableSecondColor( "Enable Second Color", Float ) = 0
		[HDR] _SecondColor( "Second Color", Color ) = ( 0.5019608, 0.5019608, 0.5019608 )
		_SecondColorIntensity( "Second Color Intensity", Range( 0, 1 ) ) = 1
		_SecondColorOffset( "Second Color Offset", Range( 0, 3 ) ) = 1
		_SecondColorContrast( "Second Color Contrast", Range( 0, 30 ) ) = 1
		[IntRange] _SecondColorInvertMask( "Second Color Invert Mask", Range( 0, 1 ) ) = 0
		[Header(Base Tint Color)][Toggle( _ENABLETINTCOLOR_ON )] _EnableTintColor( "Enable Tint Color", Float ) = 0
		[HDR] _TintColor1( "Tint Color 1", Color ) = ( 0.5019608, 0.5019608, 0.5019608, 0 )
		[HDR] _TintColor2( "Tint Color 2", Color ) = ( 0.5019608, 0.5019608, 0.5019608, 0 )
		_TintNoiseIntensity( "Tint Noise Intensity", Range( 0, 1 ) ) = 1
		_TintNoiseOffset( "Tint Noise Offset", Range( 0, 10 ) ) = 1
		_TintNoiseContrast( "Tint Noise Contrast", Range( 0, 10 ) ) = 1
		[Header(Top Layer)][Toggle( _ENABLETOPLAYERBLEND_ON )] _EnableTopLayerBlend( "Enable Top Layer Blend", Float ) = 0
		[HDR] _TopLayerColor( "Top Layer Color", Color ) = ( 0.7764706, 0.8392157, 0.9490196 )
		[HDR] _TopLayerSSSColor( "Top Layer SSS Color", Color ) = ( 0.7843137, 0.9215686, 1 )
		_TopLayerSSSIntensity( "Top Layer SSS Intensity", Range( 0, 1 ) ) = 0.5
		_TopLayerSmoothnessIntensity( "Top Layer Smoothness Intensity", Range( 0, 5 ) ) = 0
		_TopLayerIntensity( "Top Layer Intensity", Range( 0, 1 ) ) = 1
		_TopLayerOffset( "Top Layer Offset", Range( 0, 1 ) ) = 0.5
		_TopLayerContrast( "Top Layer Contrast", Range( 0, 30 ) ) = 10
		_TopLayerAOMask( "Top Layer AO Mask", Range( 0, 10 ) ) = 1
		[Toggle( _ENABLEWORLDPROJECTION_ON )] _EnableWorldProjection( "Enable World Projection", Float ) = 1
		[Toggle( _ENABLEBACKFACEPROJECTION_ON )] _EnableBackfaceProjection( "Enable Backface Projection", Float ) = 1
		[Header(Wind)][Toggle( _ENABLEWIND_ON )] _EnableWind( "Enable Wind", Float ) = 1
		_WindLeafAmplitude( "Wind Leaf Amplitude", Range( 0, 2 ) ) = 1
		_WindLeafSpeed( "Wind Leaf Speed", Range( 0, 2 ) ) = 1
		_WindLeafScale( "Wind Leaf Scale", Range( 0, 2 ) ) = 1
		_WindLeafTurbulence( "Wind Leaf Turbulence", Range( 0, 2 ) ) = 1
		_WindLeafOffset( "Wind Leaf Offset", Range( 0, 2 ) ) = 1
		[Header(Wind (Use the same values for each tree material))] _WindTreeFlexibility( "Wind Tree Flexibility", Range( 0, 2 ) ) = 1
		_WindTreeBaseRigidity( "Wind Tree Base Rigidity", Range( 0, 5 ) ) = 2.5
		[Toggle( _ENABLESTATICMESHSUPPORT_ON )] _EnableStaticMeshSupport( "Enable Static Mesh Support", Float ) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}


		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Shadow", Range( 0, 1 ) ) = 0.5

		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0

		//_InstancedTerrainNormals("Specular Highlights", Float) = 1.0
	}

	SubShader
	{
		

		Tags { "RenderType"="Opaque" "Queue"="Geometry" "DisableBatching"="False" }

	LOD 0

		Cull Off
		AlphaToMask Off
		ZWrite On
		ZTest LEqual
		ColorMask RGBA

		

		Blend Off
		

		CGINCLUDE
			#pragma target 3.5

			float4 FixedTess( float tessValue )
			{
				return tessValue;
			}

			float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
			{
				float3 wpos = mul(o2w,vertex).xyz;
				float dist = distance (wpos, cameraPos);
				float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
				return f;
			}

			float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
			{
				float4 tess;
				tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
				tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
				tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
				tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
				return tess;
			}

			float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
			{
				float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
				float len = distance(wpos0, wpos1);
				float f = max(len * scParams.y / (edgeLen * dist), 1.0);
				return f;
			}

			float DistanceFromPlane (float3 pos, float4 plane)
			{
				float d = dot (float4(pos,1.0f), plane);
				return d;
			}

			bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
			{
				float4 planeTest;
				planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
				planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
				planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
				planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
				return !all (planeTest);
			}

			float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
			{
				float3 f;
				f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
				f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
				f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

				return CalcTriEdgeTessFactors (f);
			}

			float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
			{
				float3 pos0 = mul(o2w,v0).xyz;
				float3 pos1 = mul(o2w,v1).xyz;
				float3 pos2 = mul(o2w,v2).xyz;
				float4 tess;
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
				return tess;
			}

			float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
			{
				float3 pos0 = mul(o2w,v0).xyz;
				float3 pos1 = mul(o2w,v1).xyz;
				float3 pos2 = mul(o2w,v2).xyz;
				float4 tess;

				if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
				{
					tess = 0.0f;
				}
				else
				{
					tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
					tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
					tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
					tess.w = (tess.x + tess.y + tess.z) / 3.0f;
				}
				return tess;
			}

			float4 ComputeClipSpacePosition( float2 screenPosNorm, float deviceDepth )
			{
				float4 positionCS = float4( screenPosNorm * 2.0 - 1.0, deviceDepth, 1.0 );
			#if UNITY_UV_STARTS_AT_TOP
				positionCS.y = -positionCS.y;
			#endif
				return positionCS;
			}
		ENDCG

		
		Pass
		{
			
			Name "ForwardBase"
			Tags { "LightMode"="ForwardBase" }

			Blend Off

			CGPROGRAM
				#define ASE_GEOMETRY 1
				#define ASE_FRAGMENT_NORMAL 0
				#define ASE_RECEIVE_SHADOWS
				#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
				#pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF
				#pragma multi_compile_instancing
				#pragma multi_compile_fog
				#define ASE_FOG
				#define ASE_TRANSLUCENCY 1
				#pragma multi_compile _ LOD_FADE_CROSSFADE
				#pragma multi_compile_local _ALPHATEST_ON
				#define ASE_VERSION 19903

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#ifndef UNITY_PASS_FORWARDBASE
					#define UNITY_PASS_FORWARDBASE
				#endif
				#include "HLSLSupport.cginc"
				#ifdef ASE_GEOMETRY
					#ifndef UNITY_INSTANCED_LOD_FADE
						#define UNITY_INSTANCED_LOD_FADE
					#endif
					#ifndef UNITY_INSTANCED_SH
						#define UNITY_INSTANCED_SH
					#endif
					#ifndef UNITY_INSTANCED_LIGHTMAPSTS
						#define UNITY_INSTANCED_LIGHTMAPSTS
					#endif
				#endif
				#include "UnityShaderVariables.cginc"
				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "UnityPBSLighting.cginc"
				#include "AutoLight.cginc"

				#if defined(UNITY_INSTANCING_ENABLED) && ( defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL) || defined(_INSTANCEDTERRAINNORMALS_PIXEL) )
					#define ENABLE_TERRAIN_PERPIXEL_NORMAL
				#endif

				#include "UnityStandardUtils.cginc"
				#define ASE_NEEDS_VERT_POSITION
				#define ASE_NEEDS_TEXTURE_COORDINATES2
				#define ASE_NEEDS_VERT_TEXTURE_COORDINATES2
				#define ASE_NEEDS_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_COLOR
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
				#define ASE_NEEDS_WORLD_TANGENT
				#define ASE_NEEDS_FRAG_WORLD_TANGENT
				#define ASE_NEEDS_WORLD_NORMAL
				#define ASE_NEEDS_FRAG_WORLD_NORMAL
				#define ASE_NEEDS_FRAG_WORLD_BITANGENT
				#define ASE_NEEDS_WORLD_POSITION
				#define ASE_NEEDS_FRAG_WORLD_POSITION
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLESTATICMESHSUPPORT_ON
				#pragma shader_feature_local _ENABLETOPLAYERBLEND_ON
				#pragma shader_feature_local _ENABLETINTCOLOR_ON
				#pragma shader_feature_local _ENABLESECONDCOLOR_ON
				#pragma shader_feature_local _ENABLEGRADIENTCOLOR_ON
				#pragma shader_feature_local _ENABLEBACKFACEPROJECTION_ON
				#pragma shader_feature_local _ENABLEWORLDPROJECTION_ON
				#pragma shader_feature_local _ENABLEFLIPNORMALS_ON
				#pragma shader_feature_local _ENABLEGLANCINGANGLECUT_ON


				struct appdata
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					half4 tangent : TANGENT;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					float4 ase_color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float4 positionWS : TEXCOORD0; // xyz = positionWS, w = fogCoord
					half3 normalWS : TEXCOORD1;
					float4 tangentWS : TEXCOORD2; // holds terrainUV ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
					half4 ambientOrLightmapUV : TEXCOORD3;
					UNITY_LIGHTING_COORDS( 4, 5 )
					float4 ase_texcoord6 : TEXCOORD6;
					float4 ase_color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				#ifdef ASE_TRANSMISSION
					float _TransmissionShadow;
				#endif
				#ifdef ASE_TRANSLUCENCY
					float _TransStrength;
					float _TransNormal;
					float _TransScattering;
					float _TransDirect;
					float _TransAmbient;
					float _TransShadow;
				#endif
				#ifdef ASE_TESSELLATION
					float _TessPhongStrength;
					float _TessValue;
					float _TessMin;
					float _TessMax;
					float _TessEdgeLength;
					float _TessMaxDisp;
				#endif

				uniform float _BaseSSSDirect;
				uniform float _BaseSSSAmbiet;
				uniform float _BaseSSSShadow;
				uniform float _BaseSSSNormalDistortion;
				uniform float _BaseSSSScattering;
				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform half ASPW_WindTreeSpeed;
				uniform half ASPW_WindTreeAmplitude;
				uniform float _WindTreeFlexibility;
				uniform half ASPW_WindTreeFlexibility;
				uniform float _WindTreeBaseRigidity;
				uniform float _WindLeafScale;
				uniform float _WindLeafSpeed;
				uniform half ASPW_WindTreeLeafSpeed;
				uniform float _WindLeafAmplitude;
				uniform half ASPW_WindTreeLeafAmplitude;
				uniform half ASPW_WindTreeLeafTurbulence;
				uniform float _WindLeafTurbulence;
				uniform float _WindLeafOffset;
				uniform half ASPW_WindTreeLeafOffset;
				uniform float ASPW_WindToggle;
				uniform half3 _BaseAlbedoColor;
				uniform sampler2D _BaseAlbedo;
				uniform float _BaseAlbedoDesaturation;
				uniform float _BaseAlbedoBrightness;
				uniform float _BaseLerpBetweenColorandTexture;
				uniform float _BaseTreeAOIntensity;
				uniform half ASP_GlobalTreeAO;
				uniform float3 _GradientColor;
				uniform float _GradientColorOffset;
				uniform float _GradientColorContrast;
				uniform float _GradientColorInvertMask;
				uniform float _GradientColorIntensity;
				uniform float3 _SecondColor;
				uniform float _SecondColorOffset;
				uniform float _SecondColorContrast;
				uniform float _SecondColorInvertMask;
				uniform float _SecondColorIntensity;
				uniform float4 _TintColor1;
				uniform float4 _TintColor2;
				uniform float _TintNoiseOffset;
				uniform half ASP_GlobalTintNoiseUVScale;
				uniform float _TintNoiseContrast;
				uniform half ASP_GlobalTintNoiseContrast;
				uniform float _TintNoiseIntensity;
				uniform half ASP_GlobalTintNoiseIntensity;
				uniform float3 _TopLayerColor;
				uniform sampler2D _BaseNormal;
				uniform float _BaseNormalIntensity;
				uniform half _TopLayerOffset;
				uniform half ASPT_TopLayerOffset;
				uniform half _TopLayerContrast;
				uniform half ASPT_TopLayerContrast;
				uniform half _TopLayerIntensity;
				uniform half ASPT_TopLayerIntensity;
				uniform half ASPT_TopLayerHeightStart;
				uniform float ASPT_TopLayerHeightFade;
				uniform half _TopLayerAOMask;
				uniform half _BaseSmoothnessMin;
				uniform float _BaseSmoothnessMax;
				uniform sampler2D _BaseSSE;
				uniform float _TopLayerSmoothnessIntensity;
				uniform float4 _BaseEmissiveColor;
				uniform float _BaseEmissiveMaskContrast;
				uniform float _BaseEmissiveIntensity;
				uniform float _BaseEmissiveAOMask;
				uniform float _BaseGlancingAngleCut;
				uniform float _CutOff;
				uniform float _BaseSSSIntensity;
				uniform half ASP_GlobalTreeSSSIntensity;
				uniform float _BaseSSSAOInfluence;
				uniform half ASP_GlobalTreeSSSAOInfluence;
				uniform half ASP_GlobalTreeSSSDistance;
				uniform float3 _BaseSSSColor;
				uniform float _TopLayerSSSIntensity;
				uniform float3 _TopLayerSSSColor;


				float3 ASESafeNormalize(float3 inVec)
				{
					float dp3 = max(1.175494351e-38, dot(inVec, inVec));
					return inVec* rsqrt(dp3);
				}
				
				float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
				{
					original -= center;
					float C = cos( angle );
					float S = sin( angle );
					float t = 1 - C;
					float m00 = t * u.x * u.x + C;
					float m01 = t * u.x * u.y - S * u.z;
					float m02 = t * u.x * u.z + S * u.y;
					float m10 = t * u.x * u.y + S * u.z;
					float m11 = t * u.y * u.y + C;
					float m12 = t * u.y * u.z - S * u.x;
					float m20 = t * u.x * u.z - S * u.y;
					float m21 = t * u.y * u.z + S * u.x;
					float m22 = t * u.z * u.z + C;
					float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
					return mul( finalMatrix, original ) + center;
				}
				

				v2f VertexFunction( appdata v  )
				{
					UNITY_SETUP_INSTANCE_ID(v);
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f,o);
					UNITY_TRANSFER_INSTANCE_ID(v,o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					float3 temp_cast_0 = (0.0).xxx;
					float3 Global_Wind_Direction493_g48 = ASPW_WindDirection;
					float3 worldToObjDir269_g48 = mul( unity_WorldToObject, float4( Global_Wind_Direction493_g48, 0.0 ) ).xyz;
					float3 normalizeResult268_g48 = ASESafeNormalize( worldToObjDir269_g48 );
					float3 Wind_Direction_Leaf85_g48 = normalizeResult268_g48;
					float3 break86_g48 = Wind_Direction_Leaf85_g48;
					float3 appendResult89_g48 = (float3(break86_g48.z , 0.0 , ( break86_g48.x * -1.0 )));
					float3 Wind_Direction91_g48 = appendResult89_g48;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Tree_Randomization149_g48 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_56_0_g48 = ( Wind_Tree_Randomization149_g48 + _Time.y );
					float Global_Wind_Tree_Speed491_g48 = ASPW_WindTreeSpeed;
					float temp_output_213_0_g48 = ( Global_Wind_Tree_Speed491_g48 * 7.0 );
					float Global_Wind_Tree_Amplitude464_g48 = ASPW_WindTreeAmplitude;
					float Global_Wind_Tree_Flexibility490_g48 = ASPW_WindTreeFlexibility;
					float temp_output_383_0_g48 = ( ( _WindTreeFlexibility * Global_Wind_Tree_Flexibility490_g48 ) * 0.3 );
					float Wind_Tree_257_g48 = ( ( ( ( ( sin( ( temp_output_56_0_g48 * ( temp_output_213_0_g48 * 0.1 ) ) ) + sin( ( temp_output_56_0_g48 * ( temp_output_213_0_g48 * 0.3 ) ) ) + sin( ( temp_output_56_0_g48 * ( temp_output_213_0_g48 * 0.5 ) ) ) ) + Global_Wind_Tree_Amplitude464_g48 ) * Global_Wind_Tree_Amplitude464_g48 ) * 0.01 ) * temp_output_383_0_g48 );
					float3 rotatedValue99_g48 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction91_g48, Wind_Tree_257_g48 );
					float3 Rotate_About_Axis354_g48 = ( rotatedValue99_g48 - v.vertex.xyz );
					float3 break452_g48 = normalizeResult268_g48;
					float3 appendResult454_g48 = (float3(break452_g48.x , 0.0 , break452_g48.z));
					float3 Wind_Direction_SM_Support453_g48 = appendResult454_g48;
					float Wind_Tree_Flexibility483_g48 = temp_output_383_0_g48;
					#ifdef _ENABLESTATICMESHSUPPORT_ON
					float3 staticSwitch482_g48 = ( Global_Wind_Tree_Amplitude464_g48 * Wind_Tree_257_g48 * Wind_Direction_SM_Support453_g48 * Wind_Tree_Flexibility483_g48 );
					#else
					float3 staticSwitch482_g48 = Rotate_About_Axis354_g48;
					#endif
					float3 Wind_Global450_g48 = staticSwitch482_g48;
					float2 texCoord433_g48 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float Wind_Mask_UV_CH2_VChannel434_g48 = texCoord433_g48.y;
					float saferPower113_g48 = abs( Wind_Mask_UV_CH2_VChannel434_g48 );
					float Wind_Tree_Mask114_g48 = pow( saferPower113_g48 , _WindTreeBaseRigidity );
					float3 temp_output_385_0_g48 = ( Wind_Global450_g48 * Wind_Tree_Mask114_g48 );
					float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( v.vertex ).xyz, 1 ) ).xyz;
					float temp_output_227_0_g48 = ( ( ase_positionWS.y * ( _WindLeafScale * 30.0 ) ) + _Time.y );
					float Global_Wind_Tree_Leaf_Speed495_g48 = ASPW_WindTreeLeafSpeed;
					float temp_output_223_0_g48 = ( _WindLeafSpeed * Global_Wind_Tree_Leaf_Speed495_g48 * 4.0 );
					float Global_Wind_Tree_Leaf_Amplitude497_g48 = ASPW_WindTreeLeafAmplitude;
					float temp_output_242_0_g48 = ( ( sin( ( temp_output_227_0_g48 * ( temp_output_223_0_g48 * 0.5 ) ) ) + sin( ( temp_output_227_0_g48 * ( temp_output_223_0_g48 * 1.25 ) ) ) + sin( ( temp_output_227_0_g48 * ( temp_output_223_0_g48 * 1.5 ) ) ) ) * ( ( _WindLeafAmplitude * Global_Wind_Tree_Leaf_Amplitude497_g48 ) * 0.1 ) );
					float temp_output_243_0_g48 = ( temp_output_242_0_g48 * 0.2 );
					float3 appendResult244_g48 = (float3(temp_output_243_0_g48 , temp_output_242_0_g48 , temp_output_243_0_g48));
					float3 Wind_Leaf245_g48 = appendResult244_g48;
					float3 break416_g48 = normalizeResult268_g48;
					float3 appendResult417_g48 = (float3(( break416_g48.x * -1.0 ) , ( break416_g48.z * -1.0 ) , 0.0));
					float3 Wind_Direction_Turbulence419_g48 = appendResult417_g48;
					float2 appendResult322_g48 = (float2(ase_positionWS.x , ase_positionWS.z));
					float Global_Wind_Tree_Leaf_Turbulence502_g48 = ASPW_WindTreeLeafTurbulence;
					float Wind_Leaf_Turbulence334_g48 = saturate( ( tex2Dlod( ASP_GlobalTintNoiseTexture, float4( ( ( ( Wind_Direction_Turbulence419_g48 * _Time.y ) * 0.1 ) + float3( ( appendResult322_g48 * 0.1 ) ,  0.0 ) ).xy, 0, 0.0) ).g + ( ( Global_Wind_Tree_Leaf_Turbulence502_g48 * _WindLeafTurbulence ) * 0.1 ) ) );
					float Wind_Mask_VColor_Blue429_g48 = v.ase_color.b;
					float3 Wind_Leaf_Mask285_g48 = ( ( ( Wind_Leaf245_g48 * Wind_Direction_Leaf85_g48 ) * Wind_Leaf_Turbulence334_g48 ) * Wind_Mask_VColor_Blue429_g48 );
					float Global_Wind_Tree_Leaf_Offset500_g48 = ASPW_WindTreeLeafOffset;
					float Wind_Mask_VColor_Green430_g48 = v.ase_color.g;
					float2 texCoord312_g48 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float3 Wind_Leaf_Offset314_g48 = ( ( ( ( Wind_Direction_Leaf85_g48 * ( _WindLeafOffset * Global_Wind_Tree_Leaf_Offset500_g48 * 0.5 ) ) + Wind_Leaf_Mask285_g48 ) * Wind_Mask_VColor_Green430_g48 ) * texCoord312_g48.y );
					float Global_Wind_Toggle504_g48 = ASPW_WindToggle;
					float3 lerpResult125_g48 = lerp( float3( 0,0,0 ) , ( temp_output_385_0_g48 + ( Wind_Leaf_Mask285_g48 + Wind_Leaf_Offset314_g48 ) ) , Global_Wind_Toggle504_g48);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch469_g48 = lerpResult125_g48;
					#else
					float3 staticSwitch469_g48 = temp_cast_0;
					#endif
					
					o.ase_texcoord6.xy = v.texcoord.xyzw.xy;
					o.ase_color = v.ase_color;
					o.ase_texcoord6.zw = v.texcoord2.xyzw.xy;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch469_g48;
					#ifdef ASE_ABSOLUTE_VERTEX_POS
						v.vertex.xyz = vertexValue;
					#else
						v.vertex.xyz += vertexValue;
					#endif
					v.vertex.w = 1;
					v.normal = v.normal;
					v.tangent = v.tangent;

					float3 positionWS = mul( unity_ObjectToWorld, v.vertex ).xyz;
					half3 normalWS = UnityObjectToWorldNormal( v.normal );
					half3 tangentWS = UnityObjectToWorldDir( v.tangent.xyz );

					o.pos = UnityObjectToClipPos( v.vertex );
					o.positionWS.xyz = positionWS;
					o.normalWS = normalWS;
					o.tangentWS = half4( tangentWS, v.tangent.w );

					o.ambientOrLightmapUV = 0;
					#ifdef LIGHTMAP_ON
						o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
					#elif UNITY_SHOULD_SAMPLE_SH
						#ifdef VERTEXLIGHT_ON
							o.ambientOrLightmapUV.rgb += Shade4PointLights(
								unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
								unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
								unity_4LightAtten0, positionWS, normalWS );
						#endif
						o.ambientOrLightmapUV.rgb = ShadeSHPerVertex( normalWS, o.ambientOrLightmapUV.rgb );
					#endif
					#ifdef DYNAMICLIGHTMAP_ON
						o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
					#endif

					#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
						o.tangentWS.zw = v.texcoord.xy;
						o.tangentWS.xy = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
					#endif

					UNITY_TRANSFER_LIGHTING(o, v.texcoord1.xy);
					#if defined( ASE_FOG )
						o.positionWS.w = o.pos.z;
					#endif
					return o;
				}

				#if defined(ASE_TESSELLATION)
				struct VertexControl
				{
					float4 vertex : INTERNALTESSPOS;
					half4 tangent : TANGENT;
					half3 normal : NORMAL;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					float4 ase_color : COLOR;

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct TessellationFactors
				{
					float edge[3] : SV_TessFactor;
					float inside : SV_InsideTessFactor;
				};

				VertexControl vert ( appdata v )
				{
					VertexControl o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					o.vertex = v.vertex;
					o.tangent = v.tangent;
					o.normal = v.normal;
					o.texcoord = v.texcoord;
					o.texcoord1 = v.texcoord1;
					o.texcoord2 = v.texcoord2;
					o.ase_color = v.ase_color;
					return o;
				}

				TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
				{
					TessellationFactors o;
					float4 tf = 1;
					float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
					float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
					#if defined(ASE_FIXED_TESSELLATION)
					tf = FixedTess( tessValue );
					#elif defined(ASE_DISTANCE_TESSELLATION)
					tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, UNITY_MATRIX_M, _WorldSpaceCameraPos );
					#elif defined(ASE_LENGTH_TESSELLATION)
					tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams );
					#elif defined(ASE_LENGTH_CULL_TESSELLATION)
					tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
					#endif
					o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
					return o;
				}

				[domain("tri")]
				[partitioning("fractional_odd")]
				[outputtopology("triangle_cw")]
				[patchconstantfunc("TessellationFunction")]
				[outputcontrolpoints(3)]
				VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
				{
				   return patch[id];
				}

				[domain("tri")]
				v2f DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
				{
					appdata o = (appdata) 0;
					o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
					o.tangent = patch[0].tangent * bary.x + patch[1].tangent * bary.y + patch[2].tangent * bary.z;
					o.normal = patch[0].normal * bary.x + patch[1].normal * bary.y + patch[2].normal * bary.z;
					o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
					o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
					o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
					o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
					#if defined(ASE_PHONG_TESSELLATION)
					float3 pp[3];
					for (int i = 0; i < 3; ++i)
						pp[i] = o.vertex.xyz - patch[i].normal * (dot(o.vertex.xyz, patch[i].normal) - dot(patch[i].vertex.xyz, patch[i].normal));
					float phongStrength = _TessPhongStrength;
					o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
					#endif
					UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
					return VertexFunction(o);
				}
				#else
				v2f vert ( appdata v )
				{
					return VertexFunction( v );
				}
				#endif

				half4 frag( v2f IN , uint ase_vface : SV_IsFrontFace
							#if defined( ASE_DEPTH_WRITE_ON )
								, out float outputDepth : SV_Depth
							#endif
							) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(IN);

					#ifdef LOD_FADE_CROSSFADE
						UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
					#endif

					#if defined(ASE_LIGHTING_SIMPLE)
						SurfaceOutput o = (SurfaceOutput)0;
					#else
						#if defined(_SPECULAR_SETUP)
							SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
						#else
							SurfaceOutputStandard o = (SurfaceOutputStandard)0;
						#endif
					#endif

					half atten;
					{
						#if defined( ASE_RECEIVE_SHADOWS )
							UNITY_LIGHT_ATTENUATION( temp, IN, IN.positionWS.xyz )
							atten = temp;
						#else
							atten = 1;
						#endif
					}

					float3 PositionWS = IN.positionWS.xyz;
					half3 ViewDirWS = normalize( UnityWorldSpaceViewDir( PositionWS ) );
					float4 ScreenPosNorm = float4( IN.pos.xy * ( _ScreenParams.zw - 1.0 ), IN.pos.zw );
					float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, IN.pos.z ) * IN.pos.w;
					float4 ScreenPos = ComputeScreenPos( ClipPos );
					half3 NormalWS = IN.normalWS;
					half3 TangentWS = IN.tangentWS.xyz;
					half3 BitangentWS = cross( IN.normalWS, IN.tangentWS.xyz ) * IN.tangentWS.w * unity_WorldTransformParams.w;
					half3 LightAtten = atten;
					float FogCoord = IN.positionWS.w;

					#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
						float2 sampleCoords = (IN.tangentWS.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
						NormalWS = UnityObjectToWorldNormal(normalize(tex2D(_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
						TangentWS = -cross(unity_ObjectToWorld._13_23_33, NormalWS);
						BitangentWS = cross(NormalWS, -TangentWS);
					#endif

					float2 uv_BaseAlbedo1753 = IN.ase_texcoord6.xy;
					float4 tex2DNode1753 = tex2D( _BaseAlbedo, uv_BaseAlbedo1753 );
					float3 desaturateInitialColor1712 = tex2DNode1753.rgb;
					float desaturateDot1712 = dot( desaturateInitialColor1712, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar1712 = lerp( desaturateInitialColor1712, desaturateDot1712.xxx, _BaseAlbedoDesaturation );
					float3 blendOpSrc3324 = ( desaturateVar1712 * _BaseAlbedoBrightness );
					float3 blendOpDest3324 = _BaseAlbedoColor;
					float3 lerpResult1716 = lerp( _BaseAlbedoColor , ( saturate( (( blendOpDest3324 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest3324 ) * ( 1.0 - blendOpSrc3324 ) ) : ( 2.0 * blendOpDest3324 * blendOpSrc3324 ) ) )) , _BaseLerpBetweenColorandTexture);
					float lerpResult1727 = lerp( 1.0 , IN.ase_color.a , saturate( ( _BaseTreeAOIntensity * ASP_GlobalTreeAO ) ));
					float Base_AO1735 = lerpResult1727;
					float3 Base_Albedo1723 = ( lerpResult1716 * Base_AO1735 );
					float3 blendOpSrc1836 = Base_Albedo1723;
					float3 blendOpDest1836 = _GradientColor;
					float3 temp_output_1836_0 = ( saturate( (( blendOpDest1836 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest1836 ) * ( 1.0 - blendOpSrc1836 ) ) : ( 2.0 * blendOpDest1836 * blendOpSrc1836 ) ) ));
					float2 texCoord1840 = IN.ase_texcoord6.zw * float2( 1,1 ) + float2( 0,0 );
					float saferPower1879 = abs( ( ( 1.0 - texCoord1840.y ) * _GradientColorOffset ) );
					float temp_output_1848_0 = saturate( pow( saferPower1879 , _GradientColorContrast ) );
					float lerpResult1877 = lerp( temp_output_1848_0 , ( 1.0 - temp_output_1848_0 ) , _GradientColorInvertMask);
					float Gradient_Color_Mask1854 = ( lerpResult1877 * _GradientColorIntensity );
					float3 lerpResult1838 = lerp( Base_Albedo1723 , temp_output_1836_0 , Gradient_Color_Mask1854);
					float3 Gradient_Color_Albedo1856 = lerpResult1838;
					#ifdef _ENABLEGRADIENTCOLOR_ON
					float3 staticSwitch1867 = Gradient_Color_Albedo1856;
					#else
					float3 staticSwitch1867 = Base_Albedo1723;
					#endif
					float3 Step_2_Albedo1914 = staticSwitch1867;
					float3 blendOpSrc1897 = Base_Albedo1723;
					float3 blendOpDest1897 = _SecondColor;
					float3 temp_output_1897_0 = ( saturate( (( blendOpDest1897 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest1897 ) * ( 1.0 - blendOpSrc1897 ) ) : ( 2.0 * blendOpDest1897 * blendOpSrc1897 ) ) ));
					float saferPower1886 = abs( ( ( 1.0 - IN.ase_color.a ) * _SecondColorOffset ) );
					float temp_output_1889_0 = saturate( pow( saferPower1886 , ( _SecondColorContrast + 2.0 ) ) );
					float lerpResult1890 = lerp( temp_output_1889_0 , ( 1.0 - temp_output_1889_0 ) , _SecondColorInvertMask);
					float Second_Color_Mask1895 = ( lerpResult1890 * _SecondColorIntensity );
					float3 lerpResult1899 = lerp( Step_2_Albedo1914 , temp_output_1897_0 , Second_Color_Mask1895);
					float3 Second_Color_Albedo1902 = lerpResult1899;
					#ifdef _ENABLESECONDCOLOR_ON
					float3 staticSwitch1921 = Second_Color_Albedo1902;
					#else
					float3 staticSwitch1921 = Step_2_Albedo1914;
					#endif
					float3 Step_3_Albedo1954 = staticSwitch1921;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Tint_Color_Mask1949 = saturate( round( ( frac( ( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) * ( _TintNoiseOffset * ASP_GlobalTintNoiseUVScale ) ) ) * ( _TintNoiseContrast * ASP_GlobalTintNoiseContrast ) ) ) );
					float3 lerpResult1951 = lerp( _TintColor1.rgb , _TintColor2.rgb , Tint_Color_Mask1949);
					float3 blendOpSrc1952 = Step_3_Albedo1954;
					float3 blendOpDest1952 = lerpResult1951;
					float3 temp_output_1952_0 = ( saturate( (( blendOpDest1952 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest1952 ) * ( 1.0 - blendOpSrc1952 ) ) : ( 2.0 * blendOpDest1952 * blendOpSrc1952 ) ) ));
					float temp_output_1961_0 = ( _TintNoiseIntensity * ASP_GlobalTintNoiseIntensity );
					float3 lerpResult1956 = lerp( Step_3_Albedo1954 , temp_output_1952_0 , temp_output_1961_0);
					float3 Tint_Color_Albedo1963 = lerpResult1956;
					#ifdef _ENABLETINTCOLOR_ON
					float3 staticSwitch2080 = Tint_Color_Albedo1963;
					#else
					float3 staticSwitch2080 = Step_3_Albedo1954;
					#endif
					float3 Step_4_Albedo2083 = staticSwitch2080;
					float3 Top_Layer_Color1995 = _TopLayerColor;
					float2 uv_BaseNormal1741 = IN.ase_texcoord6.xy;
					float3 tex2DNode1741 = UnpackScaleNormal( tex2D( _BaseNormal, uv_BaseNormal1741 ), _BaseNormalIntensity );
					float3 break105_g45 = tex2DNode1741;
					float switchResult107_g45 = (((ase_vface>0)?(break105_g45.z):(-break105_g45.z)));
					float3 appendResult108_g45 = (float3(break105_g45.x , break105_g45.y , switchResult107_g45));
					#ifdef _ENABLEFLIPNORMALS_ON
					float3 staticSwitch1744 = appendResult108_g45;
					#else
					float3 staticSwitch1744 = tex2DNode1741;
					#endif
					float3 Base_Normal1747 = staticSwitch1744;
					float3 desaturateInitialColor2013 = Base_Normal1747;
					float desaturateDot2013 = dot( desaturateInitialColor2013, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar2013 = lerp( desaturateInitialColor2013, desaturateDot2013.xxx, 1.0 );
					float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
					float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
					float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
					float3 tanNormal2012 = Base_Normal1747;
					float3 worldNormal2012 = float3( dot( tanToWorld0, tanNormal2012 ), dot( tanToWorld1, tanNormal2012 ), dot( tanToWorld2, tanNormal2012 ) );
					float3 temp_cast_0 = (worldNormal2012.y).xxx;
					#ifdef _ENABLEWORLDPROJECTION_ON
					float3 staticSwitch2017 = temp_cast_0;
					#else
					float3 staticSwitch2017 = desaturateVar2013;
					#endif
					float3 saferPower17_g46 = abs( abs( ( saturate( staticSwitch2017 ) + ( _TopLayerOffset * ASPT_TopLayerOffset ) ) ) );
					float temp_output_2005_0 = ( _TopLayerContrast * ASPT_TopLayerContrast );
					float3 temp_cast_1 = (temp_output_2005_0).xxx;
					float saferPower2024 = abs( ( 1.0 - IN.ase_color.a ) );
					float Top_Layer_Contrast2025 = temp_output_2005_0;
					float3 lerpResult2029 = lerp( ( ( saturate( pow( saferPower17_g46 , temp_cast_1 ) ) * ( _TopLayerIntensity * ASPT_TopLayerIntensity ) ) * saturate(  (0.0 + ( PositionWS.y - ASPT_TopLayerHeightStart ) * ( 1.0 - 0.0 ) / ( ( ASPT_TopLayerHeightStart + ASPT_TopLayerHeightFade ) - ASPT_TopLayerHeightStart ) ) ) ) , float3( 0,0,0 ) , saturate( ( pow( saferPower2024 , Top_Layer_Contrast2025 ) * _TopLayerAOMask ) ));
					float3 Top_Layer_Mask2031 = saturate( lerpResult2029 );
					float3 lerpResult2034 = lerp( Step_4_Albedo2083 , Top_Layer_Color1995 , Top_Layer_Mask2031);
					float3 lerpResult2037 = lerp( Step_4_Albedo2083 , lerpResult2034 , saturate( ( ase_vface > 0 ? +1 : -1 ) ));
					#ifdef _ENABLEBACKFACEPROJECTION_ON
					float3 staticSwitch2041 = lerpResult2034;
					#else
					float3 staticSwitch2041 = lerpResult2037;
					#endif
					float3 Top_Layer_Albedo2042 = staticSwitch2041;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch2043 = Top_Layer_Albedo2042;
					#else
					float3 staticSwitch2043 = Step_4_Albedo2083;
					#endif
					float3 Output_Albedo1873 = staticSwitch2043;
					
					float2 uv_BaseSSE1757 = IN.ase_texcoord6.xy;
					float4 tex2DNode1757 = tex2D( _BaseSSE, uv_BaseSSE1757 );
					float Texture_Smoothness3683 = tex2DNode1757.r;
					float lerpResult3679 = lerp( _BaseSmoothnessMin , _BaseSmoothnessMax , Texture_Smoothness3683);
					float Base_Smoothness1760 = saturate( lerpResult3679 );
					float lerpResult2064 = lerp( Base_Smoothness1760 , _TopLayerSmoothnessIntensity , Top_Layer_Mask2031.x);
					float lerpResult2069 = lerp( Base_Smoothness1760 , lerpResult2064 , saturate( ( ase_vface > 0 ? +1 : -1 ) ));
					#ifdef _ENABLEBACKFACEPROJECTION_ON
					float staticSwitch2072 = lerpResult2064;
					#else
					float staticSwitch2072 = lerpResult2069;
					#endif
					float Top_Layer_Smoothness2073 = saturate( staticSwitch2072 );
					#ifdef _ENABLETOPLAYERBLEND_ON
					float staticSwitch2074 = Top_Layer_Smoothness2073;
					#else
					float staticSwitch2074 = Base_Smoothness1760;
					#endif
					float Output_Smoothness2077 = staticSwitch2074;
					
					float RSE_Emissive_Mask1762 = tex2DNode1757.b;
					float saferPower1804 = abs( RSE_Emissive_Mask1762 );
					float3 temp_output_1806_0 = ( ( _BaseEmissiveColor.rgb * pow( saferPower1804 , _BaseEmissiveMaskContrast ) ) * _BaseEmissiveIntensity );
					float3 lerpResult1808 = lerp( temp_output_1806_0 , ( temp_output_1806_0 * IN.ase_color.a ) , _BaseEmissiveAOMask);
					float3 Base_Emissive1812 = lerpResult1808;
					float3 lerpResult3341 = lerp( Base_Emissive1812 , float3( 0,0,0 ) , Top_Layer_Mask2031);
					float3 Top_Layer_Emissive3342 = lerpResult3341;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch3343 = Top_Layer_Emissive3342;
					#else
					float3 staticSwitch3343 = Base_Emissive1812;
					#endif
					float3 Output_Emissive3346 = staticSwitch3343;
					
					float Opacity_Texture1737 = tex2DNode1753.a;
					float3 normalizeResult1780 = normalize( cross( ddx( PositionWS ) , ddy( PositionWS ) ) );
					float3 normalizeResult1787 = normalize( ( _WorldSpaceCameraPos - PositionWS ) );
					float dotResult1782 = dot( normalizeResult1780 , normalizeResult1787 );
					float lerpResult1794 = lerp( 1.0 , abs( dotResult1782 ) , _BaseGlancingAngleCut);
					#ifdef _ENABLEGLANCINGANGLECUT_ON
					float staticSwitch1788 = lerpResult1794;
					#else
					float staticSwitch1788 = 1.0;
					#endif
					float Base_Glancing_Angle1790 = staticSwitch1788;
					float Base_Opacity1739 = ( Opacity_Texture1737 * Base_Glancing_Angle1790 );
					
					float Local_SSS_Intensity3538 = _BaseSSSIntensity;
					float RSE_Subsurface_Mask1761 = tex2DNode1757.g;
					float lerpResult1726 = lerp( 1.0 , IN.ase_color.a , saturate( ( _BaseSSSAOInfluence * ASP_GlobalTreeSSSAOInfluence ) ));
					float Base_SSS_AO_Influence1734 = lerpResult1726;
					float clampResult3460 = clamp( ( 1.0 -  (0.0 + ( distance( PositionWS , _WorldSpaceCameraPos ) - 0.0 ) * ( 1.0 - 0.0 ) / ( ASP_GlobalTreeSSSDistance - 0.0 ) ) ) , 0.0 , 1.0 );
					float Base_SSS_Max_Distance3461 = clampResult3460;
					float3 Base_SSS1769 = ( ( ( ( ( Local_SSS_Intensity3538 * ASP_GlobalTreeSSSIntensity * 2.0 ) * RSE_Subsurface_Mask1761 ) * Base_SSS_AO_Influence1734 ) * Base_SSS_Max_Distance3461 ) * _BaseSSSColor );
					float3 lerpResult1859 = lerp( ( Base_Albedo1723 * Base_SSS1769 ) , ( temp_output_1836_0 * Base_SSS1769 ) , Gradient_Color_Mask1854);
					float3 Gradient_Color_SSS1864 = lerpResult1859;
					#ifdef _ENABLEGRADIENTCOLOR_ON
					float3 staticSwitch1869 = Gradient_Color_SSS1864;
					#else
					float3 staticSwitch1869 = ( Base_Albedo1723 * Base_SSS1769 );
					#endif
					float3 Step_2_SSS1915 = staticSwitch1869;
					float3 lerpResult1908 = lerp( Step_2_SSS1915 , ( temp_output_1897_0 * Base_SSS1769 ) , Second_Color_Mask1895);
					float3 Second_Color_SSS1910 = lerpResult1908;
					#ifdef _ENABLESECONDCOLOR_ON
					float3 staticSwitch1922 = Second_Color_SSS1910;
					#else
					float3 staticSwitch1922 = Step_2_SSS1915;
					#endif
					float3 Step_3_SSS1955 = staticSwitch1922;
					float3 lerpResult1966 = lerp( Step_3_SSS1955 , ( temp_output_1952_0 * Base_SSS1769 ) , temp_output_1961_0);
					float3 Tint_Color_SSS1968 = lerpResult1966;
					#ifdef _ENABLETINTCOLOR_ON
					float3 staticSwitch2081 = Tint_Color_SSS1968;
					#else
					float3 staticSwitch2081 = Step_3_SSS1955;
					#endif
					float3 Step_4_SSS2084 = staticSwitch2081;
					float Global_SSS_Intensity2050 = ASP_GlobalTreeSSSIntensity;
					float3 lerpResult2054 = lerp( Step_4_SSS2084 , ( ( Top_Layer_Color1995 * _TopLayerSSSIntensity * Global_SSS_Intensity2050 ) * _TopLayerSSSColor ) , Top_Layer_Mask2031);
					float3 lerpResult2059 = lerp( Step_4_SSS2084 , lerpResult2054 , saturate( ( ase_vface > 0 ? +1 : -1 ) ));
					#ifdef _ENABLEBACKFACEPROJECTION_ON
					float3 staticSwitch2060 = lerpResult2054;
					#else
					float3 staticSwitch2060 = lerpResult2059;
					#endif
					float3 Top_Layer_SSS2061 = staticSwitch2060;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch2045 = Top_Layer_SSS2061;
					#else
					float3 staticSwitch2045 = Step_4_SSS2084;
					#endif
					float3 Output_SSS1874 = staticSwitch2045;
					

					o.Albedo = Output_Albedo1873;
					o.Normal = Base_Normal1747;

					half3 Specular = half3( 0, 0, 0 );
					half Metallic = 0;
					half Smoothness = Output_Smoothness2077;
					half Occlusion = Base_AO1735;

					#if defined(ASE_LIGHTING_SIMPLE)
						o.Specular = Specular.x;
						o.Gloss = Smoothness;
					#else
						#if defined(_SPECULAR_SETUP)
							o.Specular = Specular;
						#else
							o.Metallic = Metallic;
						#endif
						o.Occlusion = Occlusion;
						o.Smoothness = Smoothness;
					#endif

					o.Emission = Output_Emissive3346;
					o.Alpha = Base_Opacity1739;
					half AlphaClipThreshold = _CutOff;
					half AlphaClipThresholdShadow = 0.5;
					half3 BakedGI = 0;
					half3 Transmission = 1;
					half3 Translucency = Output_SSS1874;

					#if defined( ASE_DEPTH_WRITE_ON )
						float DeviceDepth = IN.pos.z;
					#endif

					#ifdef _ALPHATEST_ON
						clip( o.Alpha - AlphaClipThreshold );
					#endif

					#if defined( ASE_CHANGES_WORLD_POS )
					{
						#if defined( ASE_RECEIVE_SHADOWS )
							UNITY_LIGHT_ATTENUATION( temp, IN, PositionWS )
							LightAtten = temp;
						#else
							LightAtten = 1;
						#endif
					}
					#endif

					#if ( ASE_FRAGMENT_NORMAL == 0 )
						o.Normal = normalize( o.Normal.x * TangentWS + o.Normal.y * BitangentWS + o.Normal.z * NormalWS );
					#elif ( ASE_FRAGMENT_NORMAL == 1 )
						o.Normal = UnityObjectToWorldNormal( o.Normal );
					#elif ( ASE_FRAGMENT_NORMAL == 2 )
						// @diogo: already in world-space; do nothing
					#endif

					#if defined( ASE_DEPTH_WRITE_ON )
						outputDepth = DeviceDepth;
					#endif

					#ifndef USING_DIRECTIONAL_LIGHT
						half3 lightDir = normalize( UnityWorldSpaceLightDir( PositionWS ) );
					#else
						half3 lightDir = _WorldSpaceLightPos0.xyz;
					#endif

					UnityGI gi;
					UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
					gi.indirect.diffuse = 0;
					gi.indirect.specular = 0;
					gi.light.color = _LightColor0.rgb;
					gi.light.dir = lightDir;

					UnityGIInput giInput;
					UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
					giInput.light = gi.light;
					giInput.worldPos = PositionWS;
					giInput.worldViewDir = ViewDirWS;
					giInput.atten = atten;
					#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
						giInput.lightmapUV = IN.ambientOrLightmapUV;
					#else
						giInput.lightmapUV = 0.0;
					#endif
					#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
						giInput.ambient = IN.ambientOrLightmapUV.rgb;
					#else
						giInput.ambient.rgb = 0.0;
					#endif
					giInput.probeHDR[0] = unity_SpecCube0_HDR;
					giInput.probeHDR[1] = unity_SpecCube1_HDR;
					#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
						giInput.boxMin[0] = unity_SpecCube0_BoxMin;
					#endif
					#ifdef UNITY_SPECCUBE_BOX_PROJECTION
						giInput.boxMax[0] = unity_SpecCube0_BoxMax;
						giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
						giInput.boxMax[1] = unity_SpecCube1_BoxMax;
						giInput.boxMin[1] = unity_SpecCube1_BoxMin;
						giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
					#endif

					#if defined(ASE_LIGHTING_SIMPLE)
						#if defined(_SPECULAR_SETUP)
							LightingBlinnPhong_GI(o, giInput, gi);
						#else
							LightingLambert_GI(o, giInput, gi);
						#endif
					#else
						#if defined(_SPECULAR_SETUP)
							LightingStandardSpecular_GI(o, giInput, gi);
						#else
							LightingStandard_GI(o, giInput, gi);
						#endif
					#endif

					#ifdef ASE_BAKEDGI
						gi.indirect.diffuse = BakedGI;
					#endif

					#if UNITY_SHOULD_SAMPLE_SH && !defined(LIGHTMAP_ON) && defined(ASE_NO_AMBIENT)
						gi.indirect.diffuse = 0;
					#endif

					half4 c = 0;
					#if defined(ASE_LIGHTING_SIMPLE)
						#if defined(_SPECULAR_SETUP)
							c += LightingBlinnPhong (o, ViewDirWS, gi);
						#else
							c += LightingLambert( o, gi );
						#endif
					#else
						#if defined(_SPECULAR_SETUP)
							c += LightingStandardSpecular (o, ViewDirWS, gi);
						#else
							c += LightingStandard(o, ViewDirWS, gi);
						#endif
					#endif

					#ifdef ASE_TRANSMISSION
					{
						half shadow = _TransmissionShadow;
						#ifdef DIRECTIONAL
							half3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, shadow );
						#else
							half3 lightAtten = gi.light.color;
						#endif
						half3 transmission = max(0 , -dot(o.Normal, gi.light.dir)) * lightAtten * Transmission;
						c.rgb += o.Albedo * transmission;
					}
					#endif

					#ifdef ASE_TRANSLUCENCY
					{
						half shadow = _BaseSSSShadow;
						half normal = _BaseSSSNormalDistortion;
						half scattering = _BaseSSSScattering;
						half direct = _BaseSSSDirect;
						half ambient = _BaseSSSAmbiet;
						half strength = _BaseSSSIntensity;

						#ifdef DIRECTIONAL
							half3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, shadow );
						#else
							half3 lightAtten = gi.light.color;
						#endif
						half3 lightDir = gi.light.dir + o.Normal * normal;
						half transVdotL = pow( saturate( dot( ViewDirWS, -lightDir ) ), scattering );
						half3 translucency = lightAtten * (transVdotL * direct + gi.indirect.diffuse * ambient) * Translucency;
						c.rgb += o.Albedo * translucency * strength;
					}
					#endif

					c.rgb += o.Emission;

					#if defined( ASE_FOG )
						UNITY_APPLY_FOG( FogCoord, c );
					#endif
					return c;
				}
			ENDCG
		}

		
		Pass
		{
			
			Name "ForwardAdd"
			Tags { "LightMode"="ForwardAdd" }
			ZWrite Off
			Blend One One

			CGPROGRAM
				#define ASE_GEOMETRY 1
				#define ASE_FRAGMENT_NORMAL 0
				#define ASE_RECEIVE_SHADOWS
				#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
				#pragma multi_compile_instancing
				#pragma multi_compile_fog
				#define ASE_FOG
				#define ASE_TRANSLUCENCY 1
				#pragma multi_compile _ LOD_FADE_CROSSFADE
				#pragma multi_compile_local _ALPHATEST_ON
				#define ASE_VERSION 19903

				#pragma vertex vert
				#pragma fragment frag
				#pragma skip_variants INSTANCING_ON
				#pragma multi_compile_fwdadd_fullshadows
				#ifndef UNITY_PASS_FORWARDADD
					#define UNITY_PASS_FORWARDADD
				#endif
				#include "HLSLSupport.cginc"
				#ifdef ASE_GEOMETRY
					#ifndef UNITY_INSTANCED_LOD_FADE
						#define UNITY_INSTANCED_LOD_FADE
					#endif
					#ifndef UNITY_INSTANCED_SH
						#define UNITY_INSTANCED_SH
					#endif
					#ifndef UNITY_INSTANCED_LIGHTMAPSTS
						#define UNITY_INSTANCED_LIGHTMAPSTS
					#endif
				#endif
				#include "UnityShaderVariables.cginc"
				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "UnityPBSLighting.cginc"
				#include "AutoLight.cginc"

				#if defined(UNITY_INSTANCING_ENABLED) && ( defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL) || defined(_INSTANCEDTERRAINNORMALS_PIXEL) )
					#define ENABLE_TERRAIN_PERPIXEL_NORMAL
				#endif

				#include "UnityStandardUtils.cginc"
				#define ASE_NEEDS_VERT_POSITION
				#define ASE_NEEDS_TEXTURE_COORDINATES2
				#define ASE_NEEDS_VERT_TEXTURE_COORDINATES2
				#define ASE_NEEDS_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_COLOR
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_WORLD_TANGENT
				#define ASE_NEEDS_WORLD_NORMAL
				#define ASE_NEEDS_FRAG_WORLD_NORMAL
				#define ASE_NEEDS_FRAG_WORLD_BITANGENT
				#define ASE_NEEDS_WORLD_POSITION
				#define ASE_NEEDS_FRAG_WORLD_POSITION
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLESTATICMESHSUPPORT_ON
				#pragma shader_feature_local _ENABLETOPLAYERBLEND_ON
				#pragma shader_feature_local _ENABLETINTCOLOR_ON
				#pragma shader_feature_local _ENABLESECONDCOLOR_ON
				#pragma shader_feature_local _ENABLEGRADIENTCOLOR_ON
				#pragma shader_feature_local _ENABLEBACKFACEPROJECTION_ON
				#pragma shader_feature_local _ENABLEWORLDPROJECTION_ON
				#pragma shader_feature_local _ENABLEFLIPNORMALS_ON
				#pragma shader_feature_local _ENABLEGLANCINGANGLECUT_ON


				struct appdata
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					half4 tangent : TANGENT;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					float4 ase_color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float4 positionWS : TEXCOORD0; // xyz = positionWS, w = fogCoord
					half3 normalWS : TEXCOORD1;
					float4 tangentWS : TEXCOORD2; // holds terrainUV ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
					UNITY_LIGHTING_COORDS( 3, 4 )
					float4 ase_texcoord5 : TEXCOORD5;
					float4 ase_color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				#ifdef ASE_TRANSMISSION
					float _TransmissionShadow;
				#endif
				#ifdef ASE_TRANSLUCENCY
					float _TransStrength;
					float _TransNormal;
					float _TransScattering;
					float _TransDirect;
					float _TransAmbient;
					float _TransShadow;
				#endif
				#ifdef ASE_TESSELLATION
					float _TessPhongStrength;
					float _TessValue;
					float _TessMin;
					float _TessMax;
					float _TessEdgeLength;
					float _TessMaxDisp;
				#endif

				uniform float _BaseSSSDirect;
				uniform float _BaseSSSAmbiet;
				uniform float _BaseSSSShadow;
				uniform float _BaseSSSNormalDistortion;
				uniform float _BaseSSSScattering;
				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform half ASPW_WindTreeSpeed;
				uniform half ASPW_WindTreeAmplitude;
				uniform float _WindTreeFlexibility;
				uniform half ASPW_WindTreeFlexibility;
				uniform float _WindTreeBaseRigidity;
				uniform float _WindLeafScale;
				uniform float _WindLeafSpeed;
				uniform half ASPW_WindTreeLeafSpeed;
				uniform float _WindLeafAmplitude;
				uniform half ASPW_WindTreeLeafAmplitude;
				uniform half ASPW_WindTreeLeafTurbulence;
				uniform float _WindLeafTurbulence;
				uniform float _WindLeafOffset;
				uniform half ASPW_WindTreeLeafOffset;
				uniform float ASPW_WindToggle;
				uniform half3 _BaseAlbedoColor;
				uniform sampler2D _BaseAlbedo;
				uniform float _BaseAlbedoDesaturation;
				uniform float _BaseAlbedoBrightness;
				uniform float _BaseLerpBetweenColorandTexture;
				uniform float _BaseTreeAOIntensity;
				uniform half ASP_GlobalTreeAO;
				uniform float3 _GradientColor;
				uniform float _GradientColorOffset;
				uniform float _GradientColorContrast;
				uniform float _GradientColorInvertMask;
				uniform float _GradientColorIntensity;
				uniform float3 _SecondColor;
				uniform float _SecondColorOffset;
				uniform float _SecondColorContrast;
				uniform float _SecondColorInvertMask;
				uniform float _SecondColorIntensity;
				uniform float4 _TintColor1;
				uniform float4 _TintColor2;
				uniform float _TintNoiseOffset;
				uniform half ASP_GlobalTintNoiseUVScale;
				uniform float _TintNoiseContrast;
				uniform half ASP_GlobalTintNoiseContrast;
				uniform float _TintNoiseIntensity;
				uniform half ASP_GlobalTintNoiseIntensity;
				uniform float3 _TopLayerColor;
				uniform sampler2D _BaseNormal;
				uniform float _BaseNormalIntensity;
				uniform half _TopLayerOffset;
				uniform half ASPT_TopLayerOffset;
				uniform half _TopLayerContrast;
				uniform half ASPT_TopLayerContrast;
				uniform half _TopLayerIntensity;
				uniform half ASPT_TopLayerIntensity;
				uniform half ASPT_TopLayerHeightStart;
				uniform float ASPT_TopLayerHeightFade;
				uniform half _TopLayerAOMask;
				uniform half _BaseSmoothnessMin;
				uniform float _BaseSmoothnessMax;
				uniform sampler2D _BaseSSE;
				uniform float _TopLayerSmoothnessIntensity;
				uniform float4 _BaseEmissiveColor;
				uniform float _BaseEmissiveMaskContrast;
				uniform float _BaseEmissiveIntensity;
				uniform float _BaseEmissiveAOMask;
				uniform float _BaseGlancingAngleCut;
				uniform float _CutOff;
				uniform float _BaseSSSIntensity;
				uniform half ASP_GlobalTreeSSSIntensity;
				uniform float _BaseSSSAOInfluence;
				uniform half ASP_GlobalTreeSSSAOInfluence;
				uniform half ASP_GlobalTreeSSSDistance;
				uniform float3 _BaseSSSColor;
				uniform float _TopLayerSSSIntensity;
				uniform float3 _TopLayerSSSColor;


				float3 ASESafeNormalize(float3 inVec)
				{
					float dp3 = max(1.175494351e-38, dot(inVec, inVec));
					return inVec* rsqrt(dp3);
				}
				
				float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
				{
					original -= center;
					float C = cos( angle );
					float S = sin( angle );
					float t = 1 - C;
					float m00 = t * u.x * u.x + C;
					float m01 = t * u.x * u.y - S * u.z;
					float m02 = t * u.x * u.z + S * u.y;
					float m10 = t * u.x * u.y + S * u.z;
					float m11 = t * u.y * u.y + C;
					float m12 = t * u.y * u.z - S * u.x;
					float m20 = t * u.x * u.z - S * u.y;
					float m21 = t * u.y * u.z + S * u.x;
					float m22 = t * u.z * u.z + C;
					float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
					return mul( finalMatrix, original ) + center;
				}
				

				v2f VertexFunction (appdata v  ) {
					UNITY_SETUP_INSTANCE_ID(v);
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f,o);
					UNITY_TRANSFER_INSTANCE_ID(v,o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					float3 temp_cast_0 = (0.0).xxx;
					float3 Global_Wind_Direction493_g48 = ASPW_WindDirection;
					float3 worldToObjDir269_g48 = mul( unity_WorldToObject, float4( Global_Wind_Direction493_g48, 0.0 ) ).xyz;
					float3 normalizeResult268_g48 = ASESafeNormalize( worldToObjDir269_g48 );
					float3 Wind_Direction_Leaf85_g48 = normalizeResult268_g48;
					float3 break86_g48 = Wind_Direction_Leaf85_g48;
					float3 appendResult89_g48 = (float3(break86_g48.z , 0.0 , ( break86_g48.x * -1.0 )));
					float3 Wind_Direction91_g48 = appendResult89_g48;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Tree_Randomization149_g48 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_56_0_g48 = ( Wind_Tree_Randomization149_g48 + _Time.y );
					float Global_Wind_Tree_Speed491_g48 = ASPW_WindTreeSpeed;
					float temp_output_213_0_g48 = ( Global_Wind_Tree_Speed491_g48 * 7.0 );
					float Global_Wind_Tree_Amplitude464_g48 = ASPW_WindTreeAmplitude;
					float Global_Wind_Tree_Flexibility490_g48 = ASPW_WindTreeFlexibility;
					float temp_output_383_0_g48 = ( ( _WindTreeFlexibility * Global_Wind_Tree_Flexibility490_g48 ) * 0.3 );
					float Wind_Tree_257_g48 = ( ( ( ( ( sin( ( temp_output_56_0_g48 * ( temp_output_213_0_g48 * 0.1 ) ) ) + sin( ( temp_output_56_0_g48 * ( temp_output_213_0_g48 * 0.3 ) ) ) + sin( ( temp_output_56_0_g48 * ( temp_output_213_0_g48 * 0.5 ) ) ) ) + Global_Wind_Tree_Amplitude464_g48 ) * Global_Wind_Tree_Amplitude464_g48 ) * 0.01 ) * temp_output_383_0_g48 );
					float3 rotatedValue99_g48 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction91_g48, Wind_Tree_257_g48 );
					float3 Rotate_About_Axis354_g48 = ( rotatedValue99_g48 - v.vertex.xyz );
					float3 break452_g48 = normalizeResult268_g48;
					float3 appendResult454_g48 = (float3(break452_g48.x , 0.0 , break452_g48.z));
					float3 Wind_Direction_SM_Support453_g48 = appendResult454_g48;
					float Wind_Tree_Flexibility483_g48 = temp_output_383_0_g48;
					#ifdef _ENABLESTATICMESHSUPPORT_ON
					float3 staticSwitch482_g48 = ( Global_Wind_Tree_Amplitude464_g48 * Wind_Tree_257_g48 * Wind_Direction_SM_Support453_g48 * Wind_Tree_Flexibility483_g48 );
					#else
					float3 staticSwitch482_g48 = Rotate_About_Axis354_g48;
					#endif
					float3 Wind_Global450_g48 = staticSwitch482_g48;
					float2 texCoord433_g48 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float Wind_Mask_UV_CH2_VChannel434_g48 = texCoord433_g48.y;
					float saferPower113_g48 = abs( Wind_Mask_UV_CH2_VChannel434_g48 );
					float Wind_Tree_Mask114_g48 = pow( saferPower113_g48 , _WindTreeBaseRigidity );
					float3 temp_output_385_0_g48 = ( Wind_Global450_g48 * Wind_Tree_Mask114_g48 );
					float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( v.vertex ).xyz, 1 ) ).xyz;
					float temp_output_227_0_g48 = ( ( ase_positionWS.y * ( _WindLeafScale * 30.0 ) ) + _Time.y );
					float Global_Wind_Tree_Leaf_Speed495_g48 = ASPW_WindTreeLeafSpeed;
					float temp_output_223_0_g48 = ( _WindLeafSpeed * Global_Wind_Tree_Leaf_Speed495_g48 * 4.0 );
					float Global_Wind_Tree_Leaf_Amplitude497_g48 = ASPW_WindTreeLeafAmplitude;
					float temp_output_242_0_g48 = ( ( sin( ( temp_output_227_0_g48 * ( temp_output_223_0_g48 * 0.5 ) ) ) + sin( ( temp_output_227_0_g48 * ( temp_output_223_0_g48 * 1.25 ) ) ) + sin( ( temp_output_227_0_g48 * ( temp_output_223_0_g48 * 1.5 ) ) ) ) * ( ( _WindLeafAmplitude * Global_Wind_Tree_Leaf_Amplitude497_g48 ) * 0.1 ) );
					float temp_output_243_0_g48 = ( temp_output_242_0_g48 * 0.2 );
					float3 appendResult244_g48 = (float3(temp_output_243_0_g48 , temp_output_242_0_g48 , temp_output_243_0_g48));
					float3 Wind_Leaf245_g48 = appendResult244_g48;
					float3 break416_g48 = normalizeResult268_g48;
					float3 appendResult417_g48 = (float3(( break416_g48.x * -1.0 ) , ( break416_g48.z * -1.0 ) , 0.0));
					float3 Wind_Direction_Turbulence419_g48 = appendResult417_g48;
					float2 appendResult322_g48 = (float2(ase_positionWS.x , ase_positionWS.z));
					float Global_Wind_Tree_Leaf_Turbulence502_g48 = ASPW_WindTreeLeafTurbulence;
					float Wind_Leaf_Turbulence334_g48 = saturate( ( tex2Dlod( ASP_GlobalTintNoiseTexture, float4( ( ( ( Wind_Direction_Turbulence419_g48 * _Time.y ) * 0.1 ) + float3( ( appendResult322_g48 * 0.1 ) ,  0.0 ) ).xy, 0, 0.0) ).g + ( ( Global_Wind_Tree_Leaf_Turbulence502_g48 * _WindLeafTurbulence ) * 0.1 ) ) );
					float Wind_Mask_VColor_Blue429_g48 = v.ase_color.b;
					float3 Wind_Leaf_Mask285_g48 = ( ( ( Wind_Leaf245_g48 * Wind_Direction_Leaf85_g48 ) * Wind_Leaf_Turbulence334_g48 ) * Wind_Mask_VColor_Blue429_g48 );
					float Global_Wind_Tree_Leaf_Offset500_g48 = ASPW_WindTreeLeafOffset;
					float Wind_Mask_VColor_Green430_g48 = v.ase_color.g;
					float2 texCoord312_g48 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float3 Wind_Leaf_Offset314_g48 = ( ( ( ( Wind_Direction_Leaf85_g48 * ( _WindLeafOffset * Global_Wind_Tree_Leaf_Offset500_g48 * 0.5 ) ) + Wind_Leaf_Mask285_g48 ) * Wind_Mask_VColor_Green430_g48 ) * texCoord312_g48.y );
					float Global_Wind_Toggle504_g48 = ASPW_WindToggle;
					float3 lerpResult125_g48 = lerp( float3( 0,0,0 ) , ( temp_output_385_0_g48 + ( Wind_Leaf_Mask285_g48 + Wind_Leaf_Offset314_g48 ) ) , Global_Wind_Toggle504_g48);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch469_g48 = lerpResult125_g48;
					#else
					float3 staticSwitch469_g48 = temp_cast_0;
					#endif
					
					o.ase_texcoord5.xy = v.texcoord.xyzw.xy;
					o.ase_color = v.ase_color;
					o.ase_texcoord5.zw = v.texcoord2.xyzw.xy;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch469_g48;
					#ifdef ASE_ABSOLUTE_VERTEX_POS
						v.vertex.xyz = vertexValue;
					#else
						v.vertex.xyz += vertexValue;
					#endif
					v.vertex.w = 1;
					v.normal = v.normal;
					v.tangent = v.tangent;

					float3 positionWS = mul( unity_ObjectToWorld, v.vertex ).xyz;
					half3 normalWS = UnityObjectToWorldNormal( v.normal );
					half3 tangentWS = UnityObjectToWorldDir( v.tangent.xyz );

					o.pos = UnityObjectToClipPos( v.vertex );
					o.positionWS.xyz = positionWS;
					o.normalWS = normalWS;
					o.tangentWS = half4( tangentWS, v.tangent.w );

					UNITY_TRANSFER_LIGHTING(o, v.texcoord1.xy);
					#if defined( ASE_FOG )
						o.positionWS.w = o.pos.z;
					#endif

					#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
						o.tangentWS.zw = v.texcoord.xy;
						o.tangentWS.xy = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
					#endif
					return o;
				}

				#if defined(ASE_TESSELLATION)
				struct VertexControl
				{
					float4 vertex : INTERNALTESSPOS;
					half4 tangent : TANGENT;
					half3 normal : NORMAL;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					float4 ase_color : COLOR;

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct TessellationFactors
				{
					float edge[3] : SV_TessFactor;
					float inside : SV_InsideTessFactor;
				};

				VertexControl vert ( appdata v )
				{
					VertexControl o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					o.vertex = v.vertex;
					o.tangent = v.tangent;
					o.normal = v.normal;
					o.texcoord = v.texcoord;
					o.texcoord1 = v.texcoord1;
					o.texcoord2 = v.texcoord2;
					o.ase_color = v.ase_color;
					return o;
				}

				TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
				{
					TessellationFactors o;
					float4 tf = 1;
					float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
					float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
					#if defined(ASE_FIXED_TESSELLATION)
					tf = FixedTess( tessValue );
					#elif defined(ASE_DISTANCE_TESSELLATION)
					tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, UNITY_MATRIX_M, _WorldSpaceCameraPos );
					#elif defined(ASE_LENGTH_TESSELLATION)
					tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams );
					#elif defined(ASE_LENGTH_CULL_TESSELLATION)
					tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
					#endif
					o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
					return o;
				}

				[domain("tri")]
				[partitioning("fractional_odd")]
				[outputtopology("triangle_cw")]
				[patchconstantfunc("TessellationFunction")]
				[outputcontrolpoints(3)]
				VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
				{
				   return patch[id];
				}

				[domain("tri")]
				v2f DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
				{
					appdata o = (appdata) 0;
					o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
					o.tangent = patch[0].tangent * bary.x + patch[1].tangent * bary.y + patch[2].tangent * bary.z;
					o.normal = patch[0].normal * bary.x + patch[1].normal * bary.y + patch[2].normal * bary.z;
					o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
					o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
					o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
					o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
					#if defined(ASE_PHONG_TESSELLATION)
					float3 pp[3];
					for (int i = 0; i < 3; ++i)
						pp[i] = o.vertex.xyz - patch[i].normal * (dot(o.vertex.xyz, patch[i].normal) - dot(patch[i].vertex.xyz, patch[i].normal));
					float phongStrength = _TessPhongStrength;
					o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
					#endif
					UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
					return VertexFunction(o);
				}
				#else
				v2f vert ( appdata v )
				{
					return VertexFunction( v );
				}
				#endif

				half4 frag ( v2f IN , uint ase_vface : SV_IsFrontFace
					#if defined( ASE_DEPTH_WRITE_ON )
					, out float outputDepth : SV_Depth
					#endif
					) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(IN);

					#ifdef LOD_FADE_CROSSFADE
						UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
					#endif

					#if defined(ASE_LIGHTING_SIMPLE)
						SurfaceOutput o = (SurfaceOutput)0;
					#else
						#if defined(_SPECULAR_SETUP)
							SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
						#else
							SurfaceOutputStandard o = (SurfaceOutputStandard)0;
						#endif
					#endif

					half atten;
					{
						#if defined( ASE_RECEIVE_SHADOWS )
							UNITY_LIGHT_ATTENUATION( temp, IN, IN.positionWS.xyz )
							atten = temp;
						#else
							atten = 1;
						#endif
					}

					float3 PositionWS = IN.positionWS.xyz;
					half3 ViewDirWS = normalize( UnityWorldSpaceViewDir( PositionWS ) );
					float4 ScreenPosNorm = float4( IN.pos.xy * ( _ScreenParams.zw - 1.0 ), IN.pos.zw );
					float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, IN.pos.z ) * IN.pos.w;
					float4 ScreenPos = ComputeScreenPos( ClipPos );
					half3 NormalWS = IN.normalWS;
					half3 TangentWS = IN.tangentWS.xyz;
					half3 BitangentWS = cross( IN.normalWS, IN.tangentWS.xyz ) * IN.tangentWS.w * unity_WorldTransformParams.w;
					half3 LightAtten = atten;
					float FogCoord = IN.positionWS.w;

					#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
						float2 sampleCoords = (IN.tangentWS.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
						NormalWS = UnityObjectToWorldNormal(normalize(tex2D(_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
						TangentWS = -cross(unity_ObjectToWorld._13_23_33, NormalWS);
						BitangentWS = cross(NormalWS, -TangentWS);
					#endif

					float2 uv_BaseAlbedo1753 = IN.ase_texcoord5.xy;
					float4 tex2DNode1753 = tex2D( _BaseAlbedo, uv_BaseAlbedo1753 );
					float3 desaturateInitialColor1712 = tex2DNode1753.rgb;
					float desaturateDot1712 = dot( desaturateInitialColor1712, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar1712 = lerp( desaturateInitialColor1712, desaturateDot1712.xxx, _BaseAlbedoDesaturation );
					float3 blendOpSrc3324 = ( desaturateVar1712 * _BaseAlbedoBrightness );
					float3 blendOpDest3324 = _BaseAlbedoColor;
					float3 lerpResult1716 = lerp( _BaseAlbedoColor , ( saturate( (( blendOpDest3324 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest3324 ) * ( 1.0 - blendOpSrc3324 ) ) : ( 2.0 * blendOpDest3324 * blendOpSrc3324 ) ) )) , _BaseLerpBetweenColorandTexture);
					float lerpResult1727 = lerp( 1.0 , IN.ase_color.a , saturate( ( _BaseTreeAOIntensity * ASP_GlobalTreeAO ) ));
					float Base_AO1735 = lerpResult1727;
					float3 Base_Albedo1723 = ( lerpResult1716 * Base_AO1735 );
					float3 blendOpSrc1836 = Base_Albedo1723;
					float3 blendOpDest1836 = _GradientColor;
					float3 temp_output_1836_0 = ( saturate( (( blendOpDest1836 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest1836 ) * ( 1.0 - blendOpSrc1836 ) ) : ( 2.0 * blendOpDest1836 * blendOpSrc1836 ) ) ));
					float2 texCoord1840 = IN.ase_texcoord5.zw * float2( 1,1 ) + float2( 0,0 );
					float saferPower1879 = abs( ( ( 1.0 - texCoord1840.y ) * _GradientColorOffset ) );
					float temp_output_1848_0 = saturate( pow( saferPower1879 , _GradientColorContrast ) );
					float lerpResult1877 = lerp( temp_output_1848_0 , ( 1.0 - temp_output_1848_0 ) , _GradientColorInvertMask);
					float Gradient_Color_Mask1854 = ( lerpResult1877 * _GradientColorIntensity );
					float3 lerpResult1838 = lerp( Base_Albedo1723 , temp_output_1836_0 , Gradient_Color_Mask1854);
					float3 Gradient_Color_Albedo1856 = lerpResult1838;
					#ifdef _ENABLEGRADIENTCOLOR_ON
					float3 staticSwitch1867 = Gradient_Color_Albedo1856;
					#else
					float3 staticSwitch1867 = Base_Albedo1723;
					#endif
					float3 Step_2_Albedo1914 = staticSwitch1867;
					float3 blendOpSrc1897 = Base_Albedo1723;
					float3 blendOpDest1897 = _SecondColor;
					float3 temp_output_1897_0 = ( saturate( (( blendOpDest1897 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest1897 ) * ( 1.0 - blendOpSrc1897 ) ) : ( 2.0 * blendOpDest1897 * blendOpSrc1897 ) ) ));
					float saferPower1886 = abs( ( ( 1.0 - IN.ase_color.a ) * _SecondColorOffset ) );
					float temp_output_1889_0 = saturate( pow( saferPower1886 , ( _SecondColorContrast + 2.0 ) ) );
					float lerpResult1890 = lerp( temp_output_1889_0 , ( 1.0 - temp_output_1889_0 ) , _SecondColorInvertMask);
					float Second_Color_Mask1895 = ( lerpResult1890 * _SecondColorIntensity );
					float3 lerpResult1899 = lerp( Step_2_Albedo1914 , temp_output_1897_0 , Second_Color_Mask1895);
					float3 Second_Color_Albedo1902 = lerpResult1899;
					#ifdef _ENABLESECONDCOLOR_ON
					float3 staticSwitch1921 = Second_Color_Albedo1902;
					#else
					float3 staticSwitch1921 = Step_2_Albedo1914;
					#endif
					float3 Step_3_Albedo1954 = staticSwitch1921;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Tint_Color_Mask1949 = saturate( round( ( frac( ( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) * ( _TintNoiseOffset * ASP_GlobalTintNoiseUVScale ) ) ) * ( _TintNoiseContrast * ASP_GlobalTintNoiseContrast ) ) ) );
					float3 lerpResult1951 = lerp( _TintColor1.rgb , _TintColor2.rgb , Tint_Color_Mask1949);
					float3 blendOpSrc1952 = Step_3_Albedo1954;
					float3 blendOpDest1952 = lerpResult1951;
					float3 temp_output_1952_0 = ( saturate( (( blendOpDest1952 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest1952 ) * ( 1.0 - blendOpSrc1952 ) ) : ( 2.0 * blendOpDest1952 * blendOpSrc1952 ) ) ));
					float temp_output_1961_0 = ( _TintNoiseIntensity * ASP_GlobalTintNoiseIntensity );
					float3 lerpResult1956 = lerp( Step_3_Albedo1954 , temp_output_1952_0 , temp_output_1961_0);
					float3 Tint_Color_Albedo1963 = lerpResult1956;
					#ifdef _ENABLETINTCOLOR_ON
					float3 staticSwitch2080 = Tint_Color_Albedo1963;
					#else
					float3 staticSwitch2080 = Step_3_Albedo1954;
					#endif
					float3 Step_4_Albedo2083 = staticSwitch2080;
					float3 Top_Layer_Color1995 = _TopLayerColor;
					float2 uv_BaseNormal1741 = IN.ase_texcoord5.xy;
					float3 tex2DNode1741 = UnpackScaleNormal( tex2D( _BaseNormal, uv_BaseNormal1741 ), _BaseNormalIntensity );
					float3 break105_g45 = tex2DNode1741;
					float switchResult107_g45 = (((ase_vface>0)?(break105_g45.z):(-break105_g45.z)));
					float3 appendResult108_g45 = (float3(break105_g45.x , break105_g45.y , switchResult107_g45));
					#ifdef _ENABLEFLIPNORMALS_ON
					float3 staticSwitch1744 = appendResult108_g45;
					#else
					float3 staticSwitch1744 = tex2DNode1741;
					#endif
					float3 Base_Normal1747 = staticSwitch1744;
					float3 desaturateInitialColor2013 = Base_Normal1747;
					float desaturateDot2013 = dot( desaturateInitialColor2013, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar2013 = lerp( desaturateInitialColor2013, desaturateDot2013.xxx, 1.0 );
					float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
					float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
					float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
					float3 tanNormal2012 = Base_Normal1747;
					float3 worldNormal2012 = float3( dot( tanToWorld0, tanNormal2012 ), dot( tanToWorld1, tanNormal2012 ), dot( tanToWorld2, tanNormal2012 ) );
					float3 temp_cast_0 = (worldNormal2012.y).xxx;
					#ifdef _ENABLEWORLDPROJECTION_ON
					float3 staticSwitch2017 = temp_cast_0;
					#else
					float3 staticSwitch2017 = desaturateVar2013;
					#endif
					float3 saferPower17_g46 = abs( abs( ( saturate( staticSwitch2017 ) + ( _TopLayerOffset * ASPT_TopLayerOffset ) ) ) );
					float temp_output_2005_0 = ( _TopLayerContrast * ASPT_TopLayerContrast );
					float3 temp_cast_1 = (temp_output_2005_0).xxx;
					float saferPower2024 = abs( ( 1.0 - IN.ase_color.a ) );
					float Top_Layer_Contrast2025 = temp_output_2005_0;
					float3 lerpResult2029 = lerp( ( ( saturate( pow( saferPower17_g46 , temp_cast_1 ) ) * ( _TopLayerIntensity * ASPT_TopLayerIntensity ) ) * saturate(  (0.0 + ( PositionWS.y - ASPT_TopLayerHeightStart ) * ( 1.0 - 0.0 ) / ( ( ASPT_TopLayerHeightStart + ASPT_TopLayerHeightFade ) - ASPT_TopLayerHeightStart ) ) ) ) , float3( 0,0,0 ) , saturate( ( pow( saferPower2024 , Top_Layer_Contrast2025 ) * _TopLayerAOMask ) ));
					float3 Top_Layer_Mask2031 = saturate( lerpResult2029 );
					float3 lerpResult2034 = lerp( Step_4_Albedo2083 , Top_Layer_Color1995 , Top_Layer_Mask2031);
					float3 lerpResult2037 = lerp( Step_4_Albedo2083 , lerpResult2034 , saturate( ( ase_vface > 0 ? +1 : -1 ) ));
					#ifdef _ENABLEBACKFACEPROJECTION_ON
					float3 staticSwitch2041 = lerpResult2034;
					#else
					float3 staticSwitch2041 = lerpResult2037;
					#endif
					float3 Top_Layer_Albedo2042 = staticSwitch2041;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch2043 = Top_Layer_Albedo2042;
					#else
					float3 staticSwitch2043 = Step_4_Albedo2083;
					#endif
					float3 Output_Albedo1873 = staticSwitch2043;
					
					float2 uv_BaseSSE1757 = IN.ase_texcoord5.xy;
					float4 tex2DNode1757 = tex2D( _BaseSSE, uv_BaseSSE1757 );
					float Texture_Smoothness3683 = tex2DNode1757.r;
					float lerpResult3679 = lerp( _BaseSmoothnessMin , _BaseSmoothnessMax , Texture_Smoothness3683);
					float Base_Smoothness1760 = saturate( lerpResult3679 );
					float lerpResult2064 = lerp( Base_Smoothness1760 , _TopLayerSmoothnessIntensity , Top_Layer_Mask2031.x);
					float lerpResult2069 = lerp( Base_Smoothness1760 , lerpResult2064 , saturate( ( ase_vface > 0 ? +1 : -1 ) ));
					#ifdef _ENABLEBACKFACEPROJECTION_ON
					float staticSwitch2072 = lerpResult2064;
					#else
					float staticSwitch2072 = lerpResult2069;
					#endif
					float Top_Layer_Smoothness2073 = saturate( staticSwitch2072 );
					#ifdef _ENABLETOPLAYERBLEND_ON
					float staticSwitch2074 = Top_Layer_Smoothness2073;
					#else
					float staticSwitch2074 = Base_Smoothness1760;
					#endif
					float Output_Smoothness2077 = staticSwitch2074;
					
					float RSE_Emissive_Mask1762 = tex2DNode1757.b;
					float saferPower1804 = abs( RSE_Emissive_Mask1762 );
					float3 temp_output_1806_0 = ( ( _BaseEmissiveColor.rgb * pow( saferPower1804 , _BaseEmissiveMaskContrast ) ) * _BaseEmissiveIntensity );
					float3 lerpResult1808 = lerp( temp_output_1806_0 , ( temp_output_1806_0 * IN.ase_color.a ) , _BaseEmissiveAOMask);
					float3 Base_Emissive1812 = lerpResult1808;
					float3 lerpResult3341 = lerp( Base_Emissive1812 , float3( 0,0,0 ) , Top_Layer_Mask2031);
					float3 Top_Layer_Emissive3342 = lerpResult3341;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch3343 = Top_Layer_Emissive3342;
					#else
					float3 staticSwitch3343 = Base_Emissive1812;
					#endif
					float3 Output_Emissive3346 = staticSwitch3343;
					
					float Opacity_Texture1737 = tex2DNode1753.a;
					float3 normalizeResult1780 = normalize( cross( ddx( PositionWS ) , ddy( PositionWS ) ) );
					float3 normalizeResult1787 = normalize( ( _WorldSpaceCameraPos - PositionWS ) );
					float dotResult1782 = dot( normalizeResult1780 , normalizeResult1787 );
					float lerpResult1794 = lerp( 1.0 , abs( dotResult1782 ) , _BaseGlancingAngleCut);
					#ifdef _ENABLEGLANCINGANGLECUT_ON
					float staticSwitch1788 = lerpResult1794;
					#else
					float staticSwitch1788 = 1.0;
					#endif
					float Base_Glancing_Angle1790 = staticSwitch1788;
					float Base_Opacity1739 = ( Opacity_Texture1737 * Base_Glancing_Angle1790 );
					
					float Local_SSS_Intensity3538 = _BaseSSSIntensity;
					float RSE_Subsurface_Mask1761 = tex2DNode1757.g;
					float lerpResult1726 = lerp( 1.0 , IN.ase_color.a , saturate( ( _BaseSSSAOInfluence * ASP_GlobalTreeSSSAOInfluence ) ));
					float Base_SSS_AO_Influence1734 = lerpResult1726;
					float clampResult3460 = clamp( ( 1.0 -  (0.0 + ( distance( PositionWS , _WorldSpaceCameraPos ) - 0.0 ) * ( 1.0 - 0.0 ) / ( ASP_GlobalTreeSSSDistance - 0.0 ) ) ) , 0.0 , 1.0 );
					float Base_SSS_Max_Distance3461 = clampResult3460;
					float3 Base_SSS1769 = ( ( ( ( ( Local_SSS_Intensity3538 * ASP_GlobalTreeSSSIntensity * 2.0 ) * RSE_Subsurface_Mask1761 ) * Base_SSS_AO_Influence1734 ) * Base_SSS_Max_Distance3461 ) * _BaseSSSColor );
					float3 lerpResult1859 = lerp( ( Base_Albedo1723 * Base_SSS1769 ) , ( temp_output_1836_0 * Base_SSS1769 ) , Gradient_Color_Mask1854);
					float3 Gradient_Color_SSS1864 = lerpResult1859;
					#ifdef _ENABLEGRADIENTCOLOR_ON
					float3 staticSwitch1869 = Gradient_Color_SSS1864;
					#else
					float3 staticSwitch1869 = ( Base_Albedo1723 * Base_SSS1769 );
					#endif
					float3 Step_2_SSS1915 = staticSwitch1869;
					float3 lerpResult1908 = lerp( Step_2_SSS1915 , ( temp_output_1897_0 * Base_SSS1769 ) , Second_Color_Mask1895);
					float3 Second_Color_SSS1910 = lerpResult1908;
					#ifdef _ENABLESECONDCOLOR_ON
					float3 staticSwitch1922 = Second_Color_SSS1910;
					#else
					float3 staticSwitch1922 = Step_2_SSS1915;
					#endif
					float3 Step_3_SSS1955 = staticSwitch1922;
					float3 lerpResult1966 = lerp( Step_3_SSS1955 , ( temp_output_1952_0 * Base_SSS1769 ) , temp_output_1961_0);
					float3 Tint_Color_SSS1968 = lerpResult1966;
					#ifdef _ENABLETINTCOLOR_ON
					float3 staticSwitch2081 = Tint_Color_SSS1968;
					#else
					float3 staticSwitch2081 = Step_3_SSS1955;
					#endif
					float3 Step_4_SSS2084 = staticSwitch2081;
					float Global_SSS_Intensity2050 = ASP_GlobalTreeSSSIntensity;
					float3 lerpResult2054 = lerp( Step_4_SSS2084 , ( ( Top_Layer_Color1995 * _TopLayerSSSIntensity * Global_SSS_Intensity2050 ) * _TopLayerSSSColor ) , Top_Layer_Mask2031);
					float3 lerpResult2059 = lerp( Step_4_SSS2084 , lerpResult2054 , saturate( ( ase_vface > 0 ? +1 : -1 ) ));
					#ifdef _ENABLEBACKFACEPROJECTION_ON
					float3 staticSwitch2060 = lerpResult2054;
					#else
					float3 staticSwitch2060 = lerpResult2059;
					#endif
					float3 Top_Layer_SSS2061 = staticSwitch2060;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch2045 = Top_Layer_SSS2061;
					#else
					float3 staticSwitch2045 = Step_4_SSS2084;
					#endif
					float3 Output_SSS1874 = staticSwitch2045;
					

					o.Albedo = Output_Albedo1873;
					o.Normal = Base_Normal1747;

					half3 Specular = half3( 0, 0, 0 );
					half Metallic = 0;
					half Smoothness = Output_Smoothness2077;
					half Occlusion = Base_AO1735;

					#if defined(ASE_LIGHTING_SIMPLE)
						o.Specular = Specular.x;
						o.Gloss = Smoothness;
					#else
						#if defined(_SPECULAR_SETUP)
							o.Specular = Specular;
						#else
							o.Metallic = Metallic;
						#endif
						o.Occlusion = Occlusion;
						o.Smoothness = Smoothness;
					#endif

					o.Emission = Output_Emissive3346;
					o.Alpha = Base_Opacity1739;
					half AlphaClipThreshold = _CutOff;
					half3 Transmission = 1;
					half3 Translucency = Output_SSS1874;

					#if defined( ASE_DEPTH_WRITE_ON )
						float DeviceDepth = IN.pos.z;
					#endif

					#ifdef _ALPHATEST_ON
						clip( o.Alpha - AlphaClipThreshold );
					#endif

					#if defined( ASE_CHANGES_WORLD_POS )
					{
						#if defined( ASE_RECEIVE_SHADOWS )
							UNITY_LIGHT_ATTENUATION( temp, IN, PositionWS )
							LightAtten = temp;
						#else
							LightAtten = 1;
						#endif
					}
					#endif

					#if ( ASE_FRAGMENT_NORMAL == 0 )
						o.Normal = normalize( o.Normal.x * TangentWS + o.Normal.y * BitangentWS + o.Normal.z * NormalWS );
					#elif ( ASE_FRAGMENT_NORMAL == 1 )
						o.Normal = UnityObjectToWorldNormal( o.Normal );
					#elif ( ASE_FRAGMENT_NORMAL == 2 )
						// @diogo: already in world-space; do nothing
					#endif

					#if defined( ASE_DEPTH_WRITE_ON )
						outputDepth = DeviceDepth;
					#endif

					#ifndef USING_DIRECTIONAL_LIGHT
						half3 lightDir = normalize( UnityWorldSpaceLightDir( PositionWS ) );
					#else
						half3 lightDir = _WorldSpaceLightPos0.xyz;
					#endif

					UnityGI gi;
					UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
					gi.indirect.diffuse = 0;
					gi.indirect.specular = 0;
					gi.light.color = _LightColor0.rgb;
					gi.light.dir = lightDir;
					gi.light.color *= atten;

					half4 c = 0;
					#if defined(ASE_LIGHTING_SIMPLE)
						#if defined(_SPECULAR_SETUP)
							c += LightingBlinnPhong (o, ViewDirWS, gi);
						#else
							c += LightingLambert( o, gi );
						#endif
					#else
						#if defined(_SPECULAR_SETUP)
							c += LightingStandardSpecular(o, ViewDirWS, gi);
						#else
							c += LightingStandard(o, ViewDirWS, gi);
						#endif
					#endif

					#ifdef ASE_TRANSMISSION
					{
						half shadow = _TransmissionShadow;
						#ifdef DIRECTIONAL
							half3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, shadow );
						#else
							half3 lightAtten = gi.light.color;
						#endif
						half3 transmission = max(0 , -dot(o.Normal, gi.light.dir)) * lightAtten * Transmission;
						c.rgb += o.Albedo * transmission;
					}
					#endif

					#ifdef ASE_TRANSLUCENCY
					{
						half shadow = _BaseSSSShadow;
						half normal = _BaseSSSNormalDistortion;
						half scattering = _BaseSSSScattering;
						half direct = _BaseSSSDirect;
						half ambient = _BaseSSSAmbiet;
						half strength = _BaseSSSIntensity;

						#ifdef DIRECTIONAL
							half3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, shadow );
						#else
							half3 lightAtten = gi.light.color;
						#endif
						half3 lightDir = gi.light.dir + o.Normal * normal;
						half transVdotL = pow( saturate( dot( ViewDirWS, -lightDir ) ), scattering );
						half3 translucency = lightAtten * (transVdotL * direct + gi.indirect.diffuse * ambient) * Translucency;
						c.rgb += o.Albedo * translucency * strength;
					}
					#endif

					#if defined( ASE_FOG )
						UNITY_APPLY_FOG( FogCoord, c );
					#endif
					return c;
				}
			ENDCG
		}

		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }
			Cull Off

			CGPROGRAM
				#define ASE_GEOMETRY 1
				#define ASE_FRAGMENT_NORMAL 0
				#define ASE_RECEIVE_SHADOWS
				#pragma multi_compile_instancing
				#define ASE_FOG
				#define ASE_TRANSLUCENCY 1
				#pragma multi_compile _ LOD_FADE_CROSSFADE
				#pragma multi_compile_local _ALPHATEST_ON
				#define ASE_VERSION 19903

				#pragma vertex vert
				#pragma fragment frag
				#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
				#pragma shader_feature EDITOR_VISUALIZATION
				#ifndef UNITY_PASS_META
					#define UNITY_PASS_META
				#endif
				#include "HLSLSupport.cginc"
				#ifdef ASE_GEOMETRY
					#ifndef UNITY_INSTANCED_LOD_FADE
						#define UNITY_INSTANCED_LOD_FADE
					#endif
					#ifndef UNITY_INSTANCED_SH
						#define UNITY_INSTANCED_SH
					#endif
					#ifndef UNITY_INSTANCED_LIGHTMAPSTS
						#define UNITY_INSTANCED_LIGHTMAPSTS
					#endif
				#endif
				#include "UnityShaderVariables.cginc"
				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "UnityPBSLighting.cginc"
				#include "UnityMetaPass.cginc"

				#include "UnityStandardUtils.cginc"
				#define ASE_NEEDS_VERT_POSITION
				#define ASE_NEEDS_TEXTURE_COORDINATES2
				#define ASE_NEEDS_VERT_TEXTURE_COORDINATES2
				#define ASE_NEEDS_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_COLOR
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
				#define ASE_NEEDS_VERT_TANGENT
				#define ASE_NEEDS_VERT_NORMAL
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLESTATICMESHSUPPORT_ON
				#pragma shader_feature_local _ENABLETOPLAYERBLEND_ON
				#pragma shader_feature_local _ENABLETINTCOLOR_ON
				#pragma shader_feature_local _ENABLESECONDCOLOR_ON
				#pragma shader_feature_local _ENABLEGRADIENTCOLOR_ON
				#pragma shader_feature_local _ENABLEBACKFACEPROJECTION_ON
				#pragma shader_feature_local _ENABLEWORLDPROJECTION_ON
				#pragma shader_feature_local _ENABLEFLIPNORMALS_ON
				#pragma shader_feature_local _ENABLEGLANCINGANGLECUT_ON


				struct appdata
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					half4 tangent : TANGENT;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					float4 ase_color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					#ifdef EDITOR_VISUALIZATION
						float2 vizUV : TEXCOORD0;
						float4 lightCoord : TEXCOORD1;
					#endif
					float4 ase_texcoord2 : TEXCOORD2;
					float4 ase_color : COLOR;
					float4 ase_texcoord3 : TEXCOORD3;
					float4 ase_texcoord4 : TEXCOORD4;
					float4 ase_texcoord5 : TEXCOORD5;
					float4 ase_texcoord6 : TEXCOORD6;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				#ifdef ASE_TESSELLATION
					float _TessPhongStrength;
					float _TessValue;
					float _TessMin;
					float _TessMax;
					float _TessEdgeLength;
					float _TessMaxDisp;
				#endif

				uniform float _BaseSSSDirect;
				uniform float _BaseSSSAmbiet;
				uniform float _BaseSSSShadow;
				uniform float _BaseSSSNormalDistortion;
				uniform float _BaseSSSScattering;
				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform half ASPW_WindTreeSpeed;
				uniform half ASPW_WindTreeAmplitude;
				uniform float _WindTreeFlexibility;
				uniform half ASPW_WindTreeFlexibility;
				uniform float _WindTreeBaseRigidity;
				uniform float _WindLeafScale;
				uniform float _WindLeafSpeed;
				uniform half ASPW_WindTreeLeafSpeed;
				uniform float _WindLeafAmplitude;
				uniform half ASPW_WindTreeLeafAmplitude;
				uniform half ASPW_WindTreeLeafTurbulence;
				uniform float _WindLeafTurbulence;
				uniform float _WindLeafOffset;
				uniform half ASPW_WindTreeLeafOffset;
				uniform float ASPW_WindToggle;
				uniform half3 _BaseAlbedoColor;
				uniform sampler2D _BaseAlbedo;
				uniform float _BaseAlbedoDesaturation;
				uniform float _BaseAlbedoBrightness;
				uniform float _BaseLerpBetweenColorandTexture;
				uniform float _BaseTreeAOIntensity;
				uniform half ASP_GlobalTreeAO;
				uniform float3 _GradientColor;
				uniform float _GradientColorOffset;
				uniform float _GradientColorContrast;
				uniform float _GradientColorInvertMask;
				uniform float _GradientColorIntensity;
				uniform float3 _SecondColor;
				uniform float _SecondColorOffset;
				uniform float _SecondColorContrast;
				uniform float _SecondColorInvertMask;
				uniform float _SecondColorIntensity;
				uniform float4 _TintColor1;
				uniform float4 _TintColor2;
				uniform float _TintNoiseOffset;
				uniform half ASP_GlobalTintNoiseUVScale;
				uniform float _TintNoiseContrast;
				uniform half ASP_GlobalTintNoiseContrast;
				uniform float _TintNoiseIntensity;
				uniform half ASP_GlobalTintNoiseIntensity;
				uniform float3 _TopLayerColor;
				uniform sampler2D _BaseNormal;
				uniform float _BaseNormalIntensity;
				uniform half _TopLayerOffset;
				uniform half ASPT_TopLayerOffset;
				uniform half _TopLayerContrast;
				uniform half ASPT_TopLayerContrast;
				uniform half _TopLayerIntensity;
				uniform half ASPT_TopLayerIntensity;
				uniform half ASPT_TopLayerHeightStart;
				uniform float ASPT_TopLayerHeightFade;
				uniform half _TopLayerAOMask;
				uniform float4 _BaseEmissiveColor;
				uniform sampler2D _BaseSSE;
				uniform float _BaseEmissiveMaskContrast;
				uniform float _BaseEmissiveIntensity;
				uniform float _BaseEmissiveAOMask;
				uniform float _BaseGlancingAngleCut;
				uniform float _CutOff;


				float3 ASESafeNormalize(float3 inVec)
				{
					float dp3 = max(1.175494351e-38, dot(inVec, inVec));
					return inVec* rsqrt(dp3);
				}
				
				float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
				{
					original -= center;
					float C = cos( angle );
					float S = sin( angle );
					float t = 1 - C;
					float m00 = t * u.x * u.x + C;
					float m01 = t * u.x * u.y - S * u.z;
					float m02 = t * u.x * u.z + S * u.y;
					float m10 = t * u.x * u.y + S * u.z;
					float m11 = t * u.y * u.y + C;
					float m12 = t * u.y * u.z - S * u.x;
					float m20 = t * u.x * u.z - S * u.y;
					float m21 = t * u.y * u.z + S * u.x;
					float m22 = t * u.z * u.z + C;
					float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
					return mul( finalMatrix, original ) + center;
				}
				

				v2f VertexFunction( appdata v  )
				{
					UNITY_SETUP_INSTANCE_ID(v);
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f,o);
					UNITY_TRANSFER_INSTANCE_ID(v,o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					float3 temp_cast_0 = (0.0).xxx;
					float3 Global_Wind_Direction493_g48 = ASPW_WindDirection;
					float3 worldToObjDir269_g48 = mul( unity_WorldToObject, float4( Global_Wind_Direction493_g48, 0.0 ) ).xyz;
					float3 normalizeResult268_g48 = ASESafeNormalize( worldToObjDir269_g48 );
					float3 Wind_Direction_Leaf85_g48 = normalizeResult268_g48;
					float3 break86_g48 = Wind_Direction_Leaf85_g48;
					float3 appendResult89_g48 = (float3(break86_g48.z , 0.0 , ( break86_g48.x * -1.0 )));
					float3 Wind_Direction91_g48 = appendResult89_g48;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Tree_Randomization149_g48 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_56_0_g48 = ( Wind_Tree_Randomization149_g48 + _Time.y );
					float Global_Wind_Tree_Speed491_g48 = ASPW_WindTreeSpeed;
					float temp_output_213_0_g48 = ( Global_Wind_Tree_Speed491_g48 * 7.0 );
					float Global_Wind_Tree_Amplitude464_g48 = ASPW_WindTreeAmplitude;
					float Global_Wind_Tree_Flexibility490_g48 = ASPW_WindTreeFlexibility;
					float temp_output_383_0_g48 = ( ( _WindTreeFlexibility * Global_Wind_Tree_Flexibility490_g48 ) * 0.3 );
					float Wind_Tree_257_g48 = ( ( ( ( ( sin( ( temp_output_56_0_g48 * ( temp_output_213_0_g48 * 0.1 ) ) ) + sin( ( temp_output_56_0_g48 * ( temp_output_213_0_g48 * 0.3 ) ) ) + sin( ( temp_output_56_0_g48 * ( temp_output_213_0_g48 * 0.5 ) ) ) ) + Global_Wind_Tree_Amplitude464_g48 ) * Global_Wind_Tree_Amplitude464_g48 ) * 0.01 ) * temp_output_383_0_g48 );
					float3 rotatedValue99_g48 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction91_g48, Wind_Tree_257_g48 );
					float3 Rotate_About_Axis354_g48 = ( rotatedValue99_g48 - v.vertex.xyz );
					float3 break452_g48 = normalizeResult268_g48;
					float3 appendResult454_g48 = (float3(break452_g48.x , 0.0 , break452_g48.z));
					float3 Wind_Direction_SM_Support453_g48 = appendResult454_g48;
					float Wind_Tree_Flexibility483_g48 = temp_output_383_0_g48;
					#ifdef _ENABLESTATICMESHSUPPORT_ON
					float3 staticSwitch482_g48 = ( Global_Wind_Tree_Amplitude464_g48 * Wind_Tree_257_g48 * Wind_Direction_SM_Support453_g48 * Wind_Tree_Flexibility483_g48 );
					#else
					float3 staticSwitch482_g48 = Rotate_About_Axis354_g48;
					#endif
					float3 Wind_Global450_g48 = staticSwitch482_g48;
					float2 texCoord433_g48 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float Wind_Mask_UV_CH2_VChannel434_g48 = texCoord433_g48.y;
					float saferPower113_g48 = abs( Wind_Mask_UV_CH2_VChannel434_g48 );
					float Wind_Tree_Mask114_g48 = pow( saferPower113_g48 , _WindTreeBaseRigidity );
					float3 temp_output_385_0_g48 = ( Wind_Global450_g48 * Wind_Tree_Mask114_g48 );
					float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( v.vertex ).xyz, 1 ) ).xyz;
					float temp_output_227_0_g48 = ( ( ase_positionWS.y * ( _WindLeafScale * 30.0 ) ) + _Time.y );
					float Global_Wind_Tree_Leaf_Speed495_g48 = ASPW_WindTreeLeafSpeed;
					float temp_output_223_0_g48 = ( _WindLeafSpeed * Global_Wind_Tree_Leaf_Speed495_g48 * 4.0 );
					float Global_Wind_Tree_Leaf_Amplitude497_g48 = ASPW_WindTreeLeafAmplitude;
					float temp_output_242_0_g48 = ( ( sin( ( temp_output_227_0_g48 * ( temp_output_223_0_g48 * 0.5 ) ) ) + sin( ( temp_output_227_0_g48 * ( temp_output_223_0_g48 * 1.25 ) ) ) + sin( ( temp_output_227_0_g48 * ( temp_output_223_0_g48 * 1.5 ) ) ) ) * ( ( _WindLeafAmplitude * Global_Wind_Tree_Leaf_Amplitude497_g48 ) * 0.1 ) );
					float temp_output_243_0_g48 = ( temp_output_242_0_g48 * 0.2 );
					float3 appendResult244_g48 = (float3(temp_output_243_0_g48 , temp_output_242_0_g48 , temp_output_243_0_g48));
					float3 Wind_Leaf245_g48 = appendResult244_g48;
					float3 break416_g48 = normalizeResult268_g48;
					float3 appendResult417_g48 = (float3(( break416_g48.x * -1.0 ) , ( break416_g48.z * -1.0 ) , 0.0));
					float3 Wind_Direction_Turbulence419_g48 = appendResult417_g48;
					float2 appendResult322_g48 = (float2(ase_positionWS.x , ase_positionWS.z));
					float Global_Wind_Tree_Leaf_Turbulence502_g48 = ASPW_WindTreeLeafTurbulence;
					float Wind_Leaf_Turbulence334_g48 = saturate( ( tex2Dlod( ASP_GlobalTintNoiseTexture, float4( ( ( ( Wind_Direction_Turbulence419_g48 * _Time.y ) * 0.1 ) + float3( ( appendResult322_g48 * 0.1 ) ,  0.0 ) ).xy, 0, 0.0) ).g + ( ( Global_Wind_Tree_Leaf_Turbulence502_g48 * _WindLeafTurbulence ) * 0.1 ) ) );
					float Wind_Mask_VColor_Blue429_g48 = v.ase_color.b;
					float3 Wind_Leaf_Mask285_g48 = ( ( ( Wind_Leaf245_g48 * Wind_Direction_Leaf85_g48 ) * Wind_Leaf_Turbulence334_g48 ) * Wind_Mask_VColor_Blue429_g48 );
					float Global_Wind_Tree_Leaf_Offset500_g48 = ASPW_WindTreeLeafOffset;
					float Wind_Mask_VColor_Green430_g48 = v.ase_color.g;
					float2 texCoord312_g48 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float3 Wind_Leaf_Offset314_g48 = ( ( ( ( Wind_Direction_Leaf85_g48 * ( _WindLeafOffset * Global_Wind_Tree_Leaf_Offset500_g48 * 0.5 ) ) + Wind_Leaf_Mask285_g48 ) * Wind_Mask_VColor_Green430_g48 ) * texCoord312_g48.y );
					float Global_Wind_Toggle504_g48 = ASPW_WindToggle;
					float3 lerpResult125_g48 = lerp( float3( 0,0,0 ) , ( temp_output_385_0_g48 + ( Wind_Leaf_Mask285_g48 + Wind_Leaf_Offset314_g48 ) ) , Global_Wind_Toggle504_g48);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch469_g48 = lerpResult125_g48;
					#else
					float3 staticSwitch469_g48 = temp_cast_0;
					#endif
					
					float3 ase_tangentWS = UnityObjectToWorldDir( v.tangent );
					o.ase_texcoord3.xyz = ase_tangentWS;
					float3 ase_normalWS = UnityObjectToWorldNormal( v.normal );
					o.ase_texcoord4.xyz = ase_normalWS;
					float ase_tangentSign = v.tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
					float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
					o.ase_texcoord5.xyz = ase_bitangentWS;
					o.ase_texcoord6.xyz = ase_positionWS;
					
					o.ase_texcoord2.xy = v.texcoord.xyzw.xy;
					o.ase_color = v.ase_color;
					o.ase_texcoord2.zw = v.texcoord2.xyzw.xy;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;
					o.ase_texcoord4.w = 0;
					o.ase_texcoord5.w = 0;
					o.ase_texcoord6.w = 0;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch469_g48;
					#ifdef ASE_ABSOLUTE_VERTEX_POS
						v.vertex.xyz = vertexValue;
					#else
						v.vertex.xyz += vertexValue;
					#endif
					v.vertex.w = 1;
					v.normal = v.normal;
					v.tangent = v.tangent;

					#ifdef EDITOR_VISUALIZATION
						o.vizUV = 0;
						o.lightCoord = 0;
						if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
							o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
						else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
						{
							o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
							o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
						}
					#endif

					o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
					return o;
				}

				#if defined(ASE_TESSELLATION)
				struct VertexControl
				{
					float4 vertex : INTERNALTESSPOS;
					float4 tangent : TANGENT;
					float3 normal : NORMAL;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					float4 ase_color : COLOR;

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct TessellationFactors
				{
					float edge[3] : SV_TessFactor;
					float inside : SV_InsideTessFactor;
				};

				VertexControl vert ( appdata v )
				{
					VertexControl o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					o.vertex = v.vertex;
					o.tangent = v.tangent;
					o.normal = v.normal;
					o.texcoord1 = v.texcoord1;
					o.texcoord2 = v.texcoord2;
					o.ase_color = v.ase_color;
					return o;
				}

				TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
				{
					TessellationFactors o;
					float4 tf = 1;
					float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
					float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
					#if defined(ASE_FIXED_TESSELLATION)
					tf = FixedTess( tessValue );
					#elif defined(ASE_DISTANCE_TESSELLATION)
					tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, UNITY_MATRIX_M, _WorldSpaceCameraPos );
					#elif defined(ASE_LENGTH_TESSELLATION)
					tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams );
					#elif defined(ASE_LENGTH_CULL_TESSELLATION)
					tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
					#endif
					o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
					return o;
				}

				[domain("tri")]
				[partitioning("fractional_odd")]
				[outputtopology("triangle_cw")]
				[patchconstantfunc("TessellationFunction")]
				[outputcontrolpoints(3)]
				VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
				{
				   return patch[id];
				}

				[domain("tri")]
				v2f DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
				{
					appdata o = (appdata) 0;
					o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
					o.tangent = patch[0].tangent * bary.x + patch[1].tangent * bary.y + patch[2].tangent * bary.z;
					o.normal = patch[0].normal * bary.x + patch[1].normal * bary.y + patch[2].normal * bary.z;
					o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
					o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
					o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
					#if defined(ASE_PHONG_TESSELLATION)
					float3 pp[3];
					for (int i = 0; i < 3; ++i)
						pp[i] = o.vertex.xyz - patch[i].normal * (dot(o.vertex.xyz, patch[i].normal) - dot(patch[i].vertex.xyz, patch[i].normal));
					float phongStrength = _TessPhongStrength;
					o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
					#endif
					UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
					return VertexFunction(o);
				}
				#else
				v2f vert( appdata v )
				{
					return VertexFunction( v );
				}
				#endif

				half4 frag( v2f IN , uint ase_vface : SV_IsFrontFace ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(IN);

					#ifdef LOD_FADE_CROSSFADE
						UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
					#endif

					#if defined(ASE_LIGHTING_SIMPLE)
						SurfaceOutput o = (SurfaceOutput)0;
					#else
						#if defined(_SPECULAR_SETUP)
							SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
						#else
							SurfaceOutputStandard o = (SurfaceOutputStandard)0;
						#endif
					#endif

					float2 uv_BaseAlbedo1753 = IN.ase_texcoord2.xy;
					float4 tex2DNode1753 = tex2D( _BaseAlbedo, uv_BaseAlbedo1753 );
					float3 desaturateInitialColor1712 = tex2DNode1753.rgb;
					float desaturateDot1712 = dot( desaturateInitialColor1712, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar1712 = lerp( desaturateInitialColor1712, desaturateDot1712.xxx, _BaseAlbedoDesaturation );
					float3 blendOpSrc3324 = ( desaturateVar1712 * _BaseAlbedoBrightness );
					float3 blendOpDest3324 = _BaseAlbedoColor;
					float3 lerpResult1716 = lerp( _BaseAlbedoColor , ( saturate( (( blendOpDest3324 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest3324 ) * ( 1.0 - blendOpSrc3324 ) ) : ( 2.0 * blendOpDest3324 * blendOpSrc3324 ) ) )) , _BaseLerpBetweenColorandTexture);
					float lerpResult1727 = lerp( 1.0 , IN.ase_color.a , saturate( ( _BaseTreeAOIntensity * ASP_GlobalTreeAO ) ));
					float Base_AO1735 = lerpResult1727;
					float3 Base_Albedo1723 = ( lerpResult1716 * Base_AO1735 );
					float3 blendOpSrc1836 = Base_Albedo1723;
					float3 blendOpDest1836 = _GradientColor;
					float3 temp_output_1836_0 = ( saturate( (( blendOpDest1836 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest1836 ) * ( 1.0 - blendOpSrc1836 ) ) : ( 2.0 * blendOpDest1836 * blendOpSrc1836 ) ) ));
					float2 texCoord1840 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
					float saferPower1879 = abs( ( ( 1.0 - texCoord1840.y ) * _GradientColorOffset ) );
					float temp_output_1848_0 = saturate( pow( saferPower1879 , _GradientColorContrast ) );
					float lerpResult1877 = lerp( temp_output_1848_0 , ( 1.0 - temp_output_1848_0 ) , _GradientColorInvertMask);
					float Gradient_Color_Mask1854 = ( lerpResult1877 * _GradientColorIntensity );
					float3 lerpResult1838 = lerp( Base_Albedo1723 , temp_output_1836_0 , Gradient_Color_Mask1854);
					float3 Gradient_Color_Albedo1856 = lerpResult1838;
					#ifdef _ENABLEGRADIENTCOLOR_ON
					float3 staticSwitch1867 = Gradient_Color_Albedo1856;
					#else
					float3 staticSwitch1867 = Base_Albedo1723;
					#endif
					float3 Step_2_Albedo1914 = staticSwitch1867;
					float3 blendOpSrc1897 = Base_Albedo1723;
					float3 blendOpDest1897 = _SecondColor;
					float3 temp_output_1897_0 = ( saturate( (( blendOpDest1897 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest1897 ) * ( 1.0 - blendOpSrc1897 ) ) : ( 2.0 * blendOpDest1897 * blendOpSrc1897 ) ) ));
					float saferPower1886 = abs( ( ( 1.0 - IN.ase_color.a ) * _SecondColorOffset ) );
					float temp_output_1889_0 = saturate( pow( saferPower1886 , ( _SecondColorContrast + 2.0 ) ) );
					float lerpResult1890 = lerp( temp_output_1889_0 , ( 1.0 - temp_output_1889_0 ) , _SecondColorInvertMask);
					float Second_Color_Mask1895 = ( lerpResult1890 * _SecondColorIntensity );
					float3 lerpResult1899 = lerp( Step_2_Albedo1914 , temp_output_1897_0 , Second_Color_Mask1895);
					float3 Second_Color_Albedo1902 = lerpResult1899;
					#ifdef _ENABLESECONDCOLOR_ON
					float3 staticSwitch1921 = Second_Color_Albedo1902;
					#else
					float3 staticSwitch1921 = Step_2_Albedo1914;
					#endif
					float3 Step_3_Albedo1954 = staticSwitch1921;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Tint_Color_Mask1949 = saturate( round( ( frac( ( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) * ( _TintNoiseOffset * ASP_GlobalTintNoiseUVScale ) ) ) * ( _TintNoiseContrast * ASP_GlobalTintNoiseContrast ) ) ) );
					float3 lerpResult1951 = lerp( _TintColor1.rgb , _TintColor2.rgb , Tint_Color_Mask1949);
					float3 blendOpSrc1952 = Step_3_Albedo1954;
					float3 blendOpDest1952 = lerpResult1951;
					float3 temp_output_1952_0 = ( saturate( (( blendOpDest1952 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest1952 ) * ( 1.0 - blendOpSrc1952 ) ) : ( 2.0 * blendOpDest1952 * blendOpSrc1952 ) ) ));
					float temp_output_1961_0 = ( _TintNoiseIntensity * ASP_GlobalTintNoiseIntensity );
					float3 lerpResult1956 = lerp( Step_3_Albedo1954 , temp_output_1952_0 , temp_output_1961_0);
					float3 Tint_Color_Albedo1963 = lerpResult1956;
					#ifdef _ENABLETINTCOLOR_ON
					float3 staticSwitch2080 = Tint_Color_Albedo1963;
					#else
					float3 staticSwitch2080 = Step_3_Albedo1954;
					#endif
					float3 Step_4_Albedo2083 = staticSwitch2080;
					float3 Top_Layer_Color1995 = _TopLayerColor;
					float2 uv_BaseNormal1741 = IN.ase_texcoord2.xy;
					float3 tex2DNode1741 = UnpackScaleNormal( tex2D( _BaseNormal, uv_BaseNormal1741 ), _BaseNormalIntensity );
					float3 break105_g45 = tex2DNode1741;
					float switchResult107_g45 = (((ase_vface>0)?(break105_g45.z):(-break105_g45.z)));
					float3 appendResult108_g45 = (float3(break105_g45.x , break105_g45.y , switchResult107_g45));
					#ifdef _ENABLEFLIPNORMALS_ON
					float3 staticSwitch1744 = appendResult108_g45;
					#else
					float3 staticSwitch1744 = tex2DNode1741;
					#endif
					float3 Base_Normal1747 = staticSwitch1744;
					float3 desaturateInitialColor2013 = Base_Normal1747;
					float desaturateDot2013 = dot( desaturateInitialColor2013, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar2013 = lerp( desaturateInitialColor2013, desaturateDot2013.xxx, 1.0 );
					float3 ase_tangentWS = IN.ase_texcoord3.xyz;
					float3 ase_normalWS = IN.ase_texcoord4.xyz;
					float3 ase_bitangentWS = IN.ase_texcoord5.xyz;
					float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
					float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
					float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
					float3 tanNormal2012 = Base_Normal1747;
					float3 worldNormal2012 = float3( dot( tanToWorld0, tanNormal2012 ), dot( tanToWorld1, tanNormal2012 ), dot( tanToWorld2, tanNormal2012 ) );
					float3 temp_cast_0 = (worldNormal2012.y).xxx;
					#ifdef _ENABLEWORLDPROJECTION_ON
					float3 staticSwitch2017 = temp_cast_0;
					#else
					float3 staticSwitch2017 = desaturateVar2013;
					#endif
					float3 saferPower17_g46 = abs( abs( ( saturate( staticSwitch2017 ) + ( _TopLayerOffset * ASPT_TopLayerOffset ) ) ) );
					float temp_output_2005_0 = ( _TopLayerContrast * ASPT_TopLayerContrast );
					float3 temp_cast_1 = (temp_output_2005_0).xxx;
					float3 ase_positionWS = IN.ase_texcoord6.xyz;
					float saferPower2024 = abs( ( 1.0 - IN.ase_color.a ) );
					float Top_Layer_Contrast2025 = temp_output_2005_0;
					float3 lerpResult2029 = lerp( ( ( saturate( pow( saferPower17_g46 , temp_cast_1 ) ) * ( _TopLayerIntensity * ASPT_TopLayerIntensity ) ) * saturate(  (0.0 + ( ase_positionWS.y - ASPT_TopLayerHeightStart ) * ( 1.0 - 0.0 ) / ( ( ASPT_TopLayerHeightStart + ASPT_TopLayerHeightFade ) - ASPT_TopLayerHeightStart ) ) ) ) , float3( 0,0,0 ) , saturate( ( pow( saferPower2024 , Top_Layer_Contrast2025 ) * _TopLayerAOMask ) ));
					float3 Top_Layer_Mask2031 = saturate( lerpResult2029 );
					float3 lerpResult2034 = lerp( Step_4_Albedo2083 , Top_Layer_Color1995 , Top_Layer_Mask2031);
					float3 lerpResult2037 = lerp( Step_4_Albedo2083 , lerpResult2034 , saturate( ( ase_vface > 0 ? +1 : -1 ) ));
					#ifdef _ENABLEBACKFACEPROJECTION_ON
					float3 staticSwitch2041 = lerpResult2034;
					#else
					float3 staticSwitch2041 = lerpResult2037;
					#endif
					float3 Top_Layer_Albedo2042 = staticSwitch2041;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch2043 = Top_Layer_Albedo2042;
					#else
					float3 staticSwitch2043 = Step_4_Albedo2083;
					#endif
					float3 Output_Albedo1873 = staticSwitch2043;
					
					float2 uv_BaseSSE1757 = IN.ase_texcoord2.xy;
					float4 tex2DNode1757 = tex2D( _BaseSSE, uv_BaseSSE1757 );
					float RSE_Emissive_Mask1762 = tex2DNode1757.b;
					float saferPower1804 = abs( RSE_Emissive_Mask1762 );
					float3 temp_output_1806_0 = ( ( _BaseEmissiveColor.rgb * pow( saferPower1804 , _BaseEmissiveMaskContrast ) ) * _BaseEmissiveIntensity );
					float3 lerpResult1808 = lerp( temp_output_1806_0 , ( temp_output_1806_0 * IN.ase_color.a ) , _BaseEmissiveAOMask);
					float3 Base_Emissive1812 = lerpResult1808;
					float3 lerpResult3341 = lerp( Base_Emissive1812 , float3( 0,0,0 ) , Top_Layer_Mask2031);
					float3 Top_Layer_Emissive3342 = lerpResult3341;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch3343 = Top_Layer_Emissive3342;
					#else
					float3 staticSwitch3343 = Base_Emissive1812;
					#endif
					float3 Output_Emissive3346 = staticSwitch3343;
					
					float Opacity_Texture1737 = tex2DNode1753.a;
					float3 normalizeResult1780 = normalize( cross( ddx( ase_positionWS ) , ddy( ase_positionWS ) ) );
					float3 normalizeResult1787 = normalize( ( _WorldSpaceCameraPos - ase_positionWS ) );
					float dotResult1782 = dot( normalizeResult1780 , normalizeResult1787 );
					float lerpResult1794 = lerp( 1.0 , abs( dotResult1782 ) , _BaseGlancingAngleCut);
					#ifdef _ENABLEGLANCINGANGLECUT_ON
					float staticSwitch1788 = lerpResult1794;
					#else
					float staticSwitch1788 = 1.0;
					#endif
					float Base_Glancing_Angle1790 = staticSwitch1788;
					float Base_Opacity1739 = ( Opacity_Texture1737 * Base_Glancing_Angle1790 );
					

					o.Albedo = Output_Albedo1873;
					o.Normal = half3( 0, 0, 1 );
					o.Emission = Output_Emissive3346;
					o.Alpha = Base_Opacity1739;
					half AlphaClipThreshold = _CutOff;

					#ifdef _ALPHATEST_ON
						clip( o.Alpha - AlphaClipThreshold );
					#endif

					UnityMetaInput metaIN;
					UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
					metaIN.Albedo = o.Albedo;
					metaIN.Emission = o.Emission;
					#ifdef EDITOR_VISUALIZATION
						metaIN.VizUV = IN.vizUV;
						metaIN.LightCoord = IN.lightCoord;
					#endif
					return UnityMetaFragment(metaIN);
				}
				ENDCG
			}

			
			Pass
			{
				
				Name "ShadowCaster"
				Tags { "LightMode"="ShadowCaster" }
				ZWrite On
				ZTest LEqual
				AlphaToMask Off

				CGPROGRAM
				#define ASE_GEOMETRY 1
				#define ASE_FRAGMENT_NORMAL 0
				#define ASE_RECEIVE_SHADOWS
				#pragma multi_compile_instancing
				#define ASE_FOG
				#define ASE_TRANSLUCENCY 1
				#pragma multi_compile _ LOD_FADE_CROSSFADE
				#pragma multi_compile_local _ALPHATEST_ON
				#define ASE_VERSION 19903

				#pragma vertex vert
				#pragma fragment frag
				#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
				#pragma multi_compile_shadowcaster
				#ifndef UNITY_PASS_SHADOWCASTER
					#define UNITY_PASS_SHADOWCASTER
				#endif
				#include "HLSLSupport.cginc"
				#ifdef ASE_GEOMETRY
					#ifndef UNITY_INSTANCED_LOD_FADE
						#define UNITY_INSTANCED_LOD_FADE
					#endif
					#ifndef UNITY_INSTANCED_SH
						#define UNITY_INSTANCED_SH
					#endif
					#ifndef UNITY_INSTANCED_LIGHTMAPSTS
						#define UNITY_INSTANCED_LIGHTMAPSTS
					#endif
				#endif
				#include "UnityShaderVariables.cginc"
				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "UnityPBSLighting.cginc"

				#define ASE_NEEDS_VERT_POSITION
				#define ASE_NEEDS_TEXTURE_COORDINATES2
				#define ASE_NEEDS_VERT_TEXTURE_COORDINATES2
				#define ASE_NEEDS_TEXTURE_COORDINATES0
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLESTATICMESHSUPPORT_ON
				#pragma shader_feature_local _ENABLEGLANCINGANGLECUT_ON


				struct appdata
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					half4 tangent : TANGENT;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					float4 ase_color : COLOR;
					float4 ase_texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					V2F_SHADOW_CASTER;
					float4 ase_texcoord1 : TEXCOORD1;
					float4 ase_texcoord2 : TEXCOORD2;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				#ifdef UNITY_STANDARD_USE_DITHER_MASK
					sampler3D _DitherMaskLOD;
				#endif
				#ifdef ASE_TESSELLATION
					float _TessPhongStrength;
					float _TessValue;
					float _TessMin;
					float _TessMax;
					float _TessEdgeLength;
					float _TessMaxDisp;
				#endif

				uniform float _BaseSSSDirect;
				uniform float _BaseSSSAmbiet;
				uniform float _BaseSSSShadow;
				uniform float _BaseSSSNormalDistortion;
				uniform float _BaseSSSScattering;
				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform half ASPW_WindTreeSpeed;
				uniform half ASPW_WindTreeAmplitude;
				uniform float _WindTreeFlexibility;
				uniform half ASPW_WindTreeFlexibility;
				uniform float _WindTreeBaseRigidity;
				uniform float _WindLeafScale;
				uniform float _WindLeafSpeed;
				uniform half ASPW_WindTreeLeafSpeed;
				uniform float _WindLeafAmplitude;
				uniform half ASPW_WindTreeLeafAmplitude;
				uniform half ASPW_WindTreeLeafTurbulence;
				uniform float _WindLeafTurbulence;
				uniform float _WindLeafOffset;
				uniform half ASPW_WindTreeLeafOffset;
				uniform float ASPW_WindToggle;
				uniform sampler2D _BaseAlbedo;
				uniform float _BaseGlancingAngleCut;
				uniform float _CutOff;


				float3 ASESafeNormalize(float3 inVec)
				{
					float dp3 = max(1.175494351e-38, dot(inVec, inVec));
					return inVec* rsqrt(dp3);
				}
				
				float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
				{
					original -= center;
					float C = cos( angle );
					float S = sin( angle );
					float t = 1 - C;
					float m00 = t * u.x * u.x + C;
					float m01 = t * u.x * u.y - S * u.z;
					float m02 = t * u.x * u.z + S * u.y;
					float m10 = t * u.x * u.y + S * u.z;
					float m11 = t * u.y * u.y + C;
					float m12 = t * u.y * u.z - S * u.x;
					float m20 = t * u.x * u.z - S * u.y;
					float m21 = t * u.y * u.z + S * u.x;
					float m22 = t * u.z * u.z + C;
					float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
					return mul( finalMatrix, original ) + center;
				}
				

				v2f VertexFunction( appdata v  )
				{
					UNITY_SETUP_INSTANCE_ID(v);
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f,o);
					UNITY_TRANSFER_INSTANCE_ID(v,o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					float3 temp_cast_0 = (0.0).xxx;
					float3 Global_Wind_Direction493_g48 = ASPW_WindDirection;
					float3 worldToObjDir269_g48 = mul( unity_WorldToObject, float4( Global_Wind_Direction493_g48, 0.0 ) ).xyz;
					float3 normalizeResult268_g48 = ASESafeNormalize( worldToObjDir269_g48 );
					float3 Wind_Direction_Leaf85_g48 = normalizeResult268_g48;
					float3 break86_g48 = Wind_Direction_Leaf85_g48;
					float3 appendResult89_g48 = (float3(break86_g48.z , 0.0 , ( break86_g48.x * -1.0 )));
					float3 Wind_Direction91_g48 = appendResult89_g48;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Tree_Randomization149_g48 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_56_0_g48 = ( Wind_Tree_Randomization149_g48 + _Time.y );
					float Global_Wind_Tree_Speed491_g48 = ASPW_WindTreeSpeed;
					float temp_output_213_0_g48 = ( Global_Wind_Tree_Speed491_g48 * 7.0 );
					float Global_Wind_Tree_Amplitude464_g48 = ASPW_WindTreeAmplitude;
					float Global_Wind_Tree_Flexibility490_g48 = ASPW_WindTreeFlexibility;
					float temp_output_383_0_g48 = ( ( _WindTreeFlexibility * Global_Wind_Tree_Flexibility490_g48 ) * 0.3 );
					float Wind_Tree_257_g48 = ( ( ( ( ( sin( ( temp_output_56_0_g48 * ( temp_output_213_0_g48 * 0.1 ) ) ) + sin( ( temp_output_56_0_g48 * ( temp_output_213_0_g48 * 0.3 ) ) ) + sin( ( temp_output_56_0_g48 * ( temp_output_213_0_g48 * 0.5 ) ) ) ) + Global_Wind_Tree_Amplitude464_g48 ) * Global_Wind_Tree_Amplitude464_g48 ) * 0.01 ) * temp_output_383_0_g48 );
					float3 rotatedValue99_g48 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction91_g48, Wind_Tree_257_g48 );
					float3 Rotate_About_Axis354_g48 = ( rotatedValue99_g48 - v.vertex.xyz );
					float3 break452_g48 = normalizeResult268_g48;
					float3 appendResult454_g48 = (float3(break452_g48.x , 0.0 , break452_g48.z));
					float3 Wind_Direction_SM_Support453_g48 = appendResult454_g48;
					float Wind_Tree_Flexibility483_g48 = temp_output_383_0_g48;
					#ifdef _ENABLESTATICMESHSUPPORT_ON
					float3 staticSwitch482_g48 = ( Global_Wind_Tree_Amplitude464_g48 * Wind_Tree_257_g48 * Wind_Direction_SM_Support453_g48 * Wind_Tree_Flexibility483_g48 );
					#else
					float3 staticSwitch482_g48 = Rotate_About_Axis354_g48;
					#endif
					float3 Wind_Global450_g48 = staticSwitch482_g48;
					float2 texCoord433_g48 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float Wind_Mask_UV_CH2_VChannel434_g48 = texCoord433_g48.y;
					float saferPower113_g48 = abs( Wind_Mask_UV_CH2_VChannel434_g48 );
					float Wind_Tree_Mask114_g48 = pow( saferPower113_g48 , _WindTreeBaseRigidity );
					float3 temp_output_385_0_g48 = ( Wind_Global450_g48 * Wind_Tree_Mask114_g48 );
					float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( v.vertex ).xyz, 1 ) ).xyz;
					float temp_output_227_0_g48 = ( ( ase_positionWS.y * ( _WindLeafScale * 30.0 ) ) + _Time.y );
					float Global_Wind_Tree_Leaf_Speed495_g48 = ASPW_WindTreeLeafSpeed;
					float temp_output_223_0_g48 = ( _WindLeafSpeed * Global_Wind_Tree_Leaf_Speed495_g48 * 4.0 );
					float Global_Wind_Tree_Leaf_Amplitude497_g48 = ASPW_WindTreeLeafAmplitude;
					float temp_output_242_0_g48 = ( ( sin( ( temp_output_227_0_g48 * ( temp_output_223_0_g48 * 0.5 ) ) ) + sin( ( temp_output_227_0_g48 * ( temp_output_223_0_g48 * 1.25 ) ) ) + sin( ( temp_output_227_0_g48 * ( temp_output_223_0_g48 * 1.5 ) ) ) ) * ( ( _WindLeafAmplitude * Global_Wind_Tree_Leaf_Amplitude497_g48 ) * 0.1 ) );
					float temp_output_243_0_g48 = ( temp_output_242_0_g48 * 0.2 );
					float3 appendResult244_g48 = (float3(temp_output_243_0_g48 , temp_output_242_0_g48 , temp_output_243_0_g48));
					float3 Wind_Leaf245_g48 = appendResult244_g48;
					float3 break416_g48 = normalizeResult268_g48;
					float3 appendResult417_g48 = (float3(( break416_g48.x * -1.0 ) , ( break416_g48.z * -1.0 ) , 0.0));
					float3 Wind_Direction_Turbulence419_g48 = appendResult417_g48;
					float2 appendResult322_g48 = (float2(ase_positionWS.x , ase_positionWS.z));
					float Global_Wind_Tree_Leaf_Turbulence502_g48 = ASPW_WindTreeLeafTurbulence;
					float Wind_Leaf_Turbulence334_g48 = saturate( ( tex2Dlod( ASP_GlobalTintNoiseTexture, float4( ( ( ( Wind_Direction_Turbulence419_g48 * _Time.y ) * 0.1 ) + float3( ( appendResult322_g48 * 0.1 ) ,  0.0 ) ).xy, 0, 0.0) ).g + ( ( Global_Wind_Tree_Leaf_Turbulence502_g48 * _WindLeafTurbulence ) * 0.1 ) ) );
					float Wind_Mask_VColor_Blue429_g48 = v.ase_color.b;
					float3 Wind_Leaf_Mask285_g48 = ( ( ( Wind_Leaf245_g48 * Wind_Direction_Leaf85_g48 ) * Wind_Leaf_Turbulence334_g48 ) * Wind_Mask_VColor_Blue429_g48 );
					float Global_Wind_Tree_Leaf_Offset500_g48 = ASPW_WindTreeLeafOffset;
					float Wind_Mask_VColor_Green430_g48 = v.ase_color.g;
					float2 texCoord312_g48 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float3 Wind_Leaf_Offset314_g48 = ( ( ( ( Wind_Direction_Leaf85_g48 * ( _WindLeafOffset * Global_Wind_Tree_Leaf_Offset500_g48 * 0.5 ) ) + Wind_Leaf_Mask285_g48 ) * Wind_Mask_VColor_Green430_g48 ) * texCoord312_g48.y );
					float Global_Wind_Toggle504_g48 = ASPW_WindToggle;
					float3 lerpResult125_g48 = lerp( float3( 0,0,0 ) , ( temp_output_385_0_g48 + ( Wind_Leaf_Mask285_g48 + Wind_Leaf_Offset314_g48 ) ) , Global_Wind_Toggle504_g48);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch469_g48 = lerpResult125_g48;
					#else
					float3 staticSwitch469_g48 = temp_cast_0;
					#endif
					
					o.ase_texcoord2.xyz = ase_positionWS;
					
					o.ase_texcoord1.xy = v.ase_texcoord.xy;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord1.zw = 0;
					o.ase_texcoord2.w = 0;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch469_g48;
					#ifdef ASE_ABSOLUTE_VERTEX_POS
						v.vertex.xyz = vertexValue;
					#else
						v.vertex.xyz += vertexValue;
					#endif
					v.vertex.w = 1;
					v.normal = v.normal;
					v.tangent = v.tangent;

					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
				}

				#if defined(ASE_TESSELLATION)
				struct VertexControl
				{
					float4 vertex : INTERNALTESSPOS;
					half4 tangent : TANGENT;
					half3 normal : NORMAL;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					float4 ase_color : COLOR;
					float4 ase_texcoord : TEXCOORD0;

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct TessellationFactors
				{
					float edge[3] : SV_TessFactor;
					float inside : SV_InsideTessFactor;
				};

				VertexControl vert ( appdata v )
				{
					VertexControl o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					o.vertex = v.vertex;
					o.tangent = v.tangent;
					o.normal = v.normal;
					o.texcoord1 = v.texcoord1;
					o.texcoord2 = v.texcoord2;
					o.ase_color = v.ase_color;
					o.ase_texcoord = v.ase_texcoord;
					return o;
				}

				TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
				{
					TessellationFactors o;
					float4 tf = 1;
					float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
					float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
					#if defined(ASE_FIXED_TESSELLATION)
					tf = FixedTess( tessValue );
					#elif defined(ASE_DISTANCE_TESSELLATION)
					tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, UNITY_MATRIX_M, _WorldSpaceCameraPos );
					#elif defined(ASE_LENGTH_TESSELLATION)
					tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams );
					#elif defined(ASE_LENGTH_CULL_TESSELLATION)
					tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, UNITY_MATRIX_M, _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
					#endif
					o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
					return o;
				}

				[domain("tri")]
				[partitioning("fractional_odd")]
				[outputtopology("triangle_cw")]
				[patchconstantfunc("TessellationFunction")]
				[outputcontrolpoints(3)]
				VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
				{
				   return patch[id];
				}

				[domain("tri")]
				v2f DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
				{
					appdata o = (appdata) 0;
					o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
					o.tangent = patch[0].tangent * bary.x + patch[1].tangent * bary.y + patch[2].tangent * bary.z;
					o.normal = patch[0].normal * bary.x + patch[1].normal * bary.y + patch[2].normal * bary.z;
					o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
					o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
					o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
					o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
					#if defined(ASE_PHONG_TESSELLATION)
					float3 pp[3];
					for (int i = 0; i < 3; ++i)
						pp[i] = o.vertex.xyz - patch[i].normal * (dot(o.vertex.xyz, patch[i].normal) - dot(patch[i].vertex.xyz, patch[i].normal));
					float phongStrength = _TessPhongStrength;
					o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
					#endif
					UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
					return VertexFunction(o);
				}
				#else
				v2f vert( appdata v )
				{
					return VertexFunction( v );
				}
				#endif

				half4 frag( v2f IN 
							#if defined( ASE_DEPTH_WRITE_ON )
								, out float outputDepth : SV_Depth
							#endif
							) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(IN);

					#ifdef LOD_FADE_CROSSFADE
						UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
					#endif

					#if defined(ASE_LIGHTING_SIMPLE)
						SurfaceOutput o = (SurfaceOutput)0;
					#else
						#if defined(_SPECULAR_SETUP)
							SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
						#else
							SurfaceOutputStandard o = (SurfaceOutputStandard)0;
						#endif
						o.Occlusion = 1;
					#endif

					float2 uv_BaseAlbedo1753 = IN.ase_texcoord1.xy;
					float4 tex2DNode1753 = tex2D( _BaseAlbedo, uv_BaseAlbedo1753 );
					float Opacity_Texture1737 = tex2DNode1753.a;
					float3 ase_positionWS = IN.ase_texcoord2.xyz;
					float3 normalizeResult1780 = normalize( cross( ddx( ase_positionWS ) , ddy( ase_positionWS ) ) );
					float3 normalizeResult1787 = normalize( ( _WorldSpaceCameraPos - ase_positionWS ) );
					float dotResult1782 = dot( normalizeResult1780 , normalizeResult1787 );
					float lerpResult1794 = lerp( 1.0 , abs( dotResult1782 ) , _BaseGlancingAngleCut);
					#ifdef _ENABLEGLANCINGANGLECUT_ON
					float staticSwitch1788 = lerpResult1794;
					#else
					float staticSwitch1788 = 1.0;
					#endif
					float Base_Glancing_Angle1790 = staticSwitch1788;
					float Base_Opacity1739 = ( Opacity_Texture1737 * Base_Glancing_Angle1790 );
					

					o.Normal = half3( 0, 0, 1 );

					o.Alpha = Base_Opacity1739;
					half AlphaClipThreshold = _CutOff;
					half AlphaClipThresholdShadow = 0.5;

					#if defined( ASE_DEPTH_WRITE_ON )
						float DeviceDepth = IN.pos.z;
					#endif

					#ifdef _ALPHATEST_SHADOW_ON
						if (unity_LightShadowBias.z != 0.0)
							clip(o.Alpha - AlphaClipThresholdShadow);
						#ifdef _ALPHATEST_ON
						else
							clip(o.Alpha - AlphaClipThreshold);
						#endif
					#else
						#ifdef _ALPHATEST_ON
							clip(o.Alpha - AlphaClipThreshold);
						#endif
					#endif

					#ifdef UNITY_STANDARD_USE_DITHER_MASK
						half alphaRef = tex3D(_DitherMaskLOD, float3(IN.pos.xy*0.25,o.Alpha*0.9375)).a;
						clip(alphaRef - 0.01);
					#endif

					#if defined( ASE_DEPTH_WRITE_ON )
						outputDepth = DeviceDepth;
					#endif

					SHADOW_CASTER_FRAGMENT(IN)
				}
			ENDCG
		}
		
	}
	
	
	Fallback Off
}
/*ASEBEGIN
Version=19903
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1832;-2752,4800;Inherit;False;2747;687;;16;1790;1788;1789;1794;1793;1783;1782;1787;1780;1785;1779;1784;1786;1778;1777;1776;Base Glancing Angle Cut;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1776;-2688,4848;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DdyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1778;-2432,4928;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DdxOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1777;-2432,4848;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1784;-2688,5104;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1786;-2688,5264;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CrossProductOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1779;-2304,4848;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1785;-2304,5104;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1780;-2048,4848;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1787;-2048,5104;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1782;-1792,4848;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1783;-1536,4848;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1793;-1536,4976;Inherit;False;Property;_BaseGlancingAngleCut;Base Glancing Angle Cut;2;0;Create;True;0;0;0;False;0;False;0.8;0.8;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1823;-2751,-2098;Inherit;False;2236;689;;13;1723;3617;3618;3324;1722;1717;1714;1721;1716;1713;1712;1737;1753;Base Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1794;-1152,4848;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1789;-1152,4976;Inherit;False;Constant;_One;One;20;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1753;-2688,-2048;Inherit;True;Property;_BaseAlbedo;Base Albedo;12;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;5a350584f9ab3f847a1d216801dbcaa4;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1788;-784,4848;Inherit;False;Property;_EnableGlancingAngleCut;Enable Glancing Angle Cut [ForwardOnly];1;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1834;-2752,4272;Inherit;False;818;323;;4;1739;1740;1791;1738;Base Opacity;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1737;-2304,-1920;Inherit;False;Opacity Texture;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1790;-272,4848;Inherit;False;Base Glancing Angle;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1791;-2688,4464;Inherit;False;1790;Base Glancing Angle;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1738;-2688,4336;Inherit;False;1737;Opacity Texture;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1740;-2432,4336;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3537;-2752,2752;Inherit;False;1873;400;;9;3461;3460;3454;3455;3453;3452;3451;3450;3449;Base SSS Max Distance;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1881;700,-2098;Inherit;False;2627;1330;;28;1860;1861;1862;1854;1864;1856;1837;1835;1853;1880;1846;1843;1839;1855;1859;1863;1858;1857;1838;1836;1852;1877;1851;1848;1879;1841;1842;1840;Gradient Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1831;-2752,1712;Inherit;False;1597;831;;13;1726;1729;3287;1727;1731;1728;1725;1730;3288;1733;1732;1734;1735;Base AO/SSS Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3540;-2752,960;Inherit;False;701;593;;7;3325;3262;3263;3261;3260;3538;1754;Base SSS Parameters;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1829;-2751,-688;Inherit;False;1218.115;650.9459;[R] Smoothness | [G] Subsurface | [B] Emissive;10;1762;1761;3683;1760;3684;3682;3681;3680;3679;1757;Base SSE;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1830;-2752,320;Inherit;False;2371;431;;14;3539;3536;1768;1767;3458;3350;1764;1766;1765;1755;1763;1769;1756;2050;Base SSS;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1925;3780,-2098;Inherit;False;2494;1456;;27;1896;1910;1902;1894;1892;1885;1884;1916;1917;1908;1909;1904;1903;1899;1901;1897;1895;1958;1893;1890;1891;1889;1886;1888;1887;1883;1882;Second Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1993;6975,-2098;Inherit;False;2365.561;1843.417;;32;1953;1968;1963;1943;1932;1931;1942;1962;1960;1930;1929;1967;1949;3281;3282;1944;1933;3280;3279;3273;3275;3274;3272;1966;1965;1964;1956;1961;1959;1952;1951;1950;Tint Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2092;14400,-2096;Inherit;False;3649.281;1072.076;;36;3346;2077;3345;3344;3343;2076;2075;2074;1954;1918;2043;2080;1869;1921;1867;1955;1874;2045;2062;2084;2081;1972;1922;1919;1915;1872;1873;2044;2083;1970;1914;1865;1866;1870;1871;1868;Switches;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1828;-2752,-1202;Inherit;False;1853.396;306.4712;;5;1741;1744;1742;1747;3615;Base Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2088;10692,704;Inherit;False;3259;1028;;26;3349;2021;2003;1996;1999;2001;2000;1998;1997;2005;2009;2031;2030;2029;2028;2027;2024;2026;2023;2022;2017;2013;2012;2018;2025;3638;Top Layer Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1833;-2752,3376;Inherit;False;2227;573;;12;1811;1807;1805;1801;1812;1808;1810;1809;1806;1802;1804;1803;Base Emissive;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2091;10689,-2112;Inherit;False;2235;574;;10;2036;3312;2041;2042;2037;2039;2034;2035;2038;1995;Top Layer Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2090;10690,-1344;Inherit;False;2235;573;;14;3309;2061;2060;2059;2058;2054;2056;2057;2052;2055;2047;2048;2051;2046;Top Layer SSS;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2089;10690,-576;Inherit;False;2362.389;511.3842;;11;2064;2066;2063;2070;2069;2068;2067;2065;3308;2073;2072;Top Layer Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3347;10693,208;Inherit;False;828;306;;4;3341;3342;3340;3339;Top Layer Emissive;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3283;18752,-2096;Inherit;False;1018.207;978.311;;9;1746;2792;1875;1813;1814;1774;1751;1876;3637;Outputs;1,0,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1739;-2176,4336;Inherit;False;Base Opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1840;768,-1152;Inherit;False;2;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1842;1024,-1152;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1713;-2688,-1792;Inherit;False;Property;_BaseAlbedoDesaturation;Base Albedo Desaturation;6;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1843;768,-1024;Inherit;False;Property;_GradientColorOffset;Gradient Color Offset;30;0;Create;True;0;0;0;False;0;False;1;1.72;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1841;1280,-1152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1846;1280,-1024;Inherit;False;Property;_GradientColorContrast;Gradient Color Contrast;31;0;Create;True;0;0;0;False;0;False;1;1;0;30;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1757;-2688,-640;Inherit;True;Property;_BaseSSE;Base SSE;14;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.DesaturateOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1712;-2304,-2048;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1722;-2304,-1792;Inherit;False;Property;_BaseAlbedoBrightness;Base Albedo Brightness;5;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1879;1664,-1152;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1721;-2048,-2048;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1714;-1920,-1792;Half;False;Property;_BaseAlbedoColor;Base Albedo Color;4;1;[HDR];Create;True;0;0;0;False;0;False;0.5019608,0.5019608,0.5019608,0.5019608;0.2933233,0.3396226,0.07529369,1;False;False;0;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1848;1920,-1152;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1717;-1920,-1632;Inherit;False;Property;_BaseLerpBetweenColorandTexture;Base Lerp Between Color and Texture;11;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3324;-1664,-2048;Inherit;False;Overlay;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1851;2176,-1024;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1880;2065.281,-879.9517;Inherit;False;Property;_GradientColorInvertMask;Gradient Color Invert Mask;32;1;[IntRange];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1716;-1280,-2048;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3617;-1280,-1920;Inherit;False;1735;Base AO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1877;2432,-1152;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1882;3840,-1152;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1853;2432,-1024;Inherit;False;Property;_GradientColorIntensity;Gradient Color Intensity;29;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3618;-1024,-2048;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1852;2816,-1152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1883;4096,-1152;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1884;3840,-896;Inherit;False;Property;_SecondColorOffset;Second Color Offset;36;0;Create;True;0;0;0;False;0;False;1;1;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1885;3840,-768;Inherit;False;Property;_SecondColorContrast;Second Color Contrast;37;0;Create;True;0;0;0;False;0;False;1;7.06;0;30;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1723;-768,-2048;Inherit;False;Base Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1887;4352,-1152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1888;4352,-768;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1854;3072,-1152;Inherit;False;Gradient Color Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1837;768,-2048;Inherit;False;1723;Base Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1835;768,-1920;Inherit;False;Property;_GradientColor;Gradient Color;28;1;[HDR];Create;True;0;0;0;False;0;False;0.5019608,0.5019608,0.5019608,0;0.9716981,0,0,0;True;False;0;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.PowerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1886;4608,-1152;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1855;1536,-1792;Inherit;False;1854;Gradient Color Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1839;1536,-1920;Inherit;False;1723;Base Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendOpsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1836;1152,-2048;Inherit;False;Overlay;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1838;1920,-2048;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1861;1280,-1440;Inherit;False;1769;Base SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1860;1280,-1536;Inherit;False;1723;Base Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1857;768,-1664;Inherit;False;1769;Base SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1889;4864,-1152;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1858;1536,-1664;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1863;1536,-1408;Inherit;False;1854;Gradient Color Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectPositionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3274;7040,-896;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1856;2304,-2048;Inherit;False;Gradient Color Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1862;1536,-1536;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1892;4992,-896;Inherit;False;Property;_SecondColorInvertMask;Second Color Invert Mask;38;1;[IntRange];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1891;5120,-1024;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1859;1920,-1664;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3275;7296,-896;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1931;7040,-640;Inherit;False;Property;_TintNoiseOffset;Tint Noise Offset;43;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1932;7040,-512;Half;False;Global;ASP_GlobalTintNoiseUVScale;ASP_GlobalTintNoiseUVScale;15;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1868;14464,-2048;Inherit;False;1723;Base Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1865;14464,-1920;Inherit;False;1856;Gradient Color Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1890;5376,-1152;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1894;5376,-896;Inherit;False;Property;_SecondColorIntensity;Second Color Intensity;35;0;Create;True;0;0;0;False;0;False;1;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1893;5760,-1152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3272;7424,-896;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1.23;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1933;7424,-640;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1867;14976,-2048;Inherit;False;Property;_EnableGradientColor;Enable Gradient Color;27;0;Create;True;0;0;0;False;1;Header(Base Gradient Color);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1864;2304,-1664;Inherit;False;Gradient Color SSS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1870;14464,-1792;Inherit;False;1723;Base Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1871;14464,-1696;Inherit;False;1769;Base SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3279;7680,-896;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1942;7680,-640;Inherit;False;Property;_TintNoiseContrast;Tint Noise Contrast;44;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1943;7680,-512;Half;False;Global;ASP_GlobalTintNoiseContrast;ASP_GlobalTintNoiseContrast;15;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1866;14464,-1600;Inherit;False;1864;Gradient Color SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1914;15360,-2048;Inherit;False;Step 2 Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1872;14720,-1792;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1895;6016,-1152;Inherit;False;Second Color Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1896;3840,-1920;Inherit;False;Property;_SecondColor;Second Color;34;1;[HDR];Create;True;0;0;0;False;0;False;0.5019608,0.5019608,0.5019608,0;0.2074582,0.3961525,0.6981132,1;True;False;0;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1958;3840,-2048;Inherit;False;1723;Base Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1901;4608,-1792;Inherit;False;1895;Second Color Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3273;7936,-896;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1944;8064,-640;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1917;4608,-1920;Inherit;False;1914;Step 2 Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1869;14976,-1792;Inherit;False;Property;_Keyword0;Keyword 0;27;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;1867;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1742;-2688,-1152;Inherit;False;Property;_BaseNormalIntensity;Base Normal Intensity;7;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1897;4224,-2048;Inherit;False;Overlay;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1899;4864,-2048;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1903;3840,-1664;Inherit;False;1769;Base SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3280;8192,-896;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1915;15360,-1792;Inherit;False;Step 2 SSS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1741;-2304,-1152;Inherit;True;Property;_BaseNormal;Base Normal;13;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1904;4608,-1664;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1909;4608,-1408;Inherit;False;1895;Second Color Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3282;8448,-896;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1916;4608,-1536;Inherit;False;1915;Step 2 SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1902;5376,-2048;Inherit;False;Second Color Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3615;-1920,-1024;Inherit;False;MF_ASP_FlipNormals;-1;;45;7bd57a034e214964eb311c43f973a80e;0;1;110;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1908;5120,-1664;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3281;8704,-896;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1918;15360,-1920;Inherit;False;1902;Second Color Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1744;-1536,-1152;Inherit;False;Property;_EnableFlipNormals;Enable Flip Normals;0;0;Create;True;0;0;0;False;1;Header(Base);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2009;11136,1632;Half;False;Global;ASPT_TopLayerContrast;ASPT_TopLayerContrast;32;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2003;11136,1536;Half;False;Property;_TopLayerContrast;Top Layer Contrast;52;0;Create;True;0;0;0;False;0;False;10;30;0;30;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1949;9088,-896;Inherit;False;Tint Color Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1910;5376,-1664;Inherit;False;Second Color SSS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1747;-1152,-1152;Inherit;False;Base Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1921;15744,-2048;Inherit;False;Property;_EnableSecondColor;Enable Second Color;33;0;Create;True;0;0;0;False;1;Header(Base Second Color);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2005;11520,1536;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1950;7040,-1536;Inherit;False;1949;Tint Color Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1929;7040,-2048;Inherit;False;Property;_TintColor1;Tint Color 1;40;1;[HDR];Create;True;0;0;0;False;0;False;0.5019608,0.5019608,0.5019608,0;0.8679245,0.5694438,0.4134924,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1930;7040,-1792;Inherit;False;Property;_TintColor2;Tint Color 2;41;1;[HDR];Create;True;0;0;0;False;0;False;0.5019608,0.5019608,0.5019608,0;0.5019608,0.5019608,0.5019608,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1919;15360,-1696;Inherit;False;1910;Second Color SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1954;16128,-2048;Inherit;False;Step 3 Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2025;11776,1536;Inherit;False;Top Layer Contrast;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2018;10752,768;Inherit;False;1747;Base Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2022;11904,1152;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1951;7424,-2048;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1922;15744,-1792;Inherit;False;Property;_Keyword1;Keyword 1;33;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;1921;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2012;11136,896;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DesaturateOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2013;11136,768;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1997;11136,1440;Half;False;Global;ASPT_TopLayerOffset;ASPT_TopLayerOffset;27;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1998;11136,1248;Half;False;Global;ASPT_TopLayerIntensity;ASPT_TopLayerIntensity;27;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1999;11136,1152;Half;False;Property;_TopLayerIntensity;Top Layer Intensity;50;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1996;11136,1344;Half;False;Property;_TopLayerOffset;Top Layer Offset;51;0;Create;True;0;0;0;False;0;False;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2023;12160,1152;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1953;7424,-1792;Inherit;False;1954;Step 3 Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2026;12160,1280;Inherit;False;2025;Top Layer Contrast;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1967;7808,-1280;Inherit;False;1769;Base SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1960;8192,-1664;Inherit;False;Property;_TintNoiseIntensity;Tint Noise Intensity;42;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1962;8192,-1536;Half;False;Global;ASP_GlobalTintNoiseIntensity;ASP_GlobalTintNoiseIntensity;15;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1955;16128,-1792;Inherit;False;Step 3 SSS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2017;11520,768;Float;False;Property;_EnableWorldProjection;Enable World Projection;54;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2000;11520,1344;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2001;11520,1152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2021;12416,1280;Half;False;Property;_TopLayerAOMask;Top Layer AO Mask;53;0;Create;True;0;0;0;False;0;False;1;3.75;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1952;7808,-2048;Inherit;False;Overlay;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2024;12416,1152;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1959;8192,-1792;Inherit;False;1954;Step 3 Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1961;8576,-1664;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1964;8176,-1248;Inherit;False;1955;Step 3 SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1965;8192,-1408;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3349;11904,768;Inherit;False;MF_ASP_Global_TopLayer;-1;;46;6dc1725aa9649cc439f99987e8365ea2;0;4;22;FLOAT3;0,0,0;False;24;FLOAT;1;False;23;FLOAT;0.5;False;25;FLOAT;10;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2027;12784,1152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3645;11904,1024;Inherit;False;MF_ASP_Global_TopLayerHeight;-1;;47;0851e7dd80a2406479a4b23dfe36fe1f;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1956;8704,-2048;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1966;8704,-1408;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3638;12416,768;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2028;12928,1152;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1963;9088,-2048;Inherit;False;Tint Color Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1968;9088,-1408;Inherit;False;Tint Color SSS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3312;10752,-2048;Inherit;False;Property;_TopLayerColor;Top Layer Color;46;1;[HDR];Create;True;0;0;0;False;0;False;0.7764706,0.8392157,0.9490196,0;0.25,0.5227272,1,1;True;False;0;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2029;13184,768;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1995;11136,-2048;Inherit;False;Top Layer Color;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1970;16128,-1920;Inherit;False;1963;Tint Color Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1972;16128,-1664;Inherit;False;1968;Tint Color SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2030;13440,768;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2046;10752,-1280;Inherit;False;1995;Top Layer Color;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2051;10752,-1024;Inherit;False;2050;Global SSS Intensity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2048;10752,-1152;Inherit;False;Property;_TopLayerSSSIntensity;Top Layer SSS Intensity;48;0;Create;True;0;0;0;False;0;False;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2081;16512,-1792;Inherit;False;Property;_Keyword2;Keyword 2;39;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;2080;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2080;16512,-2048;Inherit;False;Property;_EnableTintColor;Enable Tint Color;39;0;Create;True;0;0;0;False;1;Header(Base Tint Color);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2031;13696,768;Inherit;False;Top Layer Mask;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2047;11136,-1280;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2083;16896,-2048;Inherit;False;Step 4 Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2084;16896,-1792;Inherit;False;Step 4 SSS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TwoSidedSign, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2067;11136,-256;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3309;11136,-1152;Inherit;False;Property;_TopLayerSSSColor;Top Layer SSS Color;47;1;[HDR];Create;True;0;0;0;False;0;False;0.7843137,0.9215686,1,1;0.3915094,1,0.8225385,1;True;False;0;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2066;10752,-256;Inherit;False;2031;Top Layer Mask;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TwoSidedSign, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2038;11392,-1664;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2035;11392,-1920;Inherit;False;2083;Step 4 Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2055;11392,-1072;Inherit;False;2031;Top Layer Mask;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2052;11392,-1280;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TwoSidedSign, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2057;11392,-896;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2056;11392,-1152;Inherit;False;2084;Step 4 SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2068;11392,-256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2070;11136,-384;Inherit;False;1760;Base Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2036;11392,-1792;Inherit;False;2031;Top Layer Mask;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2064;11136,-512;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2034;11776,-2048;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2039;11648,-1664;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2054;11776,-1280;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2058;11648,-896;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2069;11648,-384;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2037;12032,-1920;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2072;12032,-512;Inherit;False;Property;_Keyword5;Keyword 4;55;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;2041;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2059;12032,-1152;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3339;10752,256;Inherit;False;1812;Base Emissive;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3340;10752,384;Inherit;False;2031;Top Layer Mask;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2060;12288,-1280;Inherit;False;Property;_Keyword4;Keyword 4;55;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;2041;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2041;12288,-2048;Inherit;False;Property;_EnableBackfaceProjection;Enable Backface Projection;55;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3308;12416,-512;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3341;11008,256;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2042;12672,-2048;Inherit;False;Top Layer Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2073;12672,-512;Inherit;False;Top Layer Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2061;12672,-1280;Inherit;False;Top Layer SSS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3342;11264,256;Inherit;False;Top Layer Emissive;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2044;16896,-1920;Inherit;False;2042;Top Layer Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2062;16896,-1664;Inherit;False;2061;Top Layer SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2075;16896,-1536;Inherit;False;1760;Base Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2076;16896,-1408;Inherit;False;2073;Top Layer Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3344;16896,-1280;Inherit;False;1812;Base Emissive;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3345;16896,-1152;Inherit;False;3342;Top Layer Emissive;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2045;17280,-1792;Inherit;False;Property;_Keyword3;Keyword 3;45;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;2043;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2043;17280,-2048;Inherit;False;Property;_EnableTopLayerBlend;Enable Top Layer Blend;45;0;Create;True;0;0;0;False;1;Header(Top Layer);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2074;17280,-1536;Inherit;False;Property;_Keyword6;Keyword 3;45;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;2043;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3343;17280,-1280;Inherit;False;Property;_Keyword7;Keyword 3;45;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;2043;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1873;17792,-2048;Inherit;False;Output Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1874;17792,-1792;Inherit;False;Output SSS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2077;17792,-1536;Inherit;False;Output Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3346;17792,-1280;Inherit;False;Output Emissive;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1876;18816,-1376;Inherit;False;1874;Output SSS;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1751;18816,-1952;Inherit;False;1747;Base Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1774;18816,-1664;Inherit;False;1735;Base AO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1814;18816,-1760;Inherit;False;2077;Output Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1813;18816,-1856;Inherit;False;3346;Output Emissive;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1746;18816,-1568;Inherit;False;1739;Base Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2792;18816,-1472;Inherit;False;Property;_CutOff;Base Opacity Cutoff;3;0;Create;False;0;0;0;False;0;False;0.3;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1875;18816,-2048;Inherit;False;1873;Output Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1732;-2688,2160;Half;False;Global;ASP_GlobalTreeAO;ASP_GlobalTreeAO;15;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1730;-2688,1904;Half;False;Global;ASP_GlobalTreeSSSAOInfluence;ASP_GlobalTreeSSSAOInfluence;13;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1728;-2688,1776;Inherit;False;Property;_BaseSSSAOInfluence;Base SSS AO Influence;17;0;Create;True;0;0;0;False;0;False;0.8;0.95;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1731;-2688,2032;Inherit;False;Property;_BaseTreeAOIntensity;Base Tree AO Intensity;10;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3449;-2688,2960;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3450;-2688,2800;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1733;-2304,2032;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1729;-2304,1776;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3451;-2304,2800;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3452;-2304,2928;Half;False;Constant;_MinDistance;Min Distance;27;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3454;-2304,3024;Half;False;Global;ASP_GlobalTreeSSSDistance;ASP_GlobalTreeSSSDistance;24;0;Create;True;0;0;0;False;0;False;200;5000;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1754;-2688,1008;Inherit;False;Property;_BaseSSSIntensity;Base SSS Intensity;16;0;Create;True;0;0;0;True;0;False;1;9.5;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3288;-2048,2032;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1725;-2048,2160;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3287;-2048,1776;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3453;-1920,2800;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3538;-2304,1008;Inherit;False;Local SSS Intensity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1756;-2688,496;Half;False;Global;ASP_GlobalTreeSSSIntensity;ASP_GlobalTreeSSSIntensity;14;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1727;-1792,1904;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1726;-1792,1776;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3455;-1664,2800;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3539;-2688,368;Inherit;False;3538;Local SSS Intensity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3350;-2688,624;Inherit;False;Constant;_Float4;Float 4;56;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1735;-1536,1904;Inherit;False;Base AO;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1734;-1536,1776;Inherit;False;Base SSS AO Influence;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1755;-2304,368;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3460;-1408,2800;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1764;-2304,496;Inherit;False;1761;RSE Subsurface Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1763;-2048,368;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1766;-2048,496;Inherit;False;1734;Base SSS AO Influence;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3461;-1152,2800;Inherit;False;Base SSS Max Distance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1765;-1664,368;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3536;-1664,496;Inherit;False;3461;Base SSS Max Distance;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3458;-1408,368;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1768;-1408,496;Inherit;False;Property;_BaseSSSColor;Base SSS Color;15;1;[HDR];Create;True;0;0;0;False;1;Header(Base SSS);False;1,1,1,0;1,0.6039734,0,0;True;False;0;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1767;-1152,368;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1769;-768,368;Inherit;False;Base SSS;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1803;-2688,3696;Inherit;False;1762;RSE Emissive Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1805;-2688,3824;Inherit;False;Property;_BaseEmissiveMaskContrast;Base Emissive Mask Contrast;25;0;Create;True;0;0;0;False;0;False;1;1;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1804;-2304,3696;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1801;-2688,3440;Inherit;False;Property;_BaseEmissiveColor;Base Emissive Color;23;1;[HDR];Create;True;0;0;0;False;1;Header(Base Emissive);False;0,0,0,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2050;-2304,624;Inherit;False;Global SSS Intensity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1802;-2048,3440;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1807;-2048,3696;Inherit;False;Property;_BaseEmissiveIntensity;Base Emissive Intensity;24;0;Create;True;0;0;0;False;0;False;1;1;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1806;-1792,3440;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1809;-1664,3696;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1810;-1408,3696;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1811;-1408,3824;Inherit;False;Property;_BaseEmissiveAOMask;Base Emissive AO Mask;26;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1808;-1152,3440;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1812;-768,3440;Inherit;False;Base Emissive;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3261;-2688,1264;Inherit;False;Property;_BaseSSSDirect;Base SSS Direct;20;0;Create;True;0;0;0;True;0;False;0.9;0.9;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3262;-2688,1344;Inherit;False;Property;_BaseSSSAmbiet;Base SSS Ambiet;21;0;Create;True;0;0;0;True;0;False;0.1;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3263;-2688,1424;Inherit;False;Property;_BaseSSSShadow;Base SSS Shadow;22;0;Create;True;0;0;0;True;0;False;0.9;0.9;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3325;-2688,1104;Inherit;False;Property;_BaseSSSNormalDistortion;Base SSS Normal Distortion;18;0;Create;True;0;0;0;True;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3260;-2688,1184;Inherit;False;Property;_BaseSSSScattering;Base SSS Scattering;19;0;Create;True;0;0;0;True;0;False;2;10;1;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3679;-2304,-320;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3680;-2048,-320;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3684;-2688,-128;Inherit;False;3683;Texture Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1760;-1792,-320;Inherit;False;Base Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3683;-2304,-640;Inherit;False;Texture Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1761;-2304,-544;Inherit;False;RSE Subsurface Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1762;-2304,-448;Inherit;False;RSE Emissive Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2063;10752,-512;Inherit;False;1760;Base Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3637;18816,-1280;Inherit;False;MF_ASP_Global_WindTrees;56;;48;587b51c6cec05a64bb99d28802483760;0;0;2;FLOAT3;49;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2065;10752,-384;Inherit;False;Property;_TopLayerSmoothnessIntensity;Top Layer Smoothness Intensity;49;0;Create;True;0;0;0;False;0;False;0;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3681;-2688,-320;Half;False;Property;_BaseSmoothnessMin;Base Smoothness Min;8;0;Create;True;0;0;0;False;0;False;0;0.5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3682;-2688,-224;Inherit;False;Property;_BaseSmoothnessMax;Base Smoothness Max;9;0;Create;True;0;0;0;False;0;False;1;2.5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3674;19456,-2048;Float;False;True;-1;3;;0;4;ANGRYMESH/Stylized Pack/Tree Leaf;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ForwardBase;0;1;ForwardBase;17;True;True;0;4;False;;1;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;_render_coverage;False;True;2;False;_render_cull;True;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;1;False;_render_zw;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;True;True;0;2;False;;3;False;;0;5;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;44;Category;0;0;  Instanced Terrain Normals;1;0;Workflow;1;0;Surface;0;0;  Blend;0;0;  Dither Shadows;1;0;Two Sided;0;638773212206862503;Alpha Clipping;1;638912883674275893;  Use Shadow Threshold;0;0;Deferred Pass;0;638773213061071414;Normal Space,InvertActionOnDeselection;0;0;Transmission;0;638773175531655874;  Transmission Shadow;0.5,False,;0;Translucency;1;638773212197633338;  Translucency Strength;1,True,_BaseSSSIntensity;638912887251421145;  Normal Distortion;0.5,True,_BaseSSSNormalDistortion;638912887265416955;  Scattering;2,True,_BaseSSSScattering;638912887270307929;  Direct;0.9,True,_BaseSSSDirect;638912887276137922;  Ambient;0.1,True,_BaseSSSAmbiet;638912887280764698;  Shadow;0.5,True,_BaseSSSShadow;638912887284529743;Cast Shadows;1;0;Receive Shadows;1;0;Receive Specular;2;0;Receive Reflections;2;0;GPU Instancing;1;0;LOD CrossFade;1;638773218561031012;Built-in Fog;1;0;Ambient Light;1;0;Meta Pass;1;0;Add Pass;1;0;Override Baked GI;0;0;Write Depth;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Disable Batching;0;0;Vertex Position,InvertActionOnDeselection;1;0;0;6;False;True;True;False;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3675;19456,-2048;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;1;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ForwardAdd;0;2;ForwardAdd;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;True;4;1;False;;1;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;True;1;LightMode=ForwardAdd;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3676;19456,-2048;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;1;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;Deferred;0;3;Deferred;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Deferred;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3677;19456,-2048;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;1;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;Meta;0;4;Meta;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3678;19456,-2048;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;1;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ShadowCaster;0;5;ShadowCaster;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3673;20960,-2048;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;4;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ExtraPrePass;0;0;ExtraPrePass;6;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;0;False;0
WireConnection;1778;0;1776;0
WireConnection;1777;0;1776;0
WireConnection;1779;0;1777;0
WireConnection;1779;1;1778;0
WireConnection;1785;0;1784;0
WireConnection;1785;1;1786;0
WireConnection;1780;0;1779;0
WireConnection;1787;0;1785;0
WireConnection;1782;0;1780;0
WireConnection;1782;1;1787;0
WireConnection;1783;0;1782;0
WireConnection;1794;1;1783;0
WireConnection;1794;2;1793;0
WireConnection;1788;1;1789;0
WireConnection;1788;0;1794;0
WireConnection;1737;0;1753;4
WireConnection;1790;0;1788;0
WireConnection;1740;0;1738;0
WireConnection;1740;1;1791;0
WireConnection;1739;0;1740;0
WireConnection;1842;0;1840;2
WireConnection;1841;0;1842;0
WireConnection;1841;1;1843;0
WireConnection;1712;0;1753;5
WireConnection;1712;1;1713;0
WireConnection;1879;0;1841;0
WireConnection;1879;1;1846;0
WireConnection;1721;0;1712;0
WireConnection;1721;1;1722;0
WireConnection;1848;0;1879;0
WireConnection;3324;0;1721;0
WireConnection;3324;1;1714;0
WireConnection;1851;0;1848;0
WireConnection;1716;0;1714;0
WireConnection;1716;1;3324;0
WireConnection;1716;2;1717;0
WireConnection;1877;0;1848;0
WireConnection;1877;1;1851;0
WireConnection;1877;2;1880;0
WireConnection;3618;0;1716;0
WireConnection;3618;1;3617;0
WireConnection;1852;0;1877;0
WireConnection;1852;1;1853;0
WireConnection;1883;0;1882;4
WireConnection;1723;0;3618;0
WireConnection;1887;0;1883;0
WireConnection;1887;1;1884;0
WireConnection;1888;0;1885;0
WireConnection;1854;0;1852;0
WireConnection;1886;0;1887;0
WireConnection;1886;1;1888;0
WireConnection;1836;0;1837;0
WireConnection;1836;1;1835;0
WireConnection;1838;0;1839;0
WireConnection;1838;1;1836;0
WireConnection;1838;2;1855;0
WireConnection;1889;0;1886;0
WireConnection;1858;0;1836;0
WireConnection;1858;1;1857;0
WireConnection;1856;0;1838;0
WireConnection;1862;0;1860;0
WireConnection;1862;1;1861;0
WireConnection;1891;0;1889;0
WireConnection;1859;0;1862;0
WireConnection;1859;1;1858;0
WireConnection;1859;2;1863;0
WireConnection;3275;0;3274;1
WireConnection;3275;1;3274;2
WireConnection;3275;2;3274;3
WireConnection;1890;0;1889;0
WireConnection;1890;1;1891;0
WireConnection;1890;2;1892;0
WireConnection;1893;0;1890;0
WireConnection;1893;1;1894;0
WireConnection;3272;0;3275;0
WireConnection;1933;0;1931;0
WireConnection;1933;1;1932;0
WireConnection;1867;1;1868;0
WireConnection;1867;0;1865;0
WireConnection;1864;0;1859;0
WireConnection;3279;0;3272;0
WireConnection;3279;1;1933;0
WireConnection;1914;0;1867;0
WireConnection;1872;0;1870;0
WireConnection;1872;1;1871;0
WireConnection;1895;0;1893;0
WireConnection;3273;0;3279;0
WireConnection;1944;0;1942;0
WireConnection;1944;1;1943;0
WireConnection;1869;1;1872;0
WireConnection;1869;0;1866;0
WireConnection;1897;0;1958;0
WireConnection;1897;1;1896;0
WireConnection;1899;0;1917;0
WireConnection;1899;1;1897;0
WireConnection;1899;2;1901;0
WireConnection;3280;0;3273;0
WireConnection;3280;1;1944;0
WireConnection;1915;0;1869;0
WireConnection;1741;5;1742;0
WireConnection;1904;0;1897;0
WireConnection;1904;1;1903;0
WireConnection;3282;0;3280;0
WireConnection;1902;0;1899;0
WireConnection;3615;110;1741;0
WireConnection;1908;0;1916;0
WireConnection;1908;1;1904;0
WireConnection;1908;2;1909;0
WireConnection;3281;0;3282;0
WireConnection;1744;1;1741;0
WireConnection;1744;0;3615;0
WireConnection;1949;0;3281;0
WireConnection;1910;0;1908;0
WireConnection;1747;0;1744;0
WireConnection;1921;1;1914;0
WireConnection;1921;0;1918;0
WireConnection;2005;0;2003;0
WireConnection;2005;1;2009;0
WireConnection;1954;0;1921;0
WireConnection;2025;0;2005;0
WireConnection;1951;0;1929;5
WireConnection;1951;1;1930;5
WireConnection;1951;2;1950;0
WireConnection;1922;1;1915;0
WireConnection;1922;0;1919;0
WireConnection;2012;0;2018;0
WireConnection;2013;0;2018;0
WireConnection;2023;0;2022;4
WireConnection;1955;0;1922;0
WireConnection;2017;1;2013;0
WireConnection;2017;0;2012;2
WireConnection;2000;0;1996;0
WireConnection;2000;1;1997;0
WireConnection;2001;0;1999;0
WireConnection;2001;1;1998;0
WireConnection;1952;0;1953;0
WireConnection;1952;1;1951;0
WireConnection;2024;0;2023;0
WireConnection;2024;1;2026;0
WireConnection;1961;0;1960;0
WireConnection;1961;1;1962;0
WireConnection;1965;0;1952;0
WireConnection;1965;1;1967;0
WireConnection;3349;22;2017;0
WireConnection;3349;24;2001;0
WireConnection;3349;23;2000;0
WireConnection;3349;25;2005;0
WireConnection;2027;0;2024;0
WireConnection;2027;1;2021;0
WireConnection;1956;0;1959;0
WireConnection;1956;1;1952;0
WireConnection;1956;2;1961;0
WireConnection;1966;0;1964;0
WireConnection;1966;1;1965;0
WireConnection;1966;2;1961;0
WireConnection;3638;0;3349;0
WireConnection;3638;1;3645;0
WireConnection;2028;0;2027;0
WireConnection;1963;0;1956;0
WireConnection;1968;0;1966;0
WireConnection;2029;0;3638;0
WireConnection;2029;2;2028;0
WireConnection;1995;0;3312;0
WireConnection;2030;0;2029;0
WireConnection;2081;1;1955;0
WireConnection;2081;0;1972;0
WireConnection;2080;1;1954;0
WireConnection;2080;0;1970;0
WireConnection;2031;0;2030;0
WireConnection;2047;0;2046;0
WireConnection;2047;1;2048;0
WireConnection;2047;2;2051;0
WireConnection;2083;0;2080;0
WireConnection;2084;0;2081;0
WireConnection;2052;0;2047;0
WireConnection;2052;1;3309;0
WireConnection;2068;0;2067;0
WireConnection;2064;0;2063;0
WireConnection;2064;1;2065;0
WireConnection;2064;2;2066;0
WireConnection;2034;0;2035;0
WireConnection;2034;1;1995;0
WireConnection;2034;2;2036;0
WireConnection;2039;0;2038;0
WireConnection;2054;0;2056;0
WireConnection;2054;1;2052;0
WireConnection;2054;2;2055;0
WireConnection;2058;0;2057;0
WireConnection;2069;0;2070;0
WireConnection;2069;1;2064;0
WireConnection;2069;2;2068;0
WireConnection;2037;0;2035;0
WireConnection;2037;1;2034;0
WireConnection;2037;2;2039;0
WireConnection;2072;1;2069;0
WireConnection;2072;0;2064;0
WireConnection;2059;0;2056;0
WireConnection;2059;1;2054;0
WireConnection;2059;2;2058;0
WireConnection;2060;1;2059;0
WireConnection;2060;0;2054;0
WireConnection;2041;1;2037;0
WireConnection;2041;0;2034;0
WireConnection;3308;0;2072;0
WireConnection;3341;0;3339;0
WireConnection;3341;2;3340;0
WireConnection;2042;0;2041;0
WireConnection;2073;0;3308;0
WireConnection;2061;0;2060;0
WireConnection;3342;0;3341;0
WireConnection;2045;1;2084;0
WireConnection;2045;0;2062;0
WireConnection;2043;1;2083;0
WireConnection;2043;0;2044;0
WireConnection;2074;1;2075;0
WireConnection;2074;0;2076;0
WireConnection;3343;1;3344;0
WireConnection;3343;0;3345;0
WireConnection;1873;0;2043;0
WireConnection;1874;0;2045;0
WireConnection;2077;0;2074;0
WireConnection;3346;0;3343;0
WireConnection;1733;0;1731;0
WireConnection;1733;1;1732;0
WireConnection;1729;0;1728;0
WireConnection;1729;1;1730;0
WireConnection;3451;0;3450;0
WireConnection;3451;1;3449;0
WireConnection;3288;0;1733;0
WireConnection;3287;0;1729;0
WireConnection;3453;0;3451;0
WireConnection;3453;1;3452;0
WireConnection;3453;2;3454;0
WireConnection;3538;0;1754;0
WireConnection;1727;1;1725;4
WireConnection;1727;2;3288;0
WireConnection;1726;1;1725;4
WireConnection;1726;2;3287;0
WireConnection;3455;0;3453;0
WireConnection;1735;0;1727;0
WireConnection;1734;0;1726;0
WireConnection;1755;0;3539;0
WireConnection;1755;1;1756;0
WireConnection;1755;2;3350;0
WireConnection;3460;0;3455;0
WireConnection;1763;0;1755;0
WireConnection;1763;1;1764;0
WireConnection;3461;0;3460;0
WireConnection;1765;0;1763;0
WireConnection;1765;1;1766;0
WireConnection;3458;0;1765;0
WireConnection;3458;1;3536;0
WireConnection;1767;0;3458;0
WireConnection;1767;1;1768;0
WireConnection;1769;0;1767;0
WireConnection;1804;0;1803;0
WireConnection;1804;1;1805;0
WireConnection;2050;0;1756;0
WireConnection;1802;0;1801;5
WireConnection;1802;1;1804;0
WireConnection;1806;0;1802;0
WireConnection;1806;1;1807;0
WireConnection;1810;0;1806;0
WireConnection;1810;1;1809;4
WireConnection;1808;0;1806;0
WireConnection;1808;1;1810;0
WireConnection;1808;2;1811;0
WireConnection;1812;0;1808;0
WireConnection;3679;0;3681;0
WireConnection;3679;1;3682;0
WireConnection;3679;2;3684;0
WireConnection;3680;0;3679;0
WireConnection;1760;0;3680;0
WireConnection;3683;0;1757;1
WireConnection;1761;0;1757;2
WireConnection;1762;0;1757;3
WireConnection;3674;0;1875;0
WireConnection;3674;1;1751;0
WireConnection;3674;5;1814;0
WireConnection;3674;6;1774;0
WireConnection;3674;2;1813;0
WireConnection;3674;7;1746;0
WireConnection;3674;8;2792;0
WireConnection;3674;14;1876;0
WireConnection;3674;15;3637;0
ASEEND*/
//CHKSM=9FCF7F44DB951A42DE5611C296C55F3C1E0C3B84