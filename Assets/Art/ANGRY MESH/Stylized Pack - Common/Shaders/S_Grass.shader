// Made with Amplify Shader Editor v1.9.9.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ANGRYMESH/Stylized Pack/Grass"
{
	Properties
	{
		[Header(Base)][Toggle( _ENABLESMOOTHNESSWAVES_ON )] _EnableSmoothnessWaves( "Enable Smoothness Waves", Float ) = 1
		_BaseOpacityCutoff( "Base Opacity Cutoff", Range( 0, 1 ) ) = 0.3
		[HDR] _BaseAlbedoColor( "Base Albedo Color", Color ) = ( 0.5019608, 0.5019608, 0.5019608, 0 )
		_BaseAlbedoBrightness( "Base Albedo Brightness", Range( 0, 5 ) ) = 1
		_BaseAlbedoDesaturation( "Base Albedo Desaturation", Range( 0, 1 ) ) = 0
		_BaseSmoothnessIntensity( "Base Smoothness Intensity", Range( 0, 1 ) ) = 0.5
		_BaseSmoothnessWaves( "Base Smoothness Waves", Range( 0, 1 ) ) = 0.5
		_BaseNormalIntensity( "Base Normal Intensity", Range( 0, 5 ) ) = 0
		[NoScaleOffset] _BaseAlbedo( "Base Albedo", 2D ) = "gray" {}
		[NoScaleOffset] _BaseNormal( "Base Normal", 2D ) = "bump" {}
		[Header(Bottom Color)][Toggle( _ENABLEBOTTOMCOLOR_ON )] _EnableBottomColor( "Enable Bottom Color", Float ) = 1
		[Toggle( _ENABLEBOTTOMDITHER_ON )] _EnableBottomDither( "Enable Bottom Dither", Float ) = 0
		[HDR] _BottomColor( "Bottom Color", Color ) = ( 0.5019608, 0.5019608, 0.5019608, 0 )
		_BottomColorOffset( "Bottom Color Offset", Range( 0, 5 ) ) = 1
		_BottomColorContrast( "Bottom Color Contrast", Range( 0, 5 ) ) = 1
		_BottomDitherOffset( "Bottom Dither Offset", Range( -1, 1 ) ) = 0
		_BottomDitherContrast( "Bottom Dither Contrast", Range( 1, 10 ) ) = 3
		[Header(Tint Color)][Toggle( _ENABLETINTVARIATIONCOLOR_ON )] _EnableTintVariationColor( "Enable Tint Variation Color", Float ) = 1
		[HDR] _TintColor( "Tint Color", Color ) = ( 0.5019608, 0.5019608, 0.5019608, 0 )
		_TintNoiseUVScale( "Tint Noise UV Scale", Range( 0, 50 ) ) = 5
		_TintNoiseIntensity( "Tint Noise Intensity", Range( 0, 1 ) ) = 1
		_TintNoiseContrast( "Tint Noise Contrast", Range( 0, 10 ) ) = 5
		[IntRange] _TintNoiseInvertMask( "Tint Noise Invert Mask", Range( 0, 1 ) ) = 0
		[Header(Wind)][Toggle( _ENABLEWIND_ON )] _EnableWind( "Enable Wind", Float ) = 1
		_WindGrassAmplitude( "Wind Grass Amplitude", Range( 0, 1 ) ) = 1
		_WindGrassSpeed( "Wind Grass Speed", Range( 0, 1 ) ) = 1
		_WindGrassScale( "Wind Grass Scale", Range( 0, 1 ) ) = 1
		_WindGrassTurbulence( "Wind Grass Turbulence", Range( 0, 1 ) ) = 1
		_WindGrassFlexibility( "Wind Grass Flexibility", Range( 0, 1 ) ) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}


		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5

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

			Blend One Zero

			CGPROGRAM
				#define ASE_GEOMETRY 1
				#define ASE_FRAGMENT_NORMAL 0
				#define ASE_RECEIVE_SHADOWS
				#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
				#pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF
				#pragma multi_compile_instancing
				#pragma multi_compile _ LOD_FADE_CROSSFADE
				#pragma multi_compile_fog
				#define ASE_FOG
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
				#define ASE_NEEDS_TEXTURE_COORDINATES0
				#define ASE_NEEDS_WORLD_POSITION
				#define ASE_NEEDS_FRAG_WORLD_POSITION
				#define ASE_NEEDS_FRAG_COLOR
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLEBOTTOMCOLOR_ON
				#pragma shader_feature_local _ENABLETINTVARIATIONCOLOR_ON
				#pragma shader_feature_local _ENABLESMOOTHNESSWAVES_ON
				#pragma shader_feature_local _ENABLEBOTTOMDITHER_ON


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

				uniform sampler2D ASPW_WindGrassWavesNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform float _WindGrassSpeed;
				uniform half ASPW_WindGrassSpeed;
				uniform float _WindGrassAmplitude;
				uniform half ASPW_WindGrassAmplitude;
				uniform half ASPW_WindGrassFlexibility;
				uniform float _WindGrassFlexibility;
				uniform half ASPW_WindGrassWavesAmplitude;
				uniform half ASPW_WindGrassWavesSpeed;
				uniform half ASPW_WindGrassWavesScale;
				uniform float _WindGrassScale;
				uniform half ASPW_WindGrassTurbulence;
				uniform float _WindGrassTurbulence;
				uniform half ASPW_WindToggle;
				uniform sampler2D _BaseAlbedo;
				uniform float _BaseAlbedoDesaturation;
				uniform float _BaseAlbedoBrightness;
				uniform float4 _BaseAlbedoColor;
				uniform float4 _TintColor;
				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half ASP_GlobalTintNoiseUVScale;
				uniform float _TintNoiseUVScale;
				uniform float _TintNoiseInvertMask;
				uniform half ASP_GlobalTintNoiseContrast;
				uniform float _TintNoiseContrast;
				uniform half ASP_GlobalTintNoiseToggle;
				uniform float _TintNoiseIntensity;
				uniform half ASP_GlobalTintNoiseIntensity;
				uniform float4 _BottomColor;
				uniform float _BottomColorOffset;
				uniform float _BottomColorContrast;
				uniform sampler2D _BaseNormal;
				uniform float _BaseNormalIntensity;
				uniform float _BaseSmoothnessIntensity;
				uniform float _BaseSmoothnessWaves;
				uniform float _BottomDitherOffset;
				uniform float _BottomDitherContrast;
				uniform float _BaseOpacityCutoff;


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
				
				float4 ASEScreenPositionNormalizedToPixel( float4 screenPosNorm, float4 screenParams )
				{
					float4 screenPosPixel = screenPosNorm * float4( screenParams.xy, 1, 1 );
					#if UNITY_UV_STARTS_AT_TOP
						screenPosPixel.xy = float2( screenPosPixel.x, ( _ProjectionParams.x < 0 ) ? screenParams.y - screenPosPixel.y : screenPosPixel.y );
					#else
						screenPosPixel.xy = float2( screenPosPixel.x, ( _ProjectionParams.x > 0 ) ? screenParams.y - screenPosPixel.y : screenPosPixel.y );
					#endif
					return screenPosPixel;
				}
				
				inline float Dither4x4Bayer( uint x, uint y )
				{
					const float dither[ 16 ] = {
					     1,  9,  3, 11,
					    13,  5, 15,  7,
					     4, 12,  2, 10,
					    16,  8, 14,  6 };
					uint r = y * 4 + x;
					return dither[ min( r, 15 ) ] / 16; // same # of instructions as pre-dividing due to compiler magic
				}
				

				v2f VertexFunction( appdata v  )
				{
					UNITY_SETUP_INSTANCE_ID(v);
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f,o);
					UNITY_TRANSFER_INSTANCE_ID(v,o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					float3 temp_cast_0 = (0.0).xxx;
					float3 Global_Wind_Direction238_g1 = ASPW_WindDirection;
					float3 normalizeResult41_g1 = ASESafeNormalize( Global_Wind_Direction238_g1 );
					float3 worldToObjDir40_g1 = mul( unity_WorldToObject, float4( normalizeResult41_g1, 0.0 ) ).xyz;
					float3 Wind_Direction_Leaf50_g1 = worldToObjDir40_g1;
					float3 break42_g1 = Wind_Direction_Leaf50_g1;
					float3 appendResult43_g1 = (float3(break42_g1.z , 0.0 , ( break42_g1.x * -1.0 )));
					float3 Wind_Direction52_g1 = appendResult43_g1;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Grass_Randomization65_g1 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_5_0_g1 = ( Wind_Grass_Randomization65_g1 + _Time.y );
					float Global_Wind_Grass_Speed144_g1 = ASPW_WindGrassSpeed;
					float temp_output_9_0_g1 = ( _WindGrassSpeed * Global_Wind_Grass_Speed144_g1 * 10.0 );
					float Local_Wind_Grass_Aplitude180_g1 = _WindGrassAmplitude;
					float Global_Wind_Grass_Amplitude172_g1 = ASPW_WindGrassAmplitude;
					float temp_output_27_0_g1 = ( Local_Wind_Grass_Aplitude180_g1 * Global_Wind_Grass_Amplitude172_g1 );
					float Global_Wind_Grass_Flexibility164_g1 = ASPW_WindGrassFlexibility;
					float Wind_Main31_g1 = ( ( ( ( ( sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.0 ) ) ) + sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.25 ) ) ) + sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.5 ) ) ) ) + temp_output_27_0_g1 ) * temp_output_27_0_g1 ) / 50.0 ) * ( ( Global_Wind_Grass_Flexibility164_g1 * _WindGrassFlexibility ) * 0.1 ) );
					float Global_Wind_Grass_Waves_Amplitude162_g1 = ASPW_WindGrassWavesAmplitude;
					float3 appendResult119_g1 = (float3(( normalizeResult41_g1.x * -1.0 ) , ( 0.0 * -1.0 ) , 0.0));
					float3 Wind_Direction_Waves118_g1 = appendResult119_g1;
					float Global_Wind_Grass_Waves_Speed166_g1 = ASPW_WindGrassWavesSpeed;
					float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( v.vertex ).xyz, 1 ) ).xyz;
					float2 appendResult73_g1 = (float2(ase_positionWS.x , ase_positionWS.z));
					float Global_Wind_Grass_Waves_Scale168_g1 = ASPW_WindGrassWavesScale;
					float Wind_Waves93_g1 = tex2Dlod( ASPW_WindGrassWavesNoiseTexture, float4( ( ( ( Wind_Direction_Waves118_g1 * _Time.y ) * ( Global_Wind_Grass_Waves_Speed166_g1 * 0.1 ) ) + float3( ( ( appendResult73_g1 * Global_Wind_Grass_Waves_Scale168_g1 ) * 0.1 ) ,  0.0 ) ).xy, 0, 0.0) ).r;
					float lerpResult96_g1 = lerp( Wind_Main31_g1 , ( Wind_Main31_g1 * Global_Wind_Grass_Waves_Amplitude162_g1 ) , Wind_Waves93_g1);
					float Wind_Main_with_Waves108_g1 = lerpResult96_g1;
					float temp_output_141_0_g1 = ( ( ase_positionWS.y * ( _WindGrassScale * 10.0 ) ) + _Time.y );
					float Local_Wind_Grass_Speed186_g1 = _WindGrassSpeed;
					float temp_output_146_0_g1 = ( Global_Wind_Grass_Speed144_g1 * Local_Wind_Grass_Speed186_g1 * 10.0 );
					float Global_Wind_Grass_Turbulence161_g1 = ASPW_WindGrassTurbulence;
					float clampResult175_g1 = clamp( Global_Wind_Grass_Amplitude172_g1 , 0.0 , 1.0 );
					float temp_output_188_0_g1 = ( ( ( sin( ( temp_output_141_0_g1 * ( temp_output_146_0_g1 * 1.0 ) ) ) + sin( ( temp_output_141_0_g1 * ( temp_output_146_0_g1 * 1.25 ) ) ) ) * 0.5 ) * ( ( Global_Wind_Grass_Turbulence161_g1 * ( clampResult175_g1 * ( _WindGrassTurbulence * Local_Wind_Grass_Aplitude180_g1 ) ) ) * 0.1 ) );
					float3 appendResult183_g1 = (float3(temp_output_188_0_g1 , ( temp_output_188_0_g1 * 0.2 ) , temp_output_188_0_g1));
					float3 Wind_Turbulence185_g1 = appendResult183_g1;
					float3 rotatedValue56_g1 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction52_g1, ( Wind_Main_with_Waves108_g1 + Wind_Turbulence185_g1 ).x );
					float3 Output_Wind35_g1 = ( rotatedValue56_g1 - v.vertex.xyz );
					float Wind_Mask225_g1 = v.ase_color.r;
					float3 lerpResult232_g1 = lerp( float3( 0,0,0 ) , ( Output_Wind35_g1 * Wind_Mask225_g1 ) , ASPW_WindToggle);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch192_g1 = lerpResult232_g1;
					#else
					float3 staticSwitch192_g1 = temp_cast_0;
					#endif
					
					o.ase_texcoord6.xy = v.texcoord.xyzw.xy;
					o.ase_color = v.ase_color;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord6.zw = 0;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch192_g1;
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

					float2 uv_BaseAlbedo280 = IN.ase_texcoord6.xy;
					float4 tex2DNode280 = tex2D( _BaseAlbedo, uv_BaseAlbedo280 );
					float3 desaturateInitialColor281 = tex2DNode280.rgb;
					float desaturateDot281 = dot( desaturateInitialColor281, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar281 = lerp( desaturateInitialColor281, desaturateDot281.xxx, _BaseAlbedoDesaturation );
					float3 Albedo_Texture299 = saturate( ( desaturateVar281 * _BaseAlbedoBrightness ) );
					float3 blendOpSrc283 = Albedo_Texture299;
					float3 blendOpDest283 = _BaseAlbedoColor.rgb;
					float3 Base_Albedo289 = (( blendOpDest283 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest283 ) * ( 1.0 - blendOpSrc283 ) ) : ( 2.0 * blendOpDest283 * blendOpSrc283 ) );
					float3 blendOpSrc345 = Albedo_Texture299;
					float3 blendOpDest345 = _TintColor.rgb;
					float2 appendResult649 = (float2(PositionWS.x , PositionWS.z));
					float4 tex2DNode651 = tex2D( ASP_GlobalTintNoiseTexture, ( appendResult649 * ( 0.01 * ASP_GlobalTintNoiseUVScale * _TintNoiseUVScale ) ) );
					float lerpResult676 = lerp( tex2DNode651.r , ( 1.0 - tex2DNode651.r ) , _TintNoiseInvertMask);
					float Base_Tint_Color_Mask659 = saturate( ( lerpResult676 * ( ASP_GlobalTintNoiseContrast * _TintNoiseContrast ) * IN.ase_color.r ) );
					float3 lerpResult656 = lerp( Base_Albedo289 , (( blendOpDest345 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest345 ) * ( 1.0 - blendOpSrc345 ) ) : ( 2.0 * blendOpDest345 * blendOpSrc345 ) ) , Base_Tint_Color_Mask659);
					float3 lerpResult661 = lerp( Base_Albedo289 , lerpResult656 , ( ASP_GlobalTintNoiseToggle * _TintNoiseIntensity * ASP_GlobalTintNoiseIntensity ));
					#ifdef _ENABLETINTVARIATIONCOLOR_ON
					float3 staticSwitch403 = lerpResult661;
					#else
					float3 staticSwitch403 = Base_Albedo289;
					#endif
					float3 Base_Albedo_and_Tint_Color374 = staticSwitch403;
					float3 blendOpSrc331 = Albedo_Texture299;
					float3 blendOpDest331 = _BottomColor.rgb;
					float saferPower336 = abs( ( 1.0 - IN.ase_color.a ) );
					float3 lerpResult340 = lerp( Base_Albedo_and_Tint_Color374 , (( blendOpDest331 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest331 ) * ( 1.0 - blendOpSrc331 ) ) : ( 2.0 * blendOpDest331 * blendOpSrc331 ) ) , saturate( ( _BottomColorOffset * pow( saferPower336 , _BottomColorContrast ) ) ));
					#ifdef _ENABLEBOTTOMCOLOR_ON
					float3 staticSwitch375 = lerpResult340;
					#else
					float3 staticSwitch375 = Base_Albedo_and_Tint_Color374;
					#endif
					float3 Output_Albedo342 = staticSwitch375;
					
					float2 uv_BaseNormal318 = IN.ase_texcoord6.xy;
					float3 Base_Normal642 = UnpackScaleNormal( tex2D( _BaseNormal, uv_BaseNormal318 ), _BaseNormalIntensity );
					
					float3 Global_Wind_Direction238_g1 = ASPW_WindDirection;
					float3 normalizeResult41_g1 = ASESafeNormalize( Global_Wind_Direction238_g1 );
					float3 appendResult119_g1 = (float3(( normalizeResult41_g1.x * -1.0 ) , ( 0.0 * -1.0 ) , 0.0));
					float3 Wind_Direction_Waves118_g1 = appendResult119_g1;
					float Global_Wind_Grass_Waves_Speed166_g1 = ASPW_WindGrassWavesSpeed;
					float2 appendResult73_g1 = (float2(PositionWS.x , PositionWS.z));
					float Global_Wind_Grass_Waves_Scale168_g1 = ASPW_WindGrassWavesScale;
					float Wind_Waves93_g1 = tex2D( ASPW_WindGrassWavesNoiseTexture, ( ( ( Wind_Direction_Waves118_g1 * _Time.y ) * ( Global_Wind_Grass_Waves_Speed166_g1 * 0.1 ) ) + float3( ( ( appendResult73_g1 * Global_Wind_Grass_Waves_Scale168_g1 ) * 0.1 ) ,  0.0 ) ).xy ).r;
					float Wind_Waves621 = Wind_Waves93_g1;
					float lerpResult304 = lerp( 0.0 , _BaseSmoothnessIntensity , ( IN.ase_color.r * ( Wind_Waves621 + _BaseSmoothnessWaves ) ));
					#ifdef _ENABLESMOOTHNESSWAVES_ON
					float staticSwitch719 = lerpResult304;
					#else
					float staticSwitch719 = _BaseSmoothnessIntensity;
					#endif
					float Base_Smoothness314 = saturate( staticSwitch719 );
					
					float Base_Opacity295 = tex2DNode280.a;
					float4 ase_positionSS_Pixel = ASEScreenPositionNormalizedToPixel( ScreenPosNorm, _ScreenParams );
					float dither728 = Dither4x4Bayer( fmod( ase_positionSS_Pixel.x, 4 ), fmod( ase_positionSS_Pixel.y, 4 ) );
					dither728 = step( dither728, saturate( saturate( ( ( IN.ase_color.r - _BottomDitherOffset ) * ( _BottomDitherContrast * 2 ) ) ) * 1.00001 ) );
					#ifdef _ENABLEBOTTOMDITHER_ON
					float staticSwitch731 = ( dither728 * Base_Opacity295 );
					#else
					float staticSwitch731 = Base_Opacity295;
					#endif
					float Output_Opacity732 = staticSwitch731;
					

					o.Albedo = Output_Albedo342;
					o.Normal = Base_Normal642;

					half3 Specular = half3( 0, 0, 0 );
					half Metallic = 0;
					half Smoothness = Base_Smoothness314;
					half Occlusion = 1;

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

					o.Emission = half3( 0, 0, 0 );
					o.Alpha = Output_Opacity732;
					half AlphaClipThreshold = _BaseOpacityCutoff;
					half AlphaClipThresholdShadow = 0.5;
					half3 BakedGI = 0;
					half3 Transmission = 1;
					half3 Translucency = 1;

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
						half shadow = _TransShadow;
						half normal = _TransNormal;
						half scattering = _TransScattering;
						half direct = _TransDirect;
						half ambient = _TransAmbient;
						half strength = _TransStrength;

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
				#pragma multi_compile _ LOD_FADE_CROSSFADE
				#pragma multi_compile_fog
				#define ASE_FOG
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
				#define ASE_NEEDS_TEXTURE_COORDINATES0
				#define ASE_NEEDS_WORLD_POSITION
				#define ASE_NEEDS_FRAG_WORLD_POSITION
				#define ASE_NEEDS_FRAG_COLOR
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLEBOTTOMCOLOR_ON
				#pragma shader_feature_local _ENABLETINTVARIATIONCOLOR_ON
				#pragma shader_feature_local _ENABLESMOOTHNESSWAVES_ON
				#pragma shader_feature_local _ENABLEBOTTOMDITHER_ON


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

				uniform sampler2D ASPW_WindGrassWavesNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform float _WindGrassSpeed;
				uniform half ASPW_WindGrassSpeed;
				uniform float _WindGrassAmplitude;
				uniform half ASPW_WindGrassAmplitude;
				uniform half ASPW_WindGrassFlexibility;
				uniform float _WindGrassFlexibility;
				uniform half ASPW_WindGrassWavesAmplitude;
				uniform half ASPW_WindGrassWavesSpeed;
				uniform half ASPW_WindGrassWavesScale;
				uniform float _WindGrassScale;
				uniform half ASPW_WindGrassTurbulence;
				uniform float _WindGrassTurbulence;
				uniform half ASPW_WindToggle;
				uniform sampler2D _BaseAlbedo;
				uniform float _BaseAlbedoDesaturation;
				uniform float _BaseAlbedoBrightness;
				uniform float4 _BaseAlbedoColor;
				uniform float4 _TintColor;
				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half ASP_GlobalTintNoiseUVScale;
				uniform float _TintNoiseUVScale;
				uniform float _TintNoiseInvertMask;
				uniform half ASP_GlobalTintNoiseContrast;
				uniform float _TintNoiseContrast;
				uniform half ASP_GlobalTintNoiseToggle;
				uniform float _TintNoiseIntensity;
				uniform half ASP_GlobalTintNoiseIntensity;
				uniform float4 _BottomColor;
				uniform float _BottomColorOffset;
				uniform float _BottomColorContrast;
				uniform sampler2D _BaseNormal;
				uniform float _BaseNormalIntensity;
				uniform float _BaseSmoothnessIntensity;
				uniform float _BaseSmoothnessWaves;
				uniform float _BottomDitherOffset;
				uniform float _BottomDitherContrast;
				uniform float _BaseOpacityCutoff;


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
				
				float4 ASEScreenPositionNormalizedToPixel( float4 screenPosNorm, float4 screenParams )
				{
					float4 screenPosPixel = screenPosNorm * float4( screenParams.xy, 1, 1 );
					#if UNITY_UV_STARTS_AT_TOP
						screenPosPixel.xy = float2( screenPosPixel.x, ( _ProjectionParams.x < 0 ) ? screenParams.y - screenPosPixel.y : screenPosPixel.y );
					#else
						screenPosPixel.xy = float2( screenPosPixel.x, ( _ProjectionParams.x > 0 ) ? screenParams.y - screenPosPixel.y : screenPosPixel.y );
					#endif
					return screenPosPixel;
				}
				
				inline float Dither4x4Bayer( uint x, uint y )
				{
					const float dither[ 16 ] = {
					     1,  9,  3, 11,
					    13,  5, 15,  7,
					     4, 12,  2, 10,
					    16,  8, 14,  6 };
					uint r = y * 4 + x;
					return dither[ min( r, 15 ) ] / 16; // same # of instructions as pre-dividing due to compiler magic
				}
				

				v2f VertexFunction (appdata v  ) {
					UNITY_SETUP_INSTANCE_ID(v);
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f,o);
					UNITY_TRANSFER_INSTANCE_ID(v,o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					float3 temp_cast_0 = (0.0).xxx;
					float3 Global_Wind_Direction238_g1 = ASPW_WindDirection;
					float3 normalizeResult41_g1 = ASESafeNormalize( Global_Wind_Direction238_g1 );
					float3 worldToObjDir40_g1 = mul( unity_WorldToObject, float4( normalizeResult41_g1, 0.0 ) ).xyz;
					float3 Wind_Direction_Leaf50_g1 = worldToObjDir40_g1;
					float3 break42_g1 = Wind_Direction_Leaf50_g1;
					float3 appendResult43_g1 = (float3(break42_g1.z , 0.0 , ( break42_g1.x * -1.0 )));
					float3 Wind_Direction52_g1 = appendResult43_g1;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Grass_Randomization65_g1 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_5_0_g1 = ( Wind_Grass_Randomization65_g1 + _Time.y );
					float Global_Wind_Grass_Speed144_g1 = ASPW_WindGrassSpeed;
					float temp_output_9_0_g1 = ( _WindGrassSpeed * Global_Wind_Grass_Speed144_g1 * 10.0 );
					float Local_Wind_Grass_Aplitude180_g1 = _WindGrassAmplitude;
					float Global_Wind_Grass_Amplitude172_g1 = ASPW_WindGrassAmplitude;
					float temp_output_27_0_g1 = ( Local_Wind_Grass_Aplitude180_g1 * Global_Wind_Grass_Amplitude172_g1 );
					float Global_Wind_Grass_Flexibility164_g1 = ASPW_WindGrassFlexibility;
					float Wind_Main31_g1 = ( ( ( ( ( sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.0 ) ) ) + sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.25 ) ) ) + sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.5 ) ) ) ) + temp_output_27_0_g1 ) * temp_output_27_0_g1 ) / 50.0 ) * ( ( Global_Wind_Grass_Flexibility164_g1 * _WindGrassFlexibility ) * 0.1 ) );
					float Global_Wind_Grass_Waves_Amplitude162_g1 = ASPW_WindGrassWavesAmplitude;
					float3 appendResult119_g1 = (float3(( normalizeResult41_g1.x * -1.0 ) , ( 0.0 * -1.0 ) , 0.0));
					float3 Wind_Direction_Waves118_g1 = appendResult119_g1;
					float Global_Wind_Grass_Waves_Speed166_g1 = ASPW_WindGrassWavesSpeed;
					float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( v.vertex ).xyz, 1 ) ).xyz;
					float2 appendResult73_g1 = (float2(ase_positionWS.x , ase_positionWS.z));
					float Global_Wind_Grass_Waves_Scale168_g1 = ASPW_WindGrassWavesScale;
					float Wind_Waves93_g1 = tex2Dlod( ASPW_WindGrassWavesNoiseTexture, float4( ( ( ( Wind_Direction_Waves118_g1 * _Time.y ) * ( Global_Wind_Grass_Waves_Speed166_g1 * 0.1 ) ) + float3( ( ( appendResult73_g1 * Global_Wind_Grass_Waves_Scale168_g1 ) * 0.1 ) ,  0.0 ) ).xy, 0, 0.0) ).r;
					float lerpResult96_g1 = lerp( Wind_Main31_g1 , ( Wind_Main31_g1 * Global_Wind_Grass_Waves_Amplitude162_g1 ) , Wind_Waves93_g1);
					float Wind_Main_with_Waves108_g1 = lerpResult96_g1;
					float temp_output_141_0_g1 = ( ( ase_positionWS.y * ( _WindGrassScale * 10.0 ) ) + _Time.y );
					float Local_Wind_Grass_Speed186_g1 = _WindGrassSpeed;
					float temp_output_146_0_g1 = ( Global_Wind_Grass_Speed144_g1 * Local_Wind_Grass_Speed186_g1 * 10.0 );
					float Global_Wind_Grass_Turbulence161_g1 = ASPW_WindGrassTurbulence;
					float clampResult175_g1 = clamp( Global_Wind_Grass_Amplitude172_g1 , 0.0 , 1.0 );
					float temp_output_188_0_g1 = ( ( ( sin( ( temp_output_141_0_g1 * ( temp_output_146_0_g1 * 1.0 ) ) ) + sin( ( temp_output_141_0_g1 * ( temp_output_146_0_g1 * 1.25 ) ) ) ) * 0.5 ) * ( ( Global_Wind_Grass_Turbulence161_g1 * ( clampResult175_g1 * ( _WindGrassTurbulence * Local_Wind_Grass_Aplitude180_g1 ) ) ) * 0.1 ) );
					float3 appendResult183_g1 = (float3(temp_output_188_0_g1 , ( temp_output_188_0_g1 * 0.2 ) , temp_output_188_0_g1));
					float3 Wind_Turbulence185_g1 = appendResult183_g1;
					float3 rotatedValue56_g1 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction52_g1, ( Wind_Main_with_Waves108_g1 + Wind_Turbulence185_g1 ).x );
					float3 Output_Wind35_g1 = ( rotatedValue56_g1 - v.vertex.xyz );
					float Wind_Mask225_g1 = v.ase_color.r;
					float3 lerpResult232_g1 = lerp( float3( 0,0,0 ) , ( Output_Wind35_g1 * Wind_Mask225_g1 ) , ASPW_WindToggle);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch192_g1 = lerpResult232_g1;
					#else
					float3 staticSwitch192_g1 = temp_cast_0;
					#endif
					
					o.ase_texcoord5.xy = v.texcoord.xyzw.xy;
					o.ase_color = v.ase_color;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord5.zw = 0;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch192_g1;
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

				half4 frag ( v2f IN 
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

					float2 uv_BaseAlbedo280 = IN.ase_texcoord5.xy;
					float4 tex2DNode280 = tex2D( _BaseAlbedo, uv_BaseAlbedo280 );
					float3 desaturateInitialColor281 = tex2DNode280.rgb;
					float desaturateDot281 = dot( desaturateInitialColor281, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar281 = lerp( desaturateInitialColor281, desaturateDot281.xxx, _BaseAlbedoDesaturation );
					float3 Albedo_Texture299 = saturate( ( desaturateVar281 * _BaseAlbedoBrightness ) );
					float3 blendOpSrc283 = Albedo_Texture299;
					float3 blendOpDest283 = _BaseAlbedoColor.rgb;
					float3 Base_Albedo289 = (( blendOpDest283 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest283 ) * ( 1.0 - blendOpSrc283 ) ) : ( 2.0 * blendOpDest283 * blendOpSrc283 ) );
					float3 blendOpSrc345 = Albedo_Texture299;
					float3 blendOpDest345 = _TintColor.rgb;
					float2 appendResult649 = (float2(PositionWS.x , PositionWS.z));
					float4 tex2DNode651 = tex2D( ASP_GlobalTintNoiseTexture, ( appendResult649 * ( 0.01 * ASP_GlobalTintNoiseUVScale * _TintNoiseUVScale ) ) );
					float lerpResult676 = lerp( tex2DNode651.r , ( 1.0 - tex2DNode651.r ) , _TintNoiseInvertMask);
					float Base_Tint_Color_Mask659 = saturate( ( lerpResult676 * ( ASP_GlobalTintNoiseContrast * _TintNoiseContrast ) * IN.ase_color.r ) );
					float3 lerpResult656 = lerp( Base_Albedo289 , (( blendOpDest345 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest345 ) * ( 1.0 - blendOpSrc345 ) ) : ( 2.0 * blendOpDest345 * blendOpSrc345 ) ) , Base_Tint_Color_Mask659);
					float3 lerpResult661 = lerp( Base_Albedo289 , lerpResult656 , ( ASP_GlobalTintNoiseToggle * _TintNoiseIntensity * ASP_GlobalTintNoiseIntensity ));
					#ifdef _ENABLETINTVARIATIONCOLOR_ON
					float3 staticSwitch403 = lerpResult661;
					#else
					float3 staticSwitch403 = Base_Albedo289;
					#endif
					float3 Base_Albedo_and_Tint_Color374 = staticSwitch403;
					float3 blendOpSrc331 = Albedo_Texture299;
					float3 blendOpDest331 = _BottomColor.rgb;
					float saferPower336 = abs( ( 1.0 - IN.ase_color.a ) );
					float3 lerpResult340 = lerp( Base_Albedo_and_Tint_Color374 , (( blendOpDest331 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest331 ) * ( 1.0 - blendOpSrc331 ) ) : ( 2.0 * blendOpDest331 * blendOpSrc331 ) ) , saturate( ( _BottomColorOffset * pow( saferPower336 , _BottomColorContrast ) ) ));
					#ifdef _ENABLEBOTTOMCOLOR_ON
					float3 staticSwitch375 = lerpResult340;
					#else
					float3 staticSwitch375 = Base_Albedo_and_Tint_Color374;
					#endif
					float3 Output_Albedo342 = staticSwitch375;
					
					float2 uv_BaseNormal318 = IN.ase_texcoord5.xy;
					float3 Base_Normal642 = UnpackScaleNormal( tex2D( _BaseNormal, uv_BaseNormal318 ), _BaseNormalIntensity );
					
					float3 Global_Wind_Direction238_g1 = ASPW_WindDirection;
					float3 normalizeResult41_g1 = ASESafeNormalize( Global_Wind_Direction238_g1 );
					float3 appendResult119_g1 = (float3(( normalizeResult41_g1.x * -1.0 ) , ( 0.0 * -1.0 ) , 0.0));
					float3 Wind_Direction_Waves118_g1 = appendResult119_g1;
					float Global_Wind_Grass_Waves_Speed166_g1 = ASPW_WindGrassWavesSpeed;
					float2 appendResult73_g1 = (float2(PositionWS.x , PositionWS.z));
					float Global_Wind_Grass_Waves_Scale168_g1 = ASPW_WindGrassWavesScale;
					float Wind_Waves93_g1 = tex2D( ASPW_WindGrassWavesNoiseTexture, ( ( ( Wind_Direction_Waves118_g1 * _Time.y ) * ( Global_Wind_Grass_Waves_Speed166_g1 * 0.1 ) ) + float3( ( ( appendResult73_g1 * Global_Wind_Grass_Waves_Scale168_g1 ) * 0.1 ) ,  0.0 ) ).xy ).r;
					float Wind_Waves621 = Wind_Waves93_g1;
					float lerpResult304 = lerp( 0.0 , _BaseSmoothnessIntensity , ( IN.ase_color.r * ( Wind_Waves621 + _BaseSmoothnessWaves ) ));
					#ifdef _ENABLESMOOTHNESSWAVES_ON
					float staticSwitch719 = lerpResult304;
					#else
					float staticSwitch719 = _BaseSmoothnessIntensity;
					#endif
					float Base_Smoothness314 = saturate( staticSwitch719 );
					
					float Base_Opacity295 = tex2DNode280.a;
					float4 ase_positionSS_Pixel = ASEScreenPositionNormalizedToPixel( ScreenPosNorm, _ScreenParams );
					float dither728 = Dither4x4Bayer( fmod( ase_positionSS_Pixel.x, 4 ), fmod( ase_positionSS_Pixel.y, 4 ) );
					dither728 = step( dither728, saturate( saturate( ( ( IN.ase_color.r - _BottomDitherOffset ) * ( _BottomDitherContrast * 2 ) ) ) * 1.00001 ) );
					#ifdef _ENABLEBOTTOMDITHER_ON
					float staticSwitch731 = ( dither728 * Base_Opacity295 );
					#else
					float staticSwitch731 = Base_Opacity295;
					#endif
					float Output_Opacity732 = staticSwitch731;
					

					o.Albedo = Output_Albedo342;
					o.Normal = Base_Normal642;

					half3 Specular = half3( 0, 0, 0 );
					half Metallic = 0;
					half Smoothness = Base_Smoothness314;
					half Occlusion = 1;

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

					o.Emission = half3( 0, 0, 0 );
					o.Alpha = Output_Opacity732;
					half AlphaClipThreshold = _BaseOpacityCutoff;
					half3 Transmission = 1;
					half3 Translucency = 1;

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
						half shadow = _TransShadow;
						half normal = _TransNormal;
						half scattering = _TransScattering;
						half direct = _TransDirect;
						half ambient = _TransAmbient;
						half strength = _TransStrength;

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
			
			Name "Deferred"
			Tags { "LightMode"="Deferred" }

			AlphaToMask Off

			CGPROGRAM
				#define ASE_GEOMETRY 1
				#define ASE_FRAGMENT_NORMAL 0
				#define ASE_RECEIVE_SHADOWS
				#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
				#pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF
				#pragma multi_compile_instancing
				#pragma multi_compile _ LOD_FADE_CROSSFADE
				#define ASE_FOG
				#pragma multi_compile_local _ALPHATEST_ON
				#define ASE_VERSION 19903

				#pragma vertex vert
				#pragma fragment frag
				#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
				#pragma multi_compile_prepassfinal
				#ifndef UNITY_PASS_DEFERRED
					#define UNITY_PASS_DEFERRED
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

				#if defined(UNITY_INSTANCING_ENABLED) && ( defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL) || defined(_INSTANCEDTERRAINNORMALS_PIXEL) )
					#define ENABLE_TERRAIN_PERPIXEL_NORMAL
				#endif

				#include "UnityStandardUtils.cginc"
				#define ASE_NEEDS_VERT_POSITION
				#define ASE_NEEDS_TEXTURE_COORDINATES0
				#define ASE_NEEDS_WORLD_POSITION
				#define ASE_NEEDS_FRAG_WORLD_POSITION
				#define ASE_NEEDS_FRAG_COLOR
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLEBOTTOMCOLOR_ON
				#pragma shader_feature_local _ENABLETINTVARIATIONCOLOR_ON
				#pragma shader_feature_local _ENABLESMOOTHNESSWAVES_ON
				#pragma shader_feature_local _ENABLEBOTTOMDITHER_ON


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
					float4 ase_texcoord4 : TEXCOORD4;
					float4 ase_color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				#ifdef LIGHTMAP_ON
				float4 unity_LightmapFade;
				#endif
				half4 unity_Ambient;
				#ifdef ASE_TESSELLATION
					float _TessPhongStrength;
					float _TessValue;
					float _TessMin;
					float _TessMax;
					float _TessEdgeLength;
					float _TessMaxDisp;
				#endif

				uniform sampler2D ASPW_WindGrassWavesNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform float _WindGrassSpeed;
				uniform half ASPW_WindGrassSpeed;
				uniform float _WindGrassAmplitude;
				uniform half ASPW_WindGrassAmplitude;
				uniform half ASPW_WindGrassFlexibility;
				uniform float _WindGrassFlexibility;
				uniform half ASPW_WindGrassWavesAmplitude;
				uniform half ASPW_WindGrassWavesSpeed;
				uniform half ASPW_WindGrassWavesScale;
				uniform float _WindGrassScale;
				uniform half ASPW_WindGrassTurbulence;
				uniform float _WindGrassTurbulence;
				uniform half ASPW_WindToggle;
				uniform sampler2D _BaseAlbedo;
				uniform float _BaseAlbedoDesaturation;
				uniform float _BaseAlbedoBrightness;
				uniform float4 _BaseAlbedoColor;
				uniform float4 _TintColor;
				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half ASP_GlobalTintNoiseUVScale;
				uniform float _TintNoiseUVScale;
				uniform float _TintNoiseInvertMask;
				uniform half ASP_GlobalTintNoiseContrast;
				uniform float _TintNoiseContrast;
				uniform half ASP_GlobalTintNoiseToggle;
				uniform float _TintNoiseIntensity;
				uniform half ASP_GlobalTintNoiseIntensity;
				uniform float4 _BottomColor;
				uniform float _BottomColorOffset;
				uniform float _BottomColorContrast;
				uniform sampler2D _BaseNormal;
				uniform float _BaseNormalIntensity;
				uniform float _BaseSmoothnessIntensity;
				uniform float _BaseSmoothnessWaves;
				uniform float _BottomDitherOffset;
				uniform float _BottomDitherContrast;
				uniform float _BaseOpacityCutoff;


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
				
				float4 ASEScreenPositionNormalizedToPixel( float4 screenPosNorm, float4 screenParams )
				{
					float4 screenPosPixel = screenPosNorm * float4( screenParams.xy, 1, 1 );
					#if UNITY_UV_STARTS_AT_TOP
						screenPosPixel.xy = float2( screenPosPixel.x, ( _ProjectionParams.x < 0 ) ? screenParams.y - screenPosPixel.y : screenPosPixel.y );
					#else
						screenPosPixel.xy = float2( screenPosPixel.x, ( _ProjectionParams.x > 0 ) ? screenParams.y - screenPosPixel.y : screenPosPixel.y );
					#endif
					return screenPosPixel;
				}
				
				inline float Dither4x4Bayer( uint x, uint y )
				{
					const float dither[ 16 ] = {
					     1,  9,  3, 11,
					    13,  5, 15,  7,
					     4, 12,  2, 10,
					    16,  8, 14,  6 };
					uint r = y * 4 + x;
					return dither[ min( r, 15 ) ] / 16; // same # of instructions as pre-dividing due to compiler magic
				}
				

				v2f VertexFunction (appdata v  ) {
					UNITY_SETUP_INSTANCE_ID(v);
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f,o);
					UNITY_TRANSFER_INSTANCE_ID(v,o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					float3 temp_cast_0 = (0.0).xxx;
					float3 Global_Wind_Direction238_g1 = ASPW_WindDirection;
					float3 normalizeResult41_g1 = ASESafeNormalize( Global_Wind_Direction238_g1 );
					float3 worldToObjDir40_g1 = mul( unity_WorldToObject, float4( normalizeResult41_g1, 0.0 ) ).xyz;
					float3 Wind_Direction_Leaf50_g1 = worldToObjDir40_g1;
					float3 break42_g1 = Wind_Direction_Leaf50_g1;
					float3 appendResult43_g1 = (float3(break42_g1.z , 0.0 , ( break42_g1.x * -1.0 )));
					float3 Wind_Direction52_g1 = appendResult43_g1;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Grass_Randomization65_g1 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_5_0_g1 = ( Wind_Grass_Randomization65_g1 + _Time.y );
					float Global_Wind_Grass_Speed144_g1 = ASPW_WindGrassSpeed;
					float temp_output_9_0_g1 = ( _WindGrassSpeed * Global_Wind_Grass_Speed144_g1 * 10.0 );
					float Local_Wind_Grass_Aplitude180_g1 = _WindGrassAmplitude;
					float Global_Wind_Grass_Amplitude172_g1 = ASPW_WindGrassAmplitude;
					float temp_output_27_0_g1 = ( Local_Wind_Grass_Aplitude180_g1 * Global_Wind_Grass_Amplitude172_g1 );
					float Global_Wind_Grass_Flexibility164_g1 = ASPW_WindGrassFlexibility;
					float Wind_Main31_g1 = ( ( ( ( ( sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.0 ) ) ) + sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.25 ) ) ) + sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.5 ) ) ) ) + temp_output_27_0_g1 ) * temp_output_27_0_g1 ) / 50.0 ) * ( ( Global_Wind_Grass_Flexibility164_g1 * _WindGrassFlexibility ) * 0.1 ) );
					float Global_Wind_Grass_Waves_Amplitude162_g1 = ASPW_WindGrassWavesAmplitude;
					float3 appendResult119_g1 = (float3(( normalizeResult41_g1.x * -1.0 ) , ( 0.0 * -1.0 ) , 0.0));
					float3 Wind_Direction_Waves118_g1 = appendResult119_g1;
					float Global_Wind_Grass_Waves_Speed166_g1 = ASPW_WindGrassWavesSpeed;
					float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( v.vertex ).xyz, 1 ) ).xyz;
					float2 appendResult73_g1 = (float2(ase_positionWS.x , ase_positionWS.z));
					float Global_Wind_Grass_Waves_Scale168_g1 = ASPW_WindGrassWavesScale;
					float Wind_Waves93_g1 = tex2Dlod( ASPW_WindGrassWavesNoiseTexture, float4( ( ( ( Wind_Direction_Waves118_g1 * _Time.y ) * ( Global_Wind_Grass_Waves_Speed166_g1 * 0.1 ) ) + float3( ( ( appendResult73_g1 * Global_Wind_Grass_Waves_Scale168_g1 ) * 0.1 ) ,  0.0 ) ).xy, 0, 0.0) ).r;
					float lerpResult96_g1 = lerp( Wind_Main31_g1 , ( Wind_Main31_g1 * Global_Wind_Grass_Waves_Amplitude162_g1 ) , Wind_Waves93_g1);
					float Wind_Main_with_Waves108_g1 = lerpResult96_g1;
					float temp_output_141_0_g1 = ( ( ase_positionWS.y * ( _WindGrassScale * 10.0 ) ) + _Time.y );
					float Local_Wind_Grass_Speed186_g1 = _WindGrassSpeed;
					float temp_output_146_0_g1 = ( Global_Wind_Grass_Speed144_g1 * Local_Wind_Grass_Speed186_g1 * 10.0 );
					float Global_Wind_Grass_Turbulence161_g1 = ASPW_WindGrassTurbulence;
					float clampResult175_g1 = clamp( Global_Wind_Grass_Amplitude172_g1 , 0.0 , 1.0 );
					float temp_output_188_0_g1 = ( ( ( sin( ( temp_output_141_0_g1 * ( temp_output_146_0_g1 * 1.0 ) ) ) + sin( ( temp_output_141_0_g1 * ( temp_output_146_0_g1 * 1.25 ) ) ) ) * 0.5 ) * ( ( Global_Wind_Grass_Turbulence161_g1 * ( clampResult175_g1 * ( _WindGrassTurbulence * Local_Wind_Grass_Aplitude180_g1 ) ) ) * 0.1 ) );
					float3 appendResult183_g1 = (float3(temp_output_188_0_g1 , ( temp_output_188_0_g1 * 0.2 ) , temp_output_188_0_g1));
					float3 Wind_Turbulence185_g1 = appendResult183_g1;
					float3 rotatedValue56_g1 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction52_g1, ( Wind_Main_with_Waves108_g1 + Wind_Turbulence185_g1 ).x );
					float3 Output_Wind35_g1 = ( rotatedValue56_g1 - v.vertex.xyz );
					float Wind_Mask225_g1 = v.ase_color.r;
					float3 lerpResult232_g1 = lerp( float3( 0,0,0 ) , ( Output_Wind35_g1 * Wind_Mask225_g1 ) , ASPW_WindToggle);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch192_g1 = lerpResult232_g1;
					#else
					float3 staticSwitch192_g1 = temp_cast_0;
					#endif
					
					o.ase_texcoord4.xy = v.texcoord.xyzw.xy;
					o.ase_color = v.ase_color;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord4.zw = 0;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch192_g1;
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

				void frag (v2f IN 
					, out half4 outGBuffer0 : SV_Target0
					, out half4 outGBuffer1 : SV_Target1
					, out half4 outGBuffer2 : SV_Target2
					, out half4 outEmission : SV_Target3
					#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
					, out half4 outShadowMask : SV_Target4
					#endif
					#if defined( ASE_DEPTH_WRITE_ON )
					, out float outputDepth : SV_Depth
					#endif
				)
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

					float3 PositionWS = IN.positionWS.xyz;
					half3 ViewDirWS = normalize( UnityWorldSpaceViewDir( PositionWS ) );
					float4 ScreenPosNorm = float4( IN.pos.xy * ( _ScreenParams.zw - 1.0 ), IN.pos.zw );
					float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, IN.pos.z ) * IN.pos.w;
					float4 ScreenPos = ComputeScreenPos( ClipPos );
					half3 NormalWS = IN.normalWS;
					half3 TangentWS = IN.tangentWS.xyz;
					half3 BitangentWS = cross( IN.normalWS, IN.tangentWS.xyz ) * IN.tangentWS.w * unity_WorldTransformParams.w;

					#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
						float2 sampleCoords = (IN.tangentWS.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
						NormalWS = UnityObjectToWorldNormal(normalize(tex2D(_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
						TangentWS = -cross(unity_ObjectToWorld._13_23_33, NormalWS);
						BitangentWS = cross(NormalWS, -TangentWS);
					#endif

					float2 uv_BaseAlbedo280 = IN.ase_texcoord4.xy;
					float4 tex2DNode280 = tex2D( _BaseAlbedo, uv_BaseAlbedo280 );
					float3 desaturateInitialColor281 = tex2DNode280.rgb;
					float desaturateDot281 = dot( desaturateInitialColor281, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar281 = lerp( desaturateInitialColor281, desaturateDot281.xxx, _BaseAlbedoDesaturation );
					float3 Albedo_Texture299 = saturate( ( desaturateVar281 * _BaseAlbedoBrightness ) );
					float3 blendOpSrc283 = Albedo_Texture299;
					float3 blendOpDest283 = _BaseAlbedoColor.rgb;
					float3 Base_Albedo289 = (( blendOpDest283 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest283 ) * ( 1.0 - blendOpSrc283 ) ) : ( 2.0 * blendOpDest283 * blendOpSrc283 ) );
					float3 blendOpSrc345 = Albedo_Texture299;
					float3 blendOpDest345 = _TintColor.rgb;
					float2 appendResult649 = (float2(PositionWS.x , PositionWS.z));
					float4 tex2DNode651 = tex2D( ASP_GlobalTintNoiseTexture, ( appendResult649 * ( 0.01 * ASP_GlobalTintNoiseUVScale * _TintNoiseUVScale ) ) );
					float lerpResult676 = lerp( tex2DNode651.r , ( 1.0 - tex2DNode651.r ) , _TintNoiseInvertMask);
					float Base_Tint_Color_Mask659 = saturate( ( lerpResult676 * ( ASP_GlobalTintNoiseContrast * _TintNoiseContrast ) * IN.ase_color.r ) );
					float3 lerpResult656 = lerp( Base_Albedo289 , (( blendOpDest345 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest345 ) * ( 1.0 - blendOpSrc345 ) ) : ( 2.0 * blendOpDest345 * blendOpSrc345 ) ) , Base_Tint_Color_Mask659);
					float3 lerpResult661 = lerp( Base_Albedo289 , lerpResult656 , ( ASP_GlobalTintNoiseToggle * _TintNoiseIntensity * ASP_GlobalTintNoiseIntensity ));
					#ifdef _ENABLETINTVARIATIONCOLOR_ON
					float3 staticSwitch403 = lerpResult661;
					#else
					float3 staticSwitch403 = Base_Albedo289;
					#endif
					float3 Base_Albedo_and_Tint_Color374 = staticSwitch403;
					float3 blendOpSrc331 = Albedo_Texture299;
					float3 blendOpDest331 = _BottomColor.rgb;
					float saferPower336 = abs( ( 1.0 - IN.ase_color.a ) );
					float3 lerpResult340 = lerp( Base_Albedo_and_Tint_Color374 , (( blendOpDest331 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest331 ) * ( 1.0 - blendOpSrc331 ) ) : ( 2.0 * blendOpDest331 * blendOpSrc331 ) ) , saturate( ( _BottomColorOffset * pow( saferPower336 , _BottomColorContrast ) ) ));
					#ifdef _ENABLEBOTTOMCOLOR_ON
					float3 staticSwitch375 = lerpResult340;
					#else
					float3 staticSwitch375 = Base_Albedo_and_Tint_Color374;
					#endif
					float3 Output_Albedo342 = staticSwitch375;
					
					float2 uv_BaseNormal318 = IN.ase_texcoord4.xy;
					float3 Base_Normal642 = UnpackScaleNormal( tex2D( _BaseNormal, uv_BaseNormal318 ), _BaseNormalIntensity );
					
					float3 Global_Wind_Direction238_g1 = ASPW_WindDirection;
					float3 normalizeResult41_g1 = ASESafeNormalize( Global_Wind_Direction238_g1 );
					float3 appendResult119_g1 = (float3(( normalizeResult41_g1.x * -1.0 ) , ( 0.0 * -1.0 ) , 0.0));
					float3 Wind_Direction_Waves118_g1 = appendResult119_g1;
					float Global_Wind_Grass_Waves_Speed166_g1 = ASPW_WindGrassWavesSpeed;
					float2 appendResult73_g1 = (float2(PositionWS.x , PositionWS.z));
					float Global_Wind_Grass_Waves_Scale168_g1 = ASPW_WindGrassWavesScale;
					float Wind_Waves93_g1 = tex2D( ASPW_WindGrassWavesNoiseTexture, ( ( ( Wind_Direction_Waves118_g1 * _Time.y ) * ( Global_Wind_Grass_Waves_Speed166_g1 * 0.1 ) ) + float3( ( ( appendResult73_g1 * Global_Wind_Grass_Waves_Scale168_g1 ) * 0.1 ) ,  0.0 ) ).xy ).r;
					float Wind_Waves621 = Wind_Waves93_g1;
					float lerpResult304 = lerp( 0.0 , _BaseSmoothnessIntensity , ( IN.ase_color.r * ( Wind_Waves621 + _BaseSmoothnessWaves ) ));
					#ifdef _ENABLESMOOTHNESSWAVES_ON
					float staticSwitch719 = lerpResult304;
					#else
					float staticSwitch719 = _BaseSmoothnessIntensity;
					#endif
					float Base_Smoothness314 = saturate( staticSwitch719 );
					
					float Base_Opacity295 = tex2DNode280.a;
					float4 ase_positionSS_Pixel = ASEScreenPositionNormalizedToPixel( ScreenPosNorm, _ScreenParams );
					float dither728 = Dither4x4Bayer( fmod( ase_positionSS_Pixel.x, 4 ), fmod( ase_positionSS_Pixel.y, 4 ) );
					dither728 = step( dither728, saturate( saturate( ( ( IN.ase_color.r - _BottomDitherOffset ) * ( _BottomDitherContrast * 2 ) ) ) * 1.00001 ) );
					#ifdef _ENABLEBOTTOMDITHER_ON
					float staticSwitch731 = ( dither728 * Base_Opacity295 );
					#else
					float staticSwitch731 = Base_Opacity295;
					#endif
					float Output_Opacity732 = staticSwitch731;
					

					o.Albedo = Output_Albedo342;
					o.Normal = Base_Normal642;

					half3 Specular = half3( 0, 0, 0 );
					half Metallic = 0;
					half Smoothness = Base_Smoothness314;
					half Occlusion = 1;

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

					o.Emission = half3( 0, 0, 0 );
					o.Alpha = Output_Opacity732;
					half AlphaClipThreshold = _BaseOpacityCutoff;
					half3 BakedGI = 0;

					#if defined( ASE_DEPTH_WRITE_ON )
						float DeviceDepth = IN.pos.z;
					#endif

					#if ( ASE_FRAGMENT_NORMAL == 0 )
						o.Normal = normalize( o.Normal.x * TangentWS + o.Normal.y * BitangentWS + o.Normal.z * NormalWS );
					#elif ( ASE_FRAGMENT_NORMAL == 1 )
						o.Normal = UnityObjectToWorldNormal( o.Normal );
					#elif ( ASE_FRAGMENT_NORMAL == 2 )
						// @diogo: already in world-space; do nothing
					#endif

					#ifdef _ALPHATEST_ON
						clip( o.Alpha - AlphaClipThreshold );
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
					gi.light.color = 0;
					gi.light.dir = half3( 0, 1, 0 );

					UnityGIInput giInput;
					UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
					giInput.light = gi.light;
					giInput.worldPos = PositionWS;
					giInput.worldViewDir = ViewDirWS;
					giInput.atten = 1;
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

					#if defined(ASE_LIGHTING_SIMPLE)
						#if defined(_SPECULAR_SETUP)
							outEmission = LightingBlinnPhong_Deferred( o, ViewDirWS, gi, outGBuffer0, outGBuffer1, outGBuffer2 );
						#else
							outEmission = LightingLambert_Deferred( o, gi, outGBuffer0, outGBuffer1, outGBuffer2 );
						#endif
					#else
						#if defined(_SPECULAR_SETUP)
							outEmission = LightingStandardSpecular_Deferred( o, ViewDirWS, gi, outGBuffer0, outGBuffer1, outGBuffer2 );
						#else
							outEmission = LightingStandard_Deferred( o, ViewDirWS, gi, outGBuffer0, outGBuffer1, outGBuffer2 );
						#endif
					#endif

					#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
						outShadowMask = UnityGetRawBakedOcclusions( IN.ambientOrLightmapUV.xy, float3( 0, 0, 0 ) );
					#endif
					#ifndef UNITY_HDR_ON
						outEmission.rgb = exp2(-outEmission.rgb);
					#endif
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
				#pragma multi_compile _ LOD_FADE_CROSSFADE
				#define ASE_FOG
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

				#define ASE_NEEDS_VERT_POSITION
				#define ASE_NEEDS_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_COLOR
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLEBOTTOMCOLOR_ON
				#pragma shader_feature_local _ENABLETINTVARIATIONCOLOR_ON
				#pragma shader_feature_local _ENABLEBOTTOMDITHER_ON


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
					float4 ase_texcoord3 : TEXCOORD3;
					float4 ase_color : COLOR;
					float4 ase_texcoord4 : TEXCOORD4;
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

				uniform sampler2D ASPW_WindGrassWavesNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform float _WindGrassSpeed;
				uniform half ASPW_WindGrassSpeed;
				uniform float _WindGrassAmplitude;
				uniform half ASPW_WindGrassAmplitude;
				uniform half ASPW_WindGrassFlexibility;
				uniform float _WindGrassFlexibility;
				uniform half ASPW_WindGrassWavesAmplitude;
				uniform half ASPW_WindGrassWavesSpeed;
				uniform half ASPW_WindGrassWavesScale;
				uniform float _WindGrassScale;
				uniform half ASPW_WindGrassTurbulence;
				uniform float _WindGrassTurbulence;
				uniform half ASPW_WindToggle;
				uniform sampler2D _BaseAlbedo;
				uniform float _BaseAlbedoDesaturation;
				uniform float _BaseAlbedoBrightness;
				uniform float4 _BaseAlbedoColor;
				uniform float4 _TintColor;
				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half ASP_GlobalTintNoiseUVScale;
				uniform float _TintNoiseUVScale;
				uniform float _TintNoiseInvertMask;
				uniform half ASP_GlobalTintNoiseContrast;
				uniform float _TintNoiseContrast;
				uniform half ASP_GlobalTintNoiseToggle;
				uniform float _TintNoiseIntensity;
				uniform half ASP_GlobalTintNoiseIntensity;
				uniform float4 _BottomColor;
				uniform float _BottomColorOffset;
				uniform float _BottomColorContrast;
				uniform float _BottomDitherOffset;
				uniform float _BottomDitherContrast;
				uniform float _BaseOpacityCutoff;


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
				
				float4 ASEScreenPositionNormalizedToPixel( float4 screenPosNorm, float4 screenParams )
				{
					float4 screenPosPixel = screenPosNorm * float4( screenParams.xy, 1, 1 );
					#if UNITY_UV_STARTS_AT_TOP
						screenPosPixel.xy = float2( screenPosPixel.x, ( _ProjectionParams.x < 0 ) ? screenParams.y - screenPosPixel.y : screenPosPixel.y );
					#else
						screenPosPixel.xy = float2( screenPosPixel.x, ( _ProjectionParams.x > 0 ) ? screenParams.y - screenPosPixel.y : screenPosPixel.y );
					#endif
					return screenPosPixel;
				}
				
				inline float Dither4x4Bayer( uint x, uint y )
				{
					const float dither[ 16 ] = {
					     1,  9,  3, 11,
					    13,  5, 15,  7,
					     4, 12,  2, 10,
					    16,  8, 14,  6 };
					uint r = y * 4 + x;
					return dither[ min( r, 15 ) ] / 16; // same # of instructions as pre-dividing due to compiler magic
				}
				

				v2f VertexFunction( appdata v  )
				{
					UNITY_SETUP_INSTANCE_ID(v);
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f,o);
					UNITY_TRANSFER_INSTANCE_ID(v,o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					float3 temp_cast_0 = (0.0).xxx;
					float3 Global_Wind_Direction238_g1 = ASPW_WindDirection;
					float3 normalizeResult41_g1 = ASESafeNormalize( Global_Wind_Direction238_g1 );
					float3 worldToObjDir40_g1 = mul( unity_WorldToObject, float4( normalizeResult41_g1, 0.0 ) ).xyz;
					float3 Wind_Direction_Leaf50_g1 = worldToObjDir40_g1;
					float3 break42_g1 = Wind_Direction_Leaf50_g1;
					float3 appendResult43_g1 = (float3(break42_g1.z , 0.0 , ( break42_g1.x * -1.0 )));
					float3 Wind_Direction52_g1 = appendResult43_g1;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Grass_Randomization65_g1 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_5_0_g1 = ( Wind_Grass_Randomization65_g1 + _Time.y );
					float Global_Wind_Grass_Speed144_g1 = ASPW_WindGrassSpeed;
					float temp_output_9_0_g1 = ( _WindGrassSpeed * Global_Wind_Grass_Speed144_g1 * 10.0 );
					float Local_Wind_Grass_Aplitude180_g1 = _WindGrassAmplitude;
					float Global_Wind_Grass_Amplitude172_g1 = ASPW_WindGrassAmplitude;
					float temp_output_27_0_g1 = ( Local_Wind_Grass_Aplitude180_g1 * Global_Wind_Grass_Amplitude172_g1 );
					float Global_Wind_Grass_Flexibility164_g1 = ASPW_WindGrassFlexibility;
					float Wind_Main31_g1 = ( ( ( ( ( sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.0 ) ) ) + sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.25 ) ) ) + sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.5 ) ) ) ) + temp_output_27_0_g1 ) * temp_output_27_0_g1 ) / 50.0 ) * ( ( Global_Wind_Grass_Flexibility164_g1 * _WindGrassFlexibility ) * 0.1 ) );
					float Global_Wind_Grass_Waves_Amplitude162_g1 = ASPW_WindGrassWavesAmplitude;
					float3 appendResult119_g1 = (float3(( normalizeResult41_g1.x * -1.0 ) , ( 0.0 * -1.0 ) , 0.0));
					float3 Wind_Direction_Waves118_g1 = appendResult119_g1;
					float Global_Wind_Grass_Waves_Speed166_g1 = ASPW_WindGrassWavesSpeed;
					float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( v.vertex ).xyz, 1 ) ).xyz;
					float2 appendResult73_g1 = (float2(ase_positionWS.x , ase_positionWS.z));
					float Global_Wind_Grass_Waves_Scale168_g1 = ASPW_WindGrassWavesScale;
					float Wind_Waves93_g1 = tex2Dlod( ASPW_WindGrassWavesNoiseTexture, float4( ( ( ( Wind_Direction_Waves118_g1 * _Time.y ) * ( Global_Wind_Grass_Waves_Speed166_g1 * 0.1 ) ) + float3( ( ( appendResult73_g1 * Global_Wind_Grass_Waves_Scale168_g1 ) * 0.1 ) ,  0.0 ) ).xy, 0, 0.0) ).r;
					float lerpResult96_g1 = lerp( Wind_Main31_g1 , ( Wind_Main31_g1 * Global_Wind_Grass_Waves_Amplitude162_g1 ) , Wind_Waves93_g1);
					float Wind_Main_with_Waves108_g1 = lerpResult96_g1;
					float temp_output_141_0_g1 = ( ( ase_positionWS.y * ( _WindGrassScale * 10.0 ) ) + _Time.y );
					float Local_Wind_Grass_Speed186_g1 = _WindGrassSpeed;
					float temp_output_146_0_g1 = ( Global_Wind_Grass_Speed144_g1 * Local_Wind_Grass_Speed186_g1 * 10.0 );
					float Global_Wind_Grass_Turbulence161_g1 = ASPW_WindGrassTurbulence;
					float clampResult175_g1 = clamp( Global_Wind_Grass_Amplitude172_g1 , 0.0 , 1.0 );
					float temp_output_188_0_g1 = ( ( ( sin( ( temp_output_141_0_g1 * ( temp_output_146_0_g1 * 1.0 ) ) ) + sin( ( temp_output_141_0_g1 * ( temp_output_146_0_g1 * 1.25 ) ) ) ) * 0.5 ) * ( ( Global_Wind_Grass_Turbulence161_g1 * ( clampResult175_g1 * ( _WindGrassTurbulence * Local_Wind_Grass_Aplitude180_g1 ) ) ) * 0.1 ) );
					float3 appendResult183_g1 = (float3(temp_output_188_0_g1 , ( temp_output_188_0_g1 * 0.2 ) , temp_output_188_0_g1));
					float3 Wind_Turbulence185_g1 = appendResult183_g1;
					float3 rotatedValue56_g1 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction52_g1, ( Wind_Main_with_Waves108_g1 + Wind_Turbulence185_g1 ).x );
					float3 Output_Wind35_g1 = ( rotatedValue56_g1 - v.vertex.xyz );
					float Wind_Mask225_g1 = v.ase_color.r;
					float3 lerpResult232_g1 = lerp( float3( 0,0,0 ) , ( Output_Wind35_g1 * Wind_Mask225_g1 ) , ASPW_WindToggle);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch192_g1 = lerpResult232_g1;
					#else
					float3 staticSwitch192_g1 = temp_cast_0;
					#endif
					
					o.ase_texcoord3.xyz = ase_positionWS;
					
					float4 ase_positionCS = UnityObjectToClipPos( v.vertex );
					float4 screenPos = ComputeScreenPos( ase_positionCS );
					o.ase_texcoord4 = screenPos;
					
					o.ase_texcoord2.xy = v.texcoord.xyzw.xy;
					o.ase_color = v.ase_color;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord2.zw = 0;
					o.ase_texcoord3.w = 0;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch192_g1;
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

				half4 frag( v2f IN  ) : SV_Target
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

					float2 uv_BaseAlbedo280 = IN.ase_texcoord2.xy;
					float4 tex2DNode280 = tex2D( _BaseAlbedo, uv_BaseAlbedo280 );
					float3 desaturateInitialColor281 = tex2DNode280.rgb;
					float desaturateDot281 = dot( desaturateInitialColor281, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar281 = lerp( desaturateInitialColor281, desaturateDot281.xxx, _BaseAlbedoDesaturation );
					float3 Albedo_Texture299 = saturate( ( desaturateVar281 * _BaseAlbedoBrightness ) );
					float3 blendOpSrc283 = Albedo_Texture299;
					float3 blendOpDest283 = _BaseAlbedoColor.rgb;
					float3 Base_Albedo289 = (( blendOpDest283 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest283 ) * ( 1.0 - blendOpSrc283 ) ) : ( 2.0 * blendOpDest283 * blendOpSrc283 ) );
					float3 blendOpSrc345 = Albedo_Texture299;
					float3 blendOpDest345 = _TintColor.rgb;
					float3 ase_positionWS = IN.ase_texcoord3.xyz;
					float2 appendResult649 = (float2(ase_positionWS.x , ase_positionWS.z));
					float4 tex2DNode651 = tex2D( ASP_GlobalTintNoiseTexture, ( appendResult649 * ( 0.01 * ASP_GlobalTintNoiseUVScale * _TintNoiseUVScale ) ) );
					float lerpResult676 = lerp( tex2DNode651.r , ( 1.0 - tex2DNode651.r ) , _TintNoiseInvertMask);
					float Base_Tint_Color_Mask659 = saturate( ( lerpResult676 * ( ASP_GlobalTintNoiseContrast * _TintNoiseContrast ) * IN.ase_color.r ) );
					float3 lerpResult656 = lerp( Base_Albedo289 , (( blendOpDest345 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest345 ) * ( 1.0 - blendOpSrc345 ) ) : ( 2.0 * blendOpDest345 * blendOpSrc345 ) ) , Base_Tint_Color_Mask659);
					float3 lerpResult661 = lerp( Base_Albedo289 , lerpResult656 , ( ASP_GlobalTintNoiseToggle * _TintNoiseIntensity * ASP_GlobalTintNoiseIntensity ));
					#ifdef _ENABLETINTVARIATIONCOLOR_ON
					float3 staticSwitch403 = lerpResult661;
					#else
					float3 staticSwitch403 = Base_Albedo289;
					#endif
					float3 Base_Albedo_and_Tint_Color374 = staticSwitch403;
					float3 blendOpSrc331 = Albedo_Texture299;
					float3 blendOpDest331 = _BottomColor.rgb;
					float saferPower336 = abs( ( 1.0 - IN.ase_color.a ) );
					float3 lerpResult340 = lerp( Base_Albedo_and_Tint_Color374 , (( blendOpDest331 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest331 ) * ( 1.0 - blendOpSrc331 ) ) : ( 2.0 * blendOpDest331 * blendOpSrc331 ) ) , saturate( ( _BottomColorOffset * pow( saferPower336 , _BottomColorContrast ) ) ));
					#ifdef _ENABLEBOTTOMCOLOR_ON
					float3 staticSwitch375 = lerpResult340;
					#else
					float3 staticSwitch375 = Base_Albedo_and_Tint_Color374;
					#endif
					float3 Output_Albedo342 = staticSwitch375;
					
					float Base_Opacity295 = tex2DNode280.a;
					float4 screenPos = IN.ase_texcoord4;
					float4 ase_positionSSNorm = screenPos / screenPos.w;
					ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
					float4 ase_positionSS_Pixel = ASEScreenPositionNormalizedToPixel( ase_positionSSNorm, _ScreenParams );
					float dither728 = Dither4x4Bayer( fmod( ase_positionSS_Pixel.x, 4 ), fmod( ase_positionSS_Pixel.y, 4 ) );
					dither728 = step( dither728, saturate( saturate( ( ( IN.ase_color.r - _BottomDitherOffset ) * ( _BottomDitherContrast * 2 ) ) ) * 1.00001 ) );
					#ifdef _ENABLEBOTTOMDITHER_ON
					float staticSwitch731 = ( dither728 * Base_Opacity295 );
					#else
					float staticSwitch731 = Base_Opacity295;
					#endif
					float Output_Opacity732 = staticSwitch731;
					

					o.Albedo = Output_Albedo342;
					o.Normal = half3( 0, 0, 1 );
					o.Emission = half3( 0, 0, 0 );
					o.Alpha = Output_Opacity732;
					half AlphaClipThreshold = _BaseOpacityCutoff;

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
				#pragma multi_compile _ LOD_FADE_CROSSFADE
				#define ASE_FOG
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
				#define ASE_NEEDS_TEXTURE_COORDINATES0
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLEBOTTOMDITHER_ON


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
					float4 ase_color : COLOR;
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

				uniform sampler2D ASPW_WindGrassWavesNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform float _WindGrassSpeed;
				uniform half ASPW_WindGrassSpeed;
				uniform float _WindGrassAmplitude;
				uniform half ASPW_WindGrassAmplitude;
				uniform half ASPW_WindGrassFlexibility;
				uniform float _WindGrassFlexibility;
				uniform half ASPW_WindGrassWavesAmplitude;
				uniform half ASPW_WindGrassWavesSpeed;
				uniform half ASPW_WindGrassWavesScale;
				uniform float _WindGrassScale;
				uniform half ASPW_WindGrassTurbulence;
				uniform float _WindGrassTurbulence;
				uniform half ASPW_WindToggle;
				uniform sampler2D _BaseAlbedo;
				uniform float _BottomDitherOffset;
				uniform float _BottomDitherContrast;
				uniform float _BaseOpacityCutoff;


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
				
				float4 ASEScreenPositionNormalizedToPixel( float4 screenPosNorm, float4 screenParams )
				{
					float4 screenPosPixel = screenPosNorm * float4( screenParams.xy, 1, 1 );
					#if UNITY_UV_STARTS_AT_TOP
						screenPosPixel.xy = float2( screenPosPixel.x, ( _ProjectionParams.x < 0 ) ? screenParams.y - screenPosPixel.y : screenPosPixel.y );
					#else
						screenPosPixel.xy = float2( screenPosPixel.x, ( _ProjectionParams.x > 0 ) ? screenParams.y - screenPosPixel.y : screenPosPixel.y );
					#endif
					return screenPosPixel;
				}
				
				inline float Dither4x4Bayer( uint x, uint y )
				{
					const float dither[ 16 ] = {
					     1,  9,  3, 11,
					    13,  5, 15,  7,
					     4, 12,  2, 10,
					    16,  8, 14,  6 };
					uint r = y * 4 + x;
					return dither[ min( r, 15 ) ] / 16; // same # of instructions as pre-dividing due to compiler magic
				}
				

				v2f VertexFunction( appdata v  )
				{
					UNITY_SETUP_INSTANCE_ID(v);
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f,o);
					UNITY_TRANSFER_INSTANCE_ID(v,o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					float3 temp_cast_0 = (0.0).xxx;
					float3 Global_Wind_Direction238_g1 = ASPW_WindDirection;
					float3 normalizeResult41_g1 = ASESafeNormalize( Global_Wind_Direction238_g1 );
					float3 worldToObjDir40_g1 = mul( unity_WorldToObject, float4( normalizeResult41_g1, 0.0 ) ).xyz;
					float3 Wind_Direction_Leaf50_g1 = worldToObjDir40_g1;
					float3 break42_g1 = Wind_Direction_Leaf50_g1;
					float3 appendResult43_g1 = (float3(break42_g1.z , 0.0 , ( break42_g1.x * -1.0 )));
					float3 Wind_Direction52_g1 = appendResult43_g1;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Grass_Randomization65_g1 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_5_0_g1 = ( Wind_Grass_Randomization65_g1 + _Time.y );
					float Global_Wind_Grass_Speed144_g1 = ASPW_WindGrassSpeed;
					float temp_output_9_0_g1 = ( _WindGrassSpeed * Global_Wind_Grass_Speed144_g1 * 10.0 );
					float Local_Wind_Grass_Aplitude180_g1 = _WindGrassAmplitude;
					float Global_Wind_Grass_Amplitude172_g1 = ASPW_WindGrassAmplitude;
					float temp_output_27_0_g1 = ( Local_Wind_Grass_Aplitude180_g1 * Global_Wind_Grass_Amplitude172_g1 );
					float Global_Wind_Grass_Flexibility164_g1 = ASPW_WindGrassFlexibility;
					float Wind_Main31_g1 = ( ( ( ( ( sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.0 ) ) ) + sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.25 ) ) ) + sin( ( temp_output_5_0_g1 * ( temp_output_9_0_g1 * 1.5 ) ) ) ) + temp_output_27_0_g1 ) * temp_output_27_0_g1 ) / 50.0 ) * ( ( Global_Wind_Grass_Flexibility164_g1 * _WindGrassFlexibility ) * 0.1 ) );
					float Global_Wind_Grass_Waves_Amplitude162_g1 = ASPW_WindGrassWavesAmplitude;
					float3 appendResult119_g1 = (float3(( normalizeResult41_g1.x * -1.0 ) , ( 0.0 * -1.0 ) , 0.0));
					float3 Wind_Direction_Waves118_g1 = appendResult119_g1;
					float Global_Wind_Grass_Waves_Speed166_g1 = ASPW_WindGrassWavesSpeed;
					float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( v.vertex ).xyz, 1 ) ).xyz;
					float2 appendResult73_g1 = (float2(ase_positionWS.x , ase_positionWS.z));
					float Global_Wind_Grass_Waves_Scale168_g1 = ASPW_WindGrassWavesScale;
					float Wind_Waves93_g1 = tex2Dlod( ASPW_WindGrassWavesNoiseTexture, float4( ( ( ( Wind_Direction_Waves118_g1 * _Time.y ) * ( Global_Wind_Grass_Waves_Speed166_g1 * 0.1 ) ) + float3( ( ( appendResult73_g1 * Global_Wind_Grass_Waves_Scale168_g1 ) * 0.1 ) ,  0.0 ) ).xy, 0, 0.0) ).r;
					float lerpResult96_g1 = lerp( Wind_Main31_g1 , ( Wind_Main31_g1 * Global_Wind_Grass_Waves_Amplitude162_g1 ) , Wind_Waves93_g1);
					float Wind_Main_with_Waves108_g1 = lerpResult96_g1;
					float temp_output_141_0_g1 = ( ( ase_positionWS.y * ( _WindGrassScale * 10.0 ) ) + _Time.y );
					float Local_Wind_Grass_Speed186_g1 = _WindGrassSpeed;
					float temp_output_146_0_g1 = ( Global_Wind_Grass_Speed144_g1 * Local_Wind_Grass_Speed186_g1 * 10.0 );
					float Global_Wind_Grass_Turbulence161_g1 = ASPW_WindGrassTurbulence;
					float clampResult175_g1 = clamp( Global_Wind_Grass_Amplitude172_g1 , 0.0 , 1.0 );
					float temp_output_188_0_g1 = ( ( ( sin( ( temp_output_141_0_g1 * ( temp_output_146_0_g1 * 1.0 ) ) ) + sin( ( temp_output_141_0_g1 * ( temp_output_146_0_g1 * 1.25 ) ) ) ) * 0.5 ) * ( ( Global_Wind_Grass_Turbulence161_g1 * ( clampResult175_g1 * ( _WindGrassTurbulence * Local_Wind_Grass_Aplitude180_g1 ) ) ) * 0.1 ) );
					float3 appendResult183_g1 = (float3(temp_output_188_0_g1 , ( temp_output_188_0_g1 * 0.2 ) , temp_output_188_0_g1));
					float3 Wind_Turbulence185_g1 = appendResult183_g1;
					float3 rotatedValue56_g1 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction52_g1, ( Wind_Main_with_Waves108_g1 + Wind_Turbulence185_g1 ).x );
					float3 Output_Wind35_g1 = ( rotatedValue56_g1 - v.vertex.xyz );
					float Wind_Mask225_g1 = v.ase_color.r;
					float3 lerpResult232_g1 = lerp( float3( 0,0,0 ) , ( Output_Wind35_g1 * Wind_Mask225_g1 ) , ASPW_WindToggle);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch192_g1 = lerpResult232_g1;
					#else
					float3 staticSwitch192_g1 = temp_cast_0;
					#endif
					
					float4 ase_positionCS = UnityObjectToClipPos( v.vertex );
					float4 screenPos = ComputeScreenPos( ase_positionCS );
					o.ase_texcoord2 = screenPos;
					
					o.ase_texcoord1.xy = v.ase_texcoord.xy;
					o.ase_color = v.ase_color;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord1.zw = 0;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch192_g1;
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

					float2 uv_BaseAlbedo280 = IN.ase_texcoord1.xy;
					float4 tex2DNode280 = tex2D( _BaseAlbedo, uv_BaseAlbedo280 );
					float Base_Opacity295 = tex2DNode280.a;
					float4 screenPos = IN.ase_texcoord2;
					float4 ase_positionSSNorm = screenPos / screenPos.w;
					ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
					float4 ase_positionSS_Pixel = ASEScreenPositionNormalizedToPixel( ase_positionSSNorm, _ScreenParams );
					float dither728 = Dither4x4Bayer( fmod( ase_positionSS_Pixel.x, 4 ), fmod( ase_positionSS_Pixel.y, 4 ) );
					dither728 = step( dither728, saturate( saturate( ( ( IN.ase_color.r - _BottomDitherOffset ) * ( _BottomDitherContrast * 2 ) ) ) * 1.00001 ) );
					#ifdef _ENABLEBOTTOMDITHER_ON
					float staticSwitch731 = ( dither728 * Base_Opacity295 );
					#else
					float staticSwitch731 = Base_Opacity295;
					#endif
					float Output_Opacity732 = staticSwitch731;
					

					o.Normal = half3( 0, 0, 1 );

					o.Alpha = Output_Opacity732;
					half AlphaClipThreshold = _BaseOpacityCutoff;
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
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;720;-48,960;Inherit;False;2481;469;;12;732;731;730;729;728;727;726;725;724;723;722;721;Base Bottom Dither;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;721;0,1024;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;722;0,1312;Inherit;False;Property;_BottomDitherContrast;Bottom Dither Contrast;17;0;Create;True;0;0;0;False;0;False;3;0;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;723;0,1216;Inherit;False;Property;_BottomDitherOffset;Bottom Dither Offset;16;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;724;384,1024;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;725;384,1280;Inherit;False;2;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;639;-2752,-1840;Inherit;False;2241;429;;11;289;283;285;299;425;287;288;281;282;295;280;Base Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;726;640,1024;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;280;-2688,-1792;Inherit;True;Property;_BaseAlbedo;Base Albedo;9;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;d7b5f571c971c2844b6578a9de1662fb;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;727;896,1024;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;295;-2304,-1664;Inherit;False;Base Opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DitheringNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;728;1152,1024;Inherit;False;0;False;4;0;FLOAT;0;False;1;SAMPLER2D;;False;2;FLOAT4;0,0,0,0;False;3;SAMPLERSTATE;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;729;1152,1152;Inherit;False;295;Base Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;730;1408,1024;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;731;1664,1024;Inherit;False;Property;_EnableBottomDither;Enable Bottom Dither;12;0;Create;True;0;0;0;False;1;;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;671;-64,-1840;Inherit;False;2498;1449;;32;401;654;678;655;677;670;658;653;676;374;403;404;661;662;665;656;412;664;663;657;660;345;343;344;651;650;647;649;354;652;648;659;Base Tint Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;673;-48,-176;Inherit;False;2481;941;;15;342;375;376;340;370;339;331;338;332;330;333;336;337;335;334;Base Bottom Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;675;3008,-1842;Inherit;False;768;689;;6;644;423;296;315;290;621;Output;1,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;674;-2751,-690;Inherit;False;1855;693;;10;719;314;424;304;714;307;306;312;622;311;Base Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;643;-2752,-1200;Inherit;False;1087;302;;3;642;318;319;Base Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;732;2048,1024;Inherit;False;Output Opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;648;0,-1024;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;652;0,-768;Half;False;Global;ASP_GlobalTintNoiseUVScale;ASP_GlobalTintNoiseUVScale;20;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;354;0,-672;Inherit;False;Property;_TintNoiseUVScale;Tint Noise UV Scale;20;0;Create;True;0;0;0;False;0;False;5;10;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;649;256,-1024;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;647;384,-768;Inherit;False;3;3;0;FLOAT;0.01;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;650;512,-1024;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DesaturateOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;281;-2304,-1792;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;287;-1920,-1792;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;425;-1664,-1792;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;299;-1408,-1792;Inherit;False;Albedo Texture;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;285;-1408,-1664;Inherit;False;Property;_BaseAlbedoColor;Base Albedo Color;3;1;[HDR];Create;True;0;0;0;False;0;False;0.5019608,0.5019608,0.5019608,0;0.1004732,0.2924528,0.06207726,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;289;-768,-1792;Inherit;False;Base Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;344;0,-1664;Inherit;False;Property;_TintColor;Tint Color;19;1;[HDR];Create;True;0;0;0;False;0;False;0.5019608,0.5019608,0.5019608,0;0.6037736,0.5754763,0.1623354,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;343;0,-1792;Inherit;False;299;Albedo Texture;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendOpsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;345;256,-1792;Inherit;False;Overlay;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;660;256,-1568;Inherit;False;659;Base Tint Color Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;657;256,-1664;Inherit;False;289;Base Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;663;640,-1536;Half;False;Global;ASP_GlobalTintNoiseToggle;ASP_GlobalTintNoiseToggle;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;664;640,-1344;Half;False;Global;ASP_GlobalTintNoiseIntensity;ASP_GlobalTintNoiseIntensity;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;412;640,-1440;Inherit;False;Property;_TintNoiseIntensity;Tint Noise Intensity;21;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;656;640,-1792;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;665;640,-1664;Inherit;False;289;Base Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;662;1024,-1536;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;661;1280,-1792;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;404;1280,-1536;Inherit;False;289;Base Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;374;2048,-1792;Inherit;False;Base Albedo and Tint Color;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;621;3456,-1328;Inherit;False;Wind Waves;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;290;3072,-1792;Inherit;False;342;Output Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;315;3072,-1632;Inherit;False;314;Base Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;644;3072,-1712;Inherit;False;642;Base Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;319;-2688,-1152;Inherit;False;Property;_BaseNormalIntensity;Base Normal Intensity;8;0;Create;True;0;0;0;False;0;False;0;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;318;-2304,-1152;Inherit;True;Property;_BaseNormal;Base Normal;10;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;642;-1920,-1152;Inherit;False;Base Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;676;1280,-1024;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;653;1536,-1024;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;655;1280,-768;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;654;768,-672;Half;False;Global;ASP_GlobalTintNoiseContrast;ASP_GlobalTintNoiseContrast;20;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;334;0,384;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;335;256,384;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;337;0,640;Inherit;False;Property;_BottomColorContrast;Bottom Color Contrast;15;0;Create;True;0;0;0;False;0;False;1;3;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;336;512,384;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;333;0,256;Inherit;False;Property;_BottomColorOffset;Bottom Color Offset;14;0;Create;True;0;0;0;False;0;False;1;2.53;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;332;0,0;Inherit;False;Property;_BottomColor;Bottom Color;13;1;[HDR];Create;True;0;0;0;False;0;False;0.5019608,0.5019608,0.5019608,0;0.05126947,0.1912017,0.04231142,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;338;768,256;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;331;384,-128;Inherit;False;Overlay;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;339;1024,256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;370;896,0;Inherit;False;374;Base Albedo and Tint Color;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;340;1280,-128;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;376;1280,0;Inherit;False;374;Base Albedo and Tint Color;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;375;1664,-128;Inherit;False;Property;_EnableBottomColor;Enable Bottom Color;11;0;Create;True;0;0;0;False;1;Header(Bottom Color);False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;342;2176,-128;Inherit;False;Output Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;670;1280,-640;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;658;1792,-1024;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;659;2048,-1024;Inherit;False;Base Tint Color Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;401;768,-592;Inherit;False;Property;_TintNoiseContrast;Tint Noise Contrast;22;0;Create;True;0;0;0;False;0;False;5;5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;678;768,-768;Inherit;False;Property;_TintNoiseInvertMask;Tint Noise Invert Mask;23;1;[IntRange];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;677;1104,-896;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;651;768,-1024;Inherit;True;Global;ASP_GlobalTintNoiseTexture;ASP_GlobalTintNoiseTexture;0;1;[NoScaleOffset];Create;True;0;0;0;True;1;Header(Tint);False;-1;None;e93d8f0da5bea8144ad9925e81909be8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.BlendOpsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;283;-1152,-1792;Inherit;False;Overlay;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;288;-2304,-1536;Inherit;False;Property;_BaseAlbedoBrightness;Base Albedo Brightness;4;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;282;-2688,-1536;Inherit;False;Property;_BaseAlbedoDesaturation;Base Albedo Desaturation;5;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;718;3072,-1344;Inherit;False;MF_ASP_Global_WindGrass;24;;1;ebd015072dd6d824783224f1cda1c365;0;0;2;FLOAT3;0;FLOAT;229
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;424;-1408,-640;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;314;-1152,-640;Inherit;False;Base Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;714;-2688,-640;Inherit;False;Property;_BaseSmoothnessIntensity;Base Smoothness Intensity;6;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;304;-1920,-512;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;311;-2368,-288;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;622;-2688,-288;Inherit;False;621;Wind Waves;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;306;-2688,-480;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;307;-2176,-480;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;312;-2688,-160;Inherit;False;Property;_BaseSmoothnessWaves;Base Smoothness Waves;7;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;719;-1792,-640;Inherit;False;Property;_EnableSmoothnessWaves;Enable Smoothness Waves;1;0;Create;True;0;0;0;False;1;Header(Base);False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;423;3072,-1440;Inherit;False;Property;_BaseOpacityCutoff;Base Opacity Cutoff;2;0;Create;True;0;0;0;False;0;False;0.3;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;330;0,-128;Inherit;False;299;Albedo Texture;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;296;3072,-1536;Inherit;False;732;Output Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;403;1536,-1792;Inherit;False;Property;_EnableTintVariationColor;Enable Tint Variation Color;18;0;Create;True;0;0;0;False;1;Header(Tint Color);False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;708;3456,-1792;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;4;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ExtraPrePass;0;0;ExtraPrePass;6;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;709;3456,-1792;Float;False;True;-1;3;;0;4;ANGRYMESH/Stylized Pack/Grass;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ForwardBase;0;1;ForwardBase;17;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;44;Category;0;0;  Instanced Terrain Normals;1;0;Workflow;1;0;Surface;0;0;  Blend;0;0;  Dither Shadows;1;0;Two Sided;0;638772260063199115;Alpha Clipping;1;638912883410977146;  Use Shadow Threshold;0;0;Deferred Pass;1;638773069729548435;Normal Space,InvertActionOnDeselection;0;0;Transmission;0;0;  Transmission Shadow;0.5,False,;0;Translucency;0;0;  Translucency Strength;1,True,_BaseSSSIntensity;0;  Normal Distortion;0.5,True,_BaseSSSNormalDistortion;0;  Scattering;2,True,_BaseSSSScattering;0;  Direct;0.9,True,_BaseSSSDirect;0;  Ambient;0.1,True,_BaseSSSAmbiet;0;  Shadow;0.5,True,_BaseSSSShadow;0;Cast Shadows;1;0;Receive Shadows;1;0;Receive Specular;2;0;Receive Reflections;2;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;Ambient Light;1;0;Meta Pass;1;0;Add Pass;1;0;Override Baked GI;0;0;Write Depth;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Disable Batching;0;0;Vertex Position,InvertActionOnDeselection;1;0;0;6;False;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;710;3456,-1792;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;4;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ForwardAdd;0;2;ForwardAdd;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;True;4;1;False;;1;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;True;1;LightMode=ForwardAdd;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;711;3456,-1792;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;4;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;Deferred;0;3;Deferred;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Deferred;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;712;3456,-1792;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;4;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;Meta;0;4;Meta;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;713;3456,-1792;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;4;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ShadowCaster;0;5;ShadowCaster;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
WireConnection;724;0;721;1
WireConnection;724;1;723;0
WireConnection;725;0;722;0
WireConnection;726;0;724;0
WireConnection;726;1;725;0
WireConnection;727;0;726;0
WireConnection;295;0;280;4
WireConnection;728;0;727;0
WireConnection;730;0;728;0
WireConnection;730;1;729;0
WireConnection;731;1;729;0
WireConnection;731;0;730;0
WireConnection;732;0;731;0
WireConnection;649;0;648;1
WireConnection;649;1;648;3
WireConnection;647;1;652;0
WireConnection;647;2;354;0
WireConnection;650;0;649;0
WireConnection;650;1;647;0
WireConnection;281;0;280;5
WireConnection;281;1;282;0
WireConnection;287;0;281;0
WireConnection;287;1;288;0
WireConnection;425;0;287;0
WireConnection;299;0;425;0
WireConnection;289;0;283;0
WireConnection;345;0;343;0
WireConnection;345;1;344;5
WireConnection;656;0;657;0
WireConnection;656;1;345;0
WireConnection;656;2;660;0
WireConnection;662;0;663;0
WireConnection;662;1;412;0
WireConnection;662;2;664;0
WireConnection;661;0;665;0
WireConnection;661;1;656;0
WireConnection;661;2;662;0
WireConnection;374;0;403;0
WireConnection;621;0;718;229
WireConnection;318;5;319;0
WireConnection;642;0;318;0
WireConnection;676;0;651;1
WireConnection;676;1;677;0
WireConnection;676;2;678;0
WireConnection;653;0;676;0
WireConnection;653;1;655;0
WireConnection;653;2;670;1
WireConnection;655;0;654;0
WireConnection;655;1;401;0
WireConnection;335;0;334;4
WireConnection;336;0;335;0
WireConnection;336;1;337;0
WireConnection;338;0;333;0
WireConnection;338;1;336;0
WireConnection;331;0;330;0
WireConnection;331;1;332;5
WireConnection;339;0;338;0
WireConnection;340;0;370;0
WireConnection;340;1;331;0
WireConnection;340;2;339;0
WireConnection;375;1;376;0
WireConnection;375;0;340;0
WireConnection;342;0;375;0
WireConnection;658;0;653;0
WireConnection;659;0;658;0
WireConnection;677;0;651;1
WireConnection;651;1;650;0
WireConnection;283;0;299;0
WireConnection;283;1;285;5
WireConnection;424;0;719;0
WireConnection;314;0;424;0
WireConnection;304;1;714;0
WireConnection;304;2;307;0
WireConnection;311;0;622;0
WireConnection;311;1;312;0
WireConnection;307;0;306;1
WireConnection;307;1;311;0
WireConnection;719;1;714;0
WireConnection;719;0;304;0
WireConnection;403;1;404;0
WireConnection;403;0;661;0
WireConnection;709;0;290;0
WireConnection;709;1;644;0
WireConnection;709;5;315;0
WireConnection;709;7;296;0
WireConnection;709;8;423;0
WireConnection;709;15;718;0
ASEEND*/
//CHKSM=44A0A65FEBD70243F04682359A1B2622D69B0C42