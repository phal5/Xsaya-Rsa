// Made with Amplify Shader Editor v1.9.9.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ANGRYMESH/Stylized Pack/Tree Bark"
{
	Properties
	{
		[HDR][Header(Base)] _BaseAlbedoColor( "Base Albedo Color", Color ) = ( 0.5019608, 0.5019608, 0.5019608, 0.5019608 )
		_BaseAlbedoBrightness( "Base Albedo Brightness", Range( 0, 5 ) ) = 1
		_BaseAlbedoDesaturation( "Base Albedo Desaturation", Range( 0, 1 ) ) = 0
		_BaseMetallicIntensity( "Base Metallic Intensity", Range( 0, 1 ) ) = 0
		_BaseSmoothnessMin( "Base Smoothness Min", Range( 0, 5 ) ) = 0
		_BaseSmoothnessMax( "Base Smoothness Max", Range( 0, 5 ) ) = 1
		_BaseNormalIntensity( "Base Normal Intensity", Range( 0, 5 ) ) = 1
		_BaseBarkAOIntensity( "Base Bark AO Intensity", Range( 0, 1 ) ) = 0.5
		_BaseTreeAOIntensity( "Base Tree AO Intensity", Range( 0, 1 ) ) = 0
		[HDR] _BaseEmissiveColor( "Base Emissive Color", Color ) = ( 0, 0, 0 )
		_BaseEmissiveIntensity( "Base Emissive Intensity", Float ) = 2
		_BaseEmissiveMaskContrast( "Base Emissive Mask Contrast", Float ) = 2
		[NoScaleOffset] _BaseAlbedo( "Base Albedo", 2D ) = "gray" {}
		[NoScaleOffset] _BaseNormal( "Base Normal", 2D ) = "bump" {}
		[NoScaleOffset] _BaseSMAE( "Base SMAE", 2D ) = "gray" {}
		[Header(Top Layer)][Toggle( _ENABLETOPLAYERBLEND_ON )] _EnableTopLayerBlend( "Enable Top Layer Blend", Float ) = 1
		[HDR] _TopLayerAlbedoColor( "Top Layer Albedo Color", Color ) = ( 0.5019608, 0.5019608, 0.5019608, 0.5019608 )
		_TopLayerUVScale( "Top Layer UV Scale", Range( 0, 50 ) ) = 1
		_TopLayerSmoothnessMin( "Top Layer Smoothness Min", Range( 0, 5 ) ) = 0
		_TopLayerSmoothnessMax( "Top Layer Smoothness Max", Range( 0, 5 ) ) = 1
		_TopLayerNormalIntensity( "Top Layer Normal Intensity", Range( 0, 5 ) ) = 1
		_TopLayerNormalInfluence( "Top Layer Normal Influence", Range( 0, 1 ) ) = 0
		_TopLayerIntensity( "Top Layer Intensity", Range( 0, 1 ) ) = 1
		_TopLayerOffset( "Top Layer Offset", Range( 0, 1 ) ) = 0.5
		_TopLayerContrast( "Top Layer Contrast", Range( 0, 30 ) ) = 10
		_TopArrowDirectionOffset( "Top Arrow Direction Offset", Range( 0, 2 ) ) = 0
		_TopLayerBottomOffset( "Top Layer Bottom Offset", Range( 0, 5 ) ) = 1
		[NoScaleOffset] _TopLayerAlbedo( "Top Layer Albedo", 2D ) = "gray" {}
		[NoScaleOffset][Normal] _TopLayerNormal( "Top Layer Normal", 2D ) = "bump" {}
		[NoScaleOffset] _TopLayerSmoothness( "Top Layer Smoothness", 2D ) = "gray" {}
		[Header(Wind)][Toggle( _ENABLEWIND_ON )] _EnableWind( "Enable Wind", Float ) = 1
		[Header(Wind (Use the same values for each tree material))] _WindTreeFlexibility( "Wind Tree Flexibility", Range( 0, 2 ) ) = 1
		_WindTreeBaseRigidity( "Wind Tree Base Rigidity", Range( 0, 5 ) ) = 2.5
		[Toggle( _ENABLESTATICMESHSUPPORT_ON )] _EnableStaticMeshSupport( "Enable Static Mesh Support", Float ) = 0


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

		Cull Back
		AlphaToMask Off
		ZWrite On
		ZTest LEqual
		ColorMask RGBA

		

		Blend Off
		

		CGINCLUDE
			#pragma target 4.0

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
				#define ASE_NEEDS_TEXTURE_COORDINATES3
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES3
				#define ASE_NEEDS_FRAG_COLOR
				#define ASE_NEEDS_WORLD_TANGENT
				#define ASE_NEEDS_FRAG_WORLD_TANGENT
				#define ASE_NEEDS_WORLD_NORMAL
				#define ASE_NEEDS_FRAG_WORLD_NORMAL
				#define ASE_NEEDS_FRAG_WORLD_BITANGENT
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
				#define ASE_NEEDS_WORLD_POSITION
				#define ASE_NEEDS_FRAG_WORLD_POSITION
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLESTATICMESHSUPPORT_ON
				#pragma shader_feature_local _ENABLETOPLAYERBLEND_ON


				struct appdata
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					half4 tangent : TANGENT;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					float4 ase_texcoord3 : TEXCOORD3;
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
					float4 ase_texcoord7 : TEXCOORD7;
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

				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform half ASPW_WindTreeSpeed;
				uniform half ASPW_WindTreeAmplitude;
				uniform float _WindTreeFlexibility;
				uniform half ASPW_WindTreeFlexibility;
				uniform float _WindTreeBaseRigidity;
				uniform float ASPW_WindToggle;
				uniform sampler2D _BaseAlbedo;
				uniform float _BaseAlbedoDesaturation;
				uniform float _BaseAlbedoBrightness;
				uniform half4 _BaseAlbedoColor;
				uniform sampler2D _TopLayerAlbedo;
				uniform float _TopLayerUVScale;
				uniform half4 _TopLayerAlbedoColor;
				uniform sampler2D _BaseSMAE;
				uniform half _BaseBarkAOIntensity;
				uniform float _BaseTreeAOIntensity;
				uniform sampler2D _BaseNormal;
				uniform half _BaseNormalIntensity;
				uniform half _TopLayerOffset;
				uniform half ASPT_TopLayerOffset;
				uniform half _TopLayerContrast;
				uniform half ASPT_TopLayerContrast;
				uniform half _TopLayerIntensity;
				uniform half ASPT_TopLayerIntensity;
				uniform half _TopArrowDirectionOffset;
				uniform half ASPT_TopLayerArrowDirection;
				uniform float _TopLayerBottomOffset;
				uniform half ASPT_TopLayerBottomOffset;
				uniform half ASPT_TopLayerHeightStart;
				uniform float ASPT_TopLayerHeightFade;
				uniform sampler2D _TopLayerNormal;
				uniform half _TopLayerNormalIntensity;
				uniform half _TopLayerNormalInfluence;
				uniform half _BaseMetallicIntensity;
				uniform half _BaseSmoothnessMin;
				uniform half _BaseSmoothnessMax;
				uniform half _TopLayerSmoothnessMin;
				uniform half _TopLayerSmoothnessMax;
				uniform sampler2D _TopLayerSmoothness;
				uniform half3 _BaseEmissiveColor;
				uniform float _BaseEmissiveMaskContrast;
				uniform float _BaseEmissiveIntensity;


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
					float3 Global_Wind_Direction493_g22 = ASPW_WindDirection;
					float3 worldToObjDir269_g22 = mul( unity_WorldToObject, float4( Global_Wind_Direction493_g22, 0.0 ) ).xyz;
					float3 normalizeResult268_g22 = ASESafeNormalize( worldToObjDir269_g22 );
					float3 Wind_Direction_Leaf85_g22 = normalizeResult268_g22;
					float3 break86_g22 = Wind_Direction_Leaf85_g22;
					float3 appendResult89_g22 = (float3(break86_g22.z , 0.0 , ( break86_g22.x * -1.0 )));
					float3 Wind_Direction91_g22 = appendResult89_g22;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Tree_Randomization149_g22 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_56_0_g22 = ( Wind_Tree_Randomization149_g22 + _Time.y );
					float Global_Wind_Tree_Speed491_g22 = ASPW_WindTreeSpeed;
					float temp_output_213_0_g22 = ( Global_Wind_Tree_Speed491_g22 * 7.0 );
					float Global_Wind_Tree_Amplitude464_g22 = ASPW_WindTreeAmplitude;
					float Global_Wind_Tree_Flexibility490_g22 = ASPW_WindTreeFlexibility;
					float temp_output_383_0_g22 = ( ( _WindTreeFlexibility * Global_Wind_Tree_Flexibility490_g22 ) * 0.3 );
					float Wind_Tree_257_g22 = ( ( ( ( ( sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.1 ) ) ) + sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.3 ) ) ) + sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.5 ) ) ) ) + Global_Wind_Tree_Amplitude464_g22 ) * Global_Wind_Tree_Amplitude464_g22 ) * 0.01 ) * temp_output_383_0_g22 );
					float3 rotatedValue99_g22 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction91_g22, Wind_Tree_257_g22 );
					float3 Rotate_About_Axis354_g22 = ( rotatedValue99_g22 - v.vertex.xyz );
					float3 break452_g22 = normalizeResult268_g22;
					float3 appendResult454_g22 = (float3(break452_g22.x , 0.0 , break452_g22.z));
					float3 Wind_Direction_SM_Support453_g22 = appendResult454_g22;
					float Wind_Tree_Flexibility483_g22 = temp_output_383_0_g22;
					#ifdef _ENABLESTATICMESHSUPPORT_ON
					float3 staticSwitch482_g22 = ( Global_Wind_Tree_Amplitude464_g22 * Wind_Tree_257_g22 * Wind_Direction_SM_Support453_g22 * Wind_Tree_Flexibility483_g22 );
					#else
					float3 staticSwitch482_g22 = Rotate_About_Axis354_g22;
					#endif
					float3 Wind_Global450_g22 = staticSwitch482_g22;
					float2 texCoord433_g22 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float Wind_Mask_UV_CH2_VChannel434_g22 = texCoord433_g22.y;
					float saferPower113_g22 = abs( Wind_Mask_UV_CH2_VChannel434_g22 );
					float Wind_Tree_Mask114_g22 = pow( saferPower113_g22 , _WindTreeBaseRigidity );
					float3 temp_output_385_0_g22 = ( Wind_Global450_g22 * Wind_Tree_Mask114_g22 );
					float Global_Wind_Toggle504_g22 = ASPW_WindToggle;
					float3 lerpResult124_g22 = lerp( float3( 0,0,0 ) , temp_output_385_0_g22 , Global_Wind_Toggle504_g22);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch468_g22 = lerpResult124_g22;
					#else
					float3 staticSwitch468_g22 = temp_cast_0;
					#endif
					
					o.ase_texcoord6.xy = v.texcoord.xyzw.xy;
					o.ase_texcoord6.zw = v.ase_texcoord3.xy;
					o.ase_color = v.ase_color;
					o.ase_texcoord7.xy = v.texcoord2.xyzw.xy;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord7.zw = 0;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch468_g22;
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
					float4 ase_texcoord3 : TEXCOORD3;
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
					o.texcoord = v.texcoord;
					o.ase_texcoord3 = v.ase_texcoord3;
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
					o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
					o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
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

					float2 texCoord563 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
					float2 texCoord565 = IN.ase_texcoord6.zw * float2( 1,1 ) + float2( 0,0 );
					float VColor_Blue_Tree_Mask561 = IN.ase_color.b;
					float3 lerpResult566 = lerp( tex2D( _BaseAlbedo, texCoord563 ).rgb , tex2D( _BaseAlbedo, texCoord565 ).rgb , VColor_Blue_Tree_Mask561);
					float3 desaturateInitialColor1070 = lerpResult566;
					float desaturateDot1070 = dot( desaturateInitialColor1070, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar1070 = lerp( desaturateInitialColor1070, desaturateDot1070.xxx, _BaseAlbedoDesaturation );
					float3 blendOpSrc345 = ( desaturateVar1070 * _BaseAlbedoBrightness );
					float3 blendOpDest345 = _BaseAlbedoColor.rgb;
					float3 Base_Albedo350 = ( saturate( (( blendOpDest345 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest345 ) * ( 1.0 - blendOpSrc345 ) ) : ( 2.0 * blendOpDest345 * blendOpSrc345 ) ) ));
					float2 texCoord594 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
					float2 Top_Layer_UV_Scale597 = ( texCoord594 * _TopLayerUVScale );
					float3 blendOpSrc599 = tex2D( _TopLayerAlbedo, Top_Layer_UV_Scale597 ).rgb;
					float3 blendOpDest599 = _TopLayerAlbedoColor.rgb;
					float2 texCoord580 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
					float4 tex2DNode577 = tex2D( _BaseSMAE, texCoord580 );
					float2 texCoord581 = IN.ase_texcoord6.zw * float2( 1,1 ) + float2( 0,0 );
					float4 tex2DNode576 = tex2D( _BaseSMAE, texCoord581 );
					float3 lerpResult579 = lerp( tex2DNode577.rgb , tex2DNode576.rgb , VColor_Blue_Tree_Mask561);
					float3 break582 = lerpResult579;
					float lerpResult365 = lerp( 1.0 , break582.z , _BaseBarkAOIntensity);
					float lerpResult559 = lerp( 1.0 , IN.ase_color.a , _BaseTreeAOIntensity);
					float VColor_Alpha_Tree_AO562 = lerpResult559;
					float Base_AO348 = ( lerpResult365 * VColor_Alpha_Tree_AO562 );
					float2 texCoord571 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
					float2 texCoord572 = IN.ase_texcoord6.zw * float2( 1,1 ) + float2( 0,0 );
					float3 lerpResult573 = lerp( UnpackScaleNormal( tex2D( _BaseNormal, texCoord571 ), _BaseNormalIntensity ) , UnpackScaleNormal( tex2D( _BaseNormal, texCoord572 ), _BaseNormalIntensity ) , VColor_Blue_Tree_Mask561);
					float3 Base_Normal355 = lerpResult573;
					float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
					float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
					float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
					float3 tanNormal776 = Base_Normal355;
					float3 worldNormal776 = float3( dot( tanToWorld0, tanNormal776 ), dot( tanToWorld1, tanNormal776 ), dot( tanToWorld2, tanNormal776 ) );
					float temp_output_616_0 = ( _TopLayerOffset * ASPT_TopLayerOffset );
					float saferPower17_g23 = abs( abs( ( saturate( worldNormal776.y ) + temp_output_616_0 ) ) );
					float temp_output_617_0 = ( _TopLayerContrast * ASPT_TopLayerContrast );
					float temp_output_615_0 = ( _TopLayerIntensity * ASPT_TopLayerIntensity );
					float3 tanNormal629 = Base_Normal355;
					float3 worldNormal629 = float3( dot( tanToWorld0, tanNormal629 ), dot( tanToWorld1, tanNormal629 ), dot( tanToWorld2, tanNormal629 ) );
					float3 Global_Arrow_Direction655 = ASPW_WindDirection;
					float dotResult633 = dot( worldNormal629 , Global_Arrow_Direction655 );
					float dotResult682 = dot( worldNormal629 , float3( 0, 1, 0 ) );
					float2 texCoord668 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
					float Arrow_Direction_Mask693 = ( ( ( 1.0 - dotResult633 ) * ( _TopArrowDirectionOffset * ASPT_TopLayerArrowDirection ) ) * ( ( ( 3.0 - ( 1.0 - dotResult682 ) ) * ( 1.0 - texCoord668.y ) ) + 0.05 ) );
					float2 texCoord705 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
					float temp_output_706_0 = ( 1.0 - texCoord705.y );
					float temp_output_711_0 = ( ( ( temp_output_706_0 * temp_output_706_0 ) * ( temp_output_706_0 * temp_output_706_0 ) ) * _TopLayerBottomOffset * ASPT_TopLayerBottomOffset );
					float3 tanNormal715 = Base_Normal355;
					float3 worldNormal715 = float3( dot( tanToWorld0, tanNormal715 ), dot( tanToWorld1, tanNormal715 ), dot( tanToWorld2, tanNormal715 ) );
					float dotResult716 = dot( worldNormal715 , float3( 0, 1, 0 ) );
					float Top_Layer_Bottom_Offset721 = ( ( temp_output_711_0 * temp_output_711_0 ) + ( dotResult716 - 1.0 ) );
					float clampResult1046 = clamp( ( Arrow_Direction_Mask693 + Top_Layer_Bottom_Offset721 ) , 0.0 , 5.0 );
					float Top_Layer_Offset696 = temp_output_616_0;
					float saferPower698 = abs( ( clampResult1046 * Top_Layer_Offset696 ) );
					float Top_Layer_Contrast699 = temp_output_617_0;
					float Top_Layer_Intensity702 = temp_output_615_0;
					float Top_Layer_Direction1057 = saturate( ( pow( saferPower698 , Top_Layer_Contrast699 ) * Top_Layer_Intensity702 ) );
					float Top_Layer_Mask621 = saturate( ( ( ( saturate( pow( saferPower17_g23 , temp_output_617_0 ) ) * temp_output_615_0 ) + Top_Layer_Direction1057 ) * saturate(  (0.0 + ( PositionWS.y - ASPT_TopLayerHeightStart ) * ( 1.0 - 0.0 ) / ( ( ASPT_TopLayerHeightStart + ASPT_TopLayerHeightFade ) - ASPT_TopLayerHeightStart ) ) ) ) );
					float3 lerpResult605 = lerp( Base_Albedo350 , ( (( blendOpDest599 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest599 ) * ( 1.0 - blendOpSrc599 ) ) : ( 2.0 * blendOpDest599 * blendOpSrc599 ) ) * Base_AO348 ) , Top_Layer_Mask621);
					float3 Top_Layer_Albedo625 = lerpResult605;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch623 = Top_Layer_Albedo625;
					#else
					float3 staticSwitch623 = Base_Albedo350;
					#endif
					float3 Output_Albedo627 = staticSwitch623;
					
					float3 tex2DNode646 = UnpackScaleNormal( tex2D( _TopLayerNormal, Top_Layer_UV_Scale597 ), _TopLayerNormalIntensity );
					float3 lerpResult649 = lerp( BlendNormals( tex2DNode646 , Base_Normal355 ) , tex2DNode646 , _TopLayerNormalInfluence);
					float3 lerpResult652 = lerp( Base_Normal355 , lerpResult649 , Top_Layer_Mask621);
					float3 Top_Layer_Normal653 = lerpResult652;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch738 = Top_Layer_Normal653;
					#else
					float3 staticSwitch738 = Base_Normal355;
					#endif
					float3 Output_Normal742 = staticSwitch738;
					
					float Base_Metallic364 = ( break582.y * _BaseMetallicIntensity );
					float lerpResult747 = lerp( Base_Metallic364 , 0.0 , Top_Layer_Mask621);
					float Top_Layer_Metallic749 = lerpResult747;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float staticSwitch758 = Top_Layer_Metallic749;
					#else
					float staticSwitch758 = Base_Metallic364;
					#endif
					float Output_Metallic761 = staticSwitch758;
					
					float Texture_Smoothness1079 = break582.x;
					float lerpResult1082 = lerp( _BaseSmoothnessMin , _BaseSmoothnessMax , Texture_Smoothness1079);
					float Base_Smoothness361 = saturate( lerpResult1082 );
					float lerpResult1086 = lerp( _TopLayerSmoothnessMin , _TopLayerSmoothnessMax , tex2D( _TopLayerSmoothness, Top_Layer_UV_Scale597 ).r);
					float lerpResult734 = lerp( Base_Smoothness361 , lerpResult1086 , Top_Layer_Mask621);
					float Top_Layer_Smoothness737 = saturate( lerpResult734 );
					#ifdef _ENABLETOPLAYERBLEND_ON
					float staticSwitch741 = Top_Layer_Smoothness737;
					#else
					float staticSwitch741 = Base_Smoothness361;
					#endif
					float Output_Smoothness745 = staticSwitch741;
					
					float lerpResult751 = lerp( Base_AO348 , 1.0 , Top_Layer_Mask621);
					float Top_Layer_AO753 = lerpResult751;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float staticSwitch762 = Top_Layer_AO753;
					#else
					float staticSwitch762 = Base_AO348;
					#endif
					float Output_AO765 = staticSwitch762;
					
					float lerpResult587 = lerp( tex2DNode577.a , tex2DNode576.a , VColor_Blue_Tree_Mask561);
					float Emissive_Mask371 = lerpResult587;
					float saferPower373 = abs( Emissive_Mask371 );
					float3 Base_Emissive377 = ( ( _BaseEmissiveColor * pow( saferPower373 , _BaseEmissiveMaskContrast ) ) * _BaseEmissiveIntensity );
					float3 lerpResult755 = lerp( Base_Emissive377 , float3( 0,0,0 ) , Top_Layer_Mask621);
					float3 Top_Layer_Emissive757 = lerpResult755;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch766 = Top_Layer_Emissive757;
					#else
					float3 staticSwitch766 = Base_Emissive377;
					#endif
					float3 Output_Emissive769 = staticSwitch766;
					

					o.Albedo = Output_Albedo627;
					o.Normal = Output_Normal742;

					half3 Specular = half3( 0, 0, 0 );
					half Metallic = Output_Metallic761;
					half Smoothness = Output_Smoothness745;
					half Occlusion = Output_AO765;

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

					o.Emission = Output_Emissive769;
					o.Alpha = 1;
					half AlphaClipThreshold = 0.5;
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
				#define ASE_NEEDS_TEXTURE_COORDINATES3
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES3
				#define ASE_NEEDS_FRAG_COLOR
				#define ASE_NEEDS_FRAG_WORLD_TANGENT
				#define ASE_NEEDS_WORLD_NORMAL
				#define ASE_NEEDS_FRAG_WORLD_NORMAL
				#define ASE_NEEDS_FRAG_WORLD_BITANGENT
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
				#define ASE_NEEDS_WORLD_POSITION
				#define ASE_NEEDS_FRAG_WORLD_POSITION
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLESTATICMESHSUPPORT_ON
				#pragma shader_feature_local _ENABLETOPLAYERBLEND_ON


				struct appdata
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					half4 tangent : TANGENT;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					float4 ase_texcoord3 : TEXCOORD3;
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
					float4 ase_texcoord6 : TEXCOORD6;
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

				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform half ASPW_WindTreeSpeed;
				uniform half ASPW_WindTreeAmplitude;
				uniform float _WindTreeFlexibility;
				uniform half ASPW_WindTreeFlexibility;
				uniform float _WindTreeBaseRigidity;
				uniform float ASPW_WindToggle;
				uniform sampler2D _BaseAlbedo;
				uniform float _BaseAlbedoDesaturation;
				uniform float _BaseAlbedoBrightness;
				uniform half4 _BaseAlbedoColor;
				uniform sampler2D _TopLayerAlbedo;
				uniform float _TopLayerUVScale;
				uniform half4 _TopLayerAlbedoColor;
				uniform sampler2D _BaseSMAE;
				uniform half _BaseBarkAOIntensity;
				uniform float _BaseTreeAOIntensity;
				uniform sampler2D _BaseNormal;
				uniform half _BaseNormalIntensity;
				uniform half _TopLayerOffset;
				uniform half ASPT_TopLayerOffset;
				uniform half _TopLayerContrast;
				uniform half ASPT_TopLayerContrast;
				uniform half _TopLayerIntensity;
				uniform half ASPT_TopLayerIntensity;
				uniform half _TopArrowDirectionOffset;
				uniform half ASPT_TopLayerArrowDirection;
				uniform float _TopLayerBottomOffset;
				uniform half ASPT_TopLayerBottomOffset;
				uniform half ASPT_TopLayerHeightStart;
				uniform float ASPT_TopLayerHeightFade;
				uniform sampler2D _TopLayerNormal;
				uniform half _TopLayerNormalIntensity;
				uniform half _TopLayerNormalInfluence;
				uniform half _BaseMetallicIntensity;
				uniform half _BaseSmoothnessMin;
				uniform half _BaseSmoothnessMax;
				uniform half _TopLayerSmoothnessMin;
				uniform half _TopLayerSmoothnessMax;
				uniform sampler2D _TopLayerSmoothness;
				uniform half3 _BaseEmissiveColor;
				uniform float _BaseEmissiveMaskContrast;
				uniform float _BaseEmissiveIntensity;


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
					float3 Global_Wind_Direction493_g22 = ASPW_WindDirection;
					float3 worldToObjDir269_g22 = mul( unity_WorldToObject, float4( Global_Wind_Direction493_g22, 0.0 ) ).xyz;
					float3 normalizeResult268_g22 = ASESafeNormalize( worldToObjDir269_g22 );
					float3 Wind_Direction_Leaf85_g22 = normalizeResult268_g22;
					float3 break86_g22 = Wind_Direction_Leaf85_g22;
					float3 appendResult89_g22 = (float3(break86_g22.z , 0.0 , ( break86_g22.x * -1.0 )));
					float3 Wind_Direction91_g22 = appendResult89_g22;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Tree_Randomization149_g22 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_56_0_g22 = ( Wind_Tree_Randomization149_g22 + _Time.y );
					float Global_Wind_Tree_Speed491_g22 = ASPW_WindTreeSpeed;
					float temp_output_213_0_g22 = ( Global_Wind_Tree_Speed491_g22 * 7.0 );
					float Global_Wind_Tree_Amplitude464_g22 = ASPW_WindTreeAmplitude;
					float Global_Wind_Tree_Flexibility490_g22 = ASPW_WindTreeFlexibility;
					float temp_output_383_0_g22 = ( ( _WindTreeFlexibility * Global_Wind_Tree_Flexibility490_g22 ) * 0.3 );
					float Wind_Tree_257_g22 = ( ( ( ( ( sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.1 ) ) ) + sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.3 ) ) ) + sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.5 ) ) ) ) + Global_Wind_Tree_Amplitude464_g22 ) * Global_Wind_Tree_Amplitude464_g22 ) * 0.01 ) * temp_output_383_0_g22 );
					float3 rotatedValue99_g22 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction91_g22, Wind_Tree_257_g22 );
					float3 Rotate_About_Axis354_g22 = ( rotatedValue99_g22 - v.vertex.xyz );
					float3 break452_g22 = normalizeResult268_g22;
					float3 appendResult454_g22 = (float3(break452_g22.x , 0.0 , break452_g22.z));
					float3 Wind_Direction_SM_Support453_g22 = appendResult454_g22;
					float Wind_Tree_Flexibility483_g22 = temp_output_383_0_g22;
					#ifdef _ENABLESTATICMESHSUPPORT_ON
					float3 staticSwitch482_g22 = ( Global_Wind_Tree_Amplitude464_g22 * Wind_Tree_257_g22 * Wind_Direction_SM_Support453_g22 * Wind_Tree_Flexibility483_g22 );
					#else
					float3 staticSwitch482_g22 = Rotate_About_Axis354_g22;
					#endif
					float3 Wind_Global450_g22 = staticSwitch482_g22;
					float2 texCoord433_g22 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float Wind_Mask_UV_CH2_VChannel434_g22 = texCoord433_g22.y;
					float saferPower113_g22 = abs( Wind_Mask_UV_CH2_VChannel434_g22 );
					float Wind_Tree_Mask114_g22 = pow( saferPower113_g22 , _WindTreeBaseRigidity );
					float3 temp_output_385_0_g22 = ( Wind_Global450_g22 * Wind_Tree_Mask114_g22 );
					float Global_Wind_Toggle504_g22 = ASPW_WindToggle;
					float3 lerpResult124_g22 = lerp( float3( 0,0,0 ) , temp_output_385_0_g22 , Global_Wind_Toggle504_g22);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch468_g22 = lerpResult124_g22;
					#else
					float3 staticSwitch468_g22 = temp_cast_0;
					#endif
					
					o.ase_texcoord5.xy = v.texcoord.xyzw.xy;
					o.ase_texcoord5.zw = v.ase_texcoord3.xy;
					o.ase_color = v.ase_color;
					o.ase_texcoord6.xy = v.texcoord2.xyzw.xy;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord6.zw = 0;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch468_g22;
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
					float4 ase_texcoord3 : TEXCOORD3;
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
					o.texcoord = v.texcoord;
					o.ase_texcoord3 = v.ase_texcoord3;
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
					o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
					o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
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

					float2 texCoord563 = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
					float2 texCoord565 = IN.ase_texcoord5.zw * float2( 1,1 ) + float2( 0,0 );
					float VColor_Blue_Tree_Mask561 = IN.ase_color.b;
					float3 lerpResult566 = lerp( tex2D( _BaseAlbedo, texCoord563 ).rgb , tex2D( _BaseAlbedo, texCoord565 ).rgb , VColor_Blue_Tree_Mask561);
					float3 desaturateInitialColor1070 = lerpResult566;
					float desaturateDot1070 = dot( desaturateInitialColor1070, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar1070 = lerp( desaturateInitialColor1070, desaturateDot1070.xxx, _BaseAlbedoDesaturation );
					float3 blendOpSrc345 = ( desaturateVar1070 * _BaseAlbedoBrightness );
					float3 blendOpDest345 = _BaseAlbedoColor.rgb;
					float3 Base_Albedo350 = ( saturate( (( blendOpDest345 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest345 ) * ( 1.0 - blendOpSrc345 ) ) : ( 2.0 * blendOpDest345 * blendOpSrc345 ) ) ));
					float2 texCoord594 = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
					float2 Top_Layer_UV_Scale597 = ( texCoord594 * _TopLayerUVScale );
					float3 blendOpSrc599 = tex2D( _TopLayerAlbedo, Top_Layer_UV_Scale597 ).rgb;
					float3 blendOpDest599 = _TopLayerAlbedoColor.rgb;
					float2 texCoord580 = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
					float4 tex2DNode577 = tex2D( _BaseSMAE, texCoord580 );
					float2 texCoord581 = IN.ase_texcoord5.zw * float2( 1,1 ) + float2( 0,0 );
					float4 tex2DNode576 = tex2D( _BaseSMAE, texCoord581 );
					float3 lerpResult579 = lerp( tex2DNode577.rgb , tex2DNode576.rgb , VColor_Blue_Tree_Mask561);
					float3 break582 = lerpResult579;
					float lerpResult365 = lerp( 1.0 , break582.z , _BaseBarkAOIntensity);
					float lerpResult559 = lerp( 1.0 , IN.ase_color.a , _BaseTreeAOIntensity);
					float VColor_Alpha_Tree_AO562 = lerpResult559;
					float Base_AO348 = ( lerpResult365 * VColor_Alpha_Tree_AO562 );
					float2 texCoord571 = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
					float2 texCoord572 = IN.ase_texcoord5.zw * float2( 1,1 ) + float2( 0,0 );
					float3 lerpResult573 = lerp( UnpackScaleNormal( tex2D( _BaseNormal, texCoord571 ), _BaseNormalIntensity ) , UnpackScaleNormal( tex2D( _BaseNormal, texCoord572 ), _BaseNormalIntensity ) , VColor_Blue_Tree_Mask561);
					float3 Base_Normal355 = lerpResult573;
					float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
					float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
					float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
					float3 tanNormal776 = Base_Normal355;
					float3 worldNormal776 = float3( dot( tanToWorld0, tanNormal776 ), dot( tanToWorld1, tanNormal776 ), dot( tanToWorld2, tanNormal776 ) );
					float temp_output_616_0 = ( _TopLayerOffset * ASPT_TopLayerOffset );
					float saferPower17_g23 = abs( abs( ( saturate( worldNormal776.y ) + temp_output_616_0 ) ) );
					float temp_output_617_0 = ( _TopLayerContrast * ASPT_TopLayerContrast );
					float temp_output_615_0 = ( _TopLayerIntensity * ASPT_TopLayerIntensity );
					float3 tanNormal629 = Base_Normal355;
					float3 worldNormal629 = float3( dot( tanToWorld0, tanNormal629 ), dot( tanToWorld1, tanNormal629 ), dot( tanToWorld2, tanNormal629 ) );
					float3 Global_Arrow_Direction655 = ASPW_WindDirection;
					float dotResult633 = dot( worldNormal629 , Global_Arrow_Direction655 );
					float dotResult682 = dot( worldNormal629 , float3( 0, 1, 0 ) );
					float2 texCoord668 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
					float Arrow_Direction_Mask693 = ( ( ( 1.0 - dotResult633 ) * ( _TopArrowDirectionOffset * ASPT_TopLayerArrowDirection ) ) * ( ( ( 3.0 - ( 1.0 - dotResult682 ) ) * ( 1.0 - texCoord668.y ) ) + 0.05 ) );
					float2 texCoord705 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
					float temp_output_706_0 = ( 1.0 - texCoord705.y );
					float temp_output_711_0 = ( ( ( temp_output_706_0 * temp_output_706_0 ) * ( temp_output_706_0 * temp_output_706_0 ) ) * _TopLayerBottomOffset * ASPT_TopLayerBottomOffset );
					float3 tanNormal715 = Base_Normal355;
					float3 worldNormal715 = float3( dot( tanToWorld0, tanNormal715 ), dot( tanToWorld1, tanNormal715 ), dot( tanToWorld2, tanNormal715 ) );
					float dotResult716 = dot( worldNormal715 , float3( 0, 1, 0 ) );
					float Top_Layer_Bottom_Offset721 = ( ( temp_output_711_0 * temp_output_711_0 ) + ( dotResult716 - 1.0 ) );
					float clampResult1046 = clamp( ( Arrow_Direction_Mask693 + Top_Layer_Bottom_Offset721 ) , 0.0 , 5.0 );
					float Top_Layer_Offset696 = temp_output_616_0;
					float saferPower698 = abs( ( clampResult1046 * Top_Layer_Offset696 ) );
					float Top_Layer_Contrast699 = temp_output_617_0;
					float Top_Layer_Intensity702 = temp_output_615_0;
					float Top_Layer_Direction1057 = saturate( ( pow( saferPower698 , Top_Layer_Contrast699 ) * Top_Layer_Intensity702 ) );
					float Top_Layer_Mask621 = saturate( ( ( ( saturate( pow( saferPower17_g23 , temp_output_617_0 ) ) * temp_output_615_0 ) + Top_Layer_Direction1057 ) * saturate(  (0.0 + ( PositionWS.y - ASPT_TopLayerHeightStart ) * ( 1.0 - 0.0 ) / ( ( ASPT_TopLayerHeightStart + ASPT_TopLayerHeightFade ) - ASPT_TopLayerHeightStart ) ) ) ) );
					float3 lerpResult605 = lerp( Base_Albedo350 , ( (( blendOpDest599 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest599 ) * ( 1.0 - blendOpSrc599 ) ) : ( 2.0 * blendOpDest599 * blendOpSrc599 ) ) * Base_AO348 ) , Top_Layer_Mask621);
					float3 Top_Layer_Albedo625 = lerpResult605;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch623 = Top_Layer_Albedo625;
					#else
					float3 staticSwitch623 = Base_Albedo350;
					#endif
					float3 Output_Albedo627 = staticSwitch623;
					
					float3 tex2DNode646 = UnpackScaleNormal( tex2D( _TopLayerNormal, Top_Layer_UV_Scale597 ), _TopLayerNormalIntensity );
					float3 lerpResult649 = lerp( BlendNormals( tex2DNode646 , Base_Normal355 ) , tex2DNode646 , _TopLayerNormalInfluence);
					float3 lerpResult652 = lerp( Base_Normal355 , lerpResult649 , Top_Layer_Mask621);
					float3 Top_Layer_Normal653 = lerpResult652;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch738 = Top_Layer_Normal653;
					#else
					float3 staticSwitch738 = Base_Normal355;
					#endif
					float3 Output_Normal742 = staticSwitch738;
					
					float Base_Metallic364 = ( break582.y * _BaseMetallicIntensity );
					float lerpResult747 = lerp( Base_Metallic364 , 0.0 , Top_Layer_Mask621);
					float Top_Layer_Metallic749 = lerpResult747;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float staticSwitch758 = Top_Layer_Metallic749;
					#else
					float staticSwitch758 = Base_Metallic364;
					#endif
					float Output_Metallic761 = staticSwitch758;
					
					float Texture_Smoothness1079 = break582.x;
					float lerpResult1082 = lerp( _BaseSmoothnessMin , _BaseSmoothnessMax , Texture_Smoothness1079);
					float Base_Smoothness361 = saturate( lerpResult1082 );
					float lerpResult1086 = lerp( _TopLayerSmoothnessMin , _TopLayerSmoothnessMax , tex2D( _TopLayerSmoothness, Top_Layer_UV_Scale597 ).r);
					float lerpResult734 = lerp( Base_Smoothness361 , lerpResult1086 , Top_Layer_Mask621);
					float Top_Layer_Smoothness737 = saturate( lerpResult734 );
					#ifdef _ENABLETOPLAYERBLEND_ON
					float staticSwitch741 = Top_Layer_Smoothness737;
					#else
					float staticSwitch741 = Base_Smoothness361;
					#endif
					float Output_Smoothness745 = staticSwitch741;
					
					float lerpResult751 = lerp( Base_AO348 , 1.0 , Top_Layer_Mask621);
					float Top_Layer_AO753 = lerpResult751;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float staticSwitch762 = Top_Layer_AO753;
					#else
					float staticSwitch762 = Base_AO348;
					#endif
					float Output_AO765 = staticSwitch762;
					
					float lerpResult587 = lerp( tex2DNode577.a , tex2DNode576.a , VColor_Blue_Tree_Mask561);
					float Emissive_Mask371 = lerpResult587;
					float saferPower373 = abs( Emissive_Mask371 );
					float3 Base_Emissive377 = ( ( _BaseEmissiveColor * pow( saferPower373 , _BaseEmissiveMaskContrast ) ) * _BaseEmissiveIntensity );
					float3 lerpResult755 = lerp( Base_Emissive377 , float3( 0,0,0 ) , Top_Layer_Mask621);
					float3 Top_Layer_Emissive757 = lerpResult755;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch766 = Top_Layer_Emissive757;
					#else
					float3 staticSwitch766 = Base_Emissive377;
					#endif
					float3 Output_Emissive769 = staticSwitch766;
					

					o.Albedo = Output_Albedo627;
					o.Normal = Output_Normal742;

					half3 Specular = half3( 0, 0, 0 );
					half Metallic = Output_Metallic761;
					half Smoothness = Output_Smoothness745;
					half Occlusion = Output_AO765;

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

					o.Emission = Output_Emissive769;
					o.Alpha = 1;
					half AlphaClipThreshold = 0.5;
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
				#define ASE_NEEDS_TEXTURE_COORDINATES2
				#define ASE_NEEDS_VERT_TEXTURE_COORDINATES2
				#define ASE_NEEDS_TEXTURE_COORDINATES0
				#define ASE_NEEDS_TEXTURE_COORDINATES3
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES3
				#define ASE_NEEDS_FRAG_COLOR
				#define ASE_NEEDS_WORLD_TANGENT
				#define ASE_NEEDS_FRAG_WORLD_TANGENT
				#define ASE_NEEDS_WORLD_NORMAL
				#define ASE_NEEDS_FRAG_WORLD_NORMAL
				#define ASE_NEEDS_FRAG_WORLD_BITANGENT
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
				#define ASE_NEEDS_WORLD_POSITION
				#define ASE_NEEDS_FRAG_WORLD_POSITION
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLESTATICMESHSUPPORT_ON
				#pragma shader_feature_local _ENABLETOPLAYERBLEND_ON


				struct appdata
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					half4 tangent : TANGENT;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					float4 ase_texcoord3 : TEXCOORD3;
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
					float4 ase_texcoord5 : TEXCOORD5;
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

				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform half ASPW_WindTreeSpeed;
				uniform half ASPW_WindTreeAmplitude;
				uniform float _WindTreeFlexibility;
				uniform half ASPW_WindTreeFlexibility;
				uniform float _WindTreeBaseRigidity;
				uniform float ASPW_WindToggle;
				uniform sampler2D _BaseAlbedo;
				uniform float _BaseAlbedoDesaturation;
				uniform float _BaseAlbedoBrightness;
				uniform half4 _BaseAlbedoColor;
				uniform sampler2D _TopLayerAlbedo;
				uniform float _TopLayerUVScale;
				uniform half4 _TopLayerAlbedoColor;
				uniform sampler2D _BaseSMAE;
				uniform half _BaseBarkAOIntensity;
				uniform float _BaseTreeAOIntensity;
				uniform sampler2D _BaseNormal;
				uniform half _BaseNormalIntensity;
				uniform half _TopLayerOffset;
				uniform half ASPT_TopLayerOffset;
				uniform half _TopLayerContrast;
				uniform half ASPT_TopLayerContrast;
				uniform half _TopLayerIntensity;
				uniform half ASPT_TopLayerIntensity;
				uniform half _TopArrowDirectionOffset;
				uniform half ASPT_TopLayerArrowDirection;
				uniform float _TopLayerBottomOffset;
				uniform half ASPT_TopLayerBottomOffset;
				uniform half ASPT_TopLayerHeightStart;
				uniform float ASPT_TopLayerHeightFade;
				uniform sampler2D _TopLayerNormal;
				uniform half _TopLayerNormalIntensity;
				uniform half _TopLayerNormalInfluence;
				uniform half _BaseMetallicIntensity;
				uniform half _BaseSmoothnessMin;
				uniform half _BaseSmoothnessMax;
				uniform half _TopLayerSmoothnessMin;
				uniform half _TopLayerSmoothnessMax;
				uniform sampler2D _TopLayerSmoothness;
				uniform half3 _BaseEmissiveColor;
				uniform float _BaseEmissiveMaskContrast;
				uniform float _BaseEmissiveIntensity;


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
					float3 Global_Wind_Direction493_g22 = ASPW_WindDirection;
					float3 worldToObjDir269_g22 = mul( unity_WorldToObject, float4( Global_Wind_Direction493_g22, 0.0 ) ).xyz;
					float3 normalizeResult268_g22 = ASESafeNormalize( worldToObjDir269_g22 );
					float3 Wind_Direction_Leaf85_g22 = normalizeResult268_g22;
					float3 break86_g22 = Wind_Direction_Leaf85_g22;
					float3 appendResult89_g22 = (float3(break86_g22.z , 0.0 , ( break86_g22.x * -1.0 )));
					float3 Wind_Direction91_g22 = appendResult89_g22;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Tree_Randomization149_g22 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_56_0_g22 = ( Wind_Tree_Randomization149_g22 + _Time.y );
					float Global_Wind_Tree_Speed491_g22 = ASPW_WindTreeSpeed;
					float temp_output_213_0_g22 = ( Global_Wind_Tree_Speed491_g22 * 7.0 );
					float Global_Wind_Tree_Amplitude464_g22 = ASPW_WindTreeAmplitude;
					float Global_Wind_Tree_Flexibility490_g22 = ASPW_WindTreeFlexibility;
					float temp_output_383_0_g22 = ( ( _WindTreeFlexibility * Global_Wind_Tree_Flexibility490_g22 ) * 0.3 );
					float Wind_Tree_257_g22 = ( ( ( ( ( sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.1 ) ) ) + sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.3 ) ) ) + sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.5 ) ) ) ) + Global_Wind_Tree_Amplitude464_g22 ) * Global_Wind_Tree_Amplitude464_g22 ) * 0.01 ) * temp_output_383_0_g22 );
					float3 rotatedValue99_g22 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction91_g22, Wind_Tree_257_g22 );
					float3 Rotate_About_Axis354_g22 = ( rotatedValue99_g22 - v.vertex.xyz );
					float3 break452_g22 = normalizeResult268_g22;
					float3 appendResult454_g22 = (float3(break452_g22.x , 0.0 , break452_g22.z));
					float3 Wind_Direction_SM_Support453_g22 = appendResult454_g22;
					float Wind_Tree_Flexibility483_g22 = temp_output_383_0_g22;
					#ifdef _ENABLESTATICMESHSUPPORT_ON
					float3 staticSwitch482_g22 = ( Global_Wind_Tree_Amplitude464_g22 * Wind_Tree_257_g22 * Wind_Direction_SM_Support453_g22 * Wind_Tree_Flexibility483_g22 );
					#else
					float3 staticSwitch482_g22 = Rotate_About_Axis354_g22;
					#endif
					float3 Wind_Global450_g22 = staticSwitch482_g22;
					float2 texCoord433_g22 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float Wind_Mask_UV_CH2_VChannel434_g22 = texCoord433_g22.y;
					float saferPower113_g22 = abs( Wind_Mask_UV_CH2_VChannel434_g22 );
					float Wind_Tree_Mask114_g22 = pow( saferPower113_g22 , _WindTreeBaseRigidity );
					float3 temp_output_385_0_g22 = ( Wind_Global450_g22 * Wind_Tree_Mask114_g22 );
					float Global_Wind_Toggle504_g22 = ASPW_WindToggle;
					float3 lerpResult124_g22 = lerp( float3( 0,0,0 ) , temp_output_385_0_g22 , Global_Wind_Toggle504_g22);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch468_g22 = lerpResult124_g22;
					#else
					float3 staticSwitch468_g22 = temp_cast_0;
					#endif
					
					o.ase_texcoord4.xy = v.texcoord.xyzw.xy;
					o.ase_texcoord4.zw = v.ase_texcoord3.xy;
					o.ase_color = v.ase_color;
					o.ase_texcoord5.xy = v.texcoord2.xyzw.xy;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord5.zw = 0;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch468_g22;
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
					float4 ase_texcoord3 : TEXCOORD3;
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
					o.texcoord = v.texcoord;
					o.ase_texcoord3 = v.ase_texcoord3;
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
					o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
					o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
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

					float2 texCoord563 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
					float2 texCoord565 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
					float VColor_Blue_Tree_Mask561 = IN.ase_color.b;
					float3 lerpResult566 = lerp( tex2D( _BaseAlbedo, texCoord563 ).rgb , tex2D( _BaseAlbedo, texCoord565 ).rgb , VColor_Blue_Tree_Mask561);
					float3 desaturateInitialColor1070 = lerpResult566;
					float desaturateDot1070 = dot( desaturateInitialColor1070, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar1070 = lerp( desaturateInitialColor1070, desaturateDot1070.xxx, _BaseAlbedoDesaturation );
					float3 blendOpSrc345 = ( desaturateVar1070 * _BaseAlbedoBrightness );
					float3 blendOpDest345 = _BaseAlbedoColor.rgb;
					float3 Base_Albedo350 = ( saturate( (( blendOpDest345 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest345 ) * ( 1.0 - blendOpSrc345 ) ) : ( 2.0 * blendOpDest345 * blendOpSrc345 ) ) ));
					float2 texCoord594 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
					float2 Top_Layer_UV_Scale597 = ( texCoord594 * _TopLayerUVScale );
					float3 blendOpSrc599 = tex2D( _TopLayerAlbedo, Top_Layer_UV_Scale597 ).rgb;
					float3 blendOpDest599 = _TopLayerAlbedoColor.rgb;
					float2 texCoord580 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
					float4 tex2DNode577 = tex2D( _BaseSMAE, texCoord580 );
					float2 texCoord581 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
					float4 tex2DNode576 = tex2D( _BaseSMAE, texCoord581 );
					float3 lerpResult579 = lerp( tex2DNode577.rgb , tex2DNode576.rgb , VColor_Blue_Tree_Mask561);
					float3 break582 = lerpResult579;
					float lerpResult365 = lerp( 1.0 , break582.z , _BaseBarkAOIntensity);
					float lerpResult559 = lerp( 1.0 , IN.ase_color.a , _BaseTreeAOIntensity);
					float VColor_Alpha_Tree_AO562 = lerpResult559;
					float Base_AO348 = ( lerpResult365 * VColor_Alpha_Tree_AO562 );
					float2 texCoord571 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
					float2 texCoord572 = IN.ase_texcoord4.zw * float2( 1,1 ) + float2( 0,0 );
					float3 lerpResult573 = lerp( UnpackScaleNormal( tex2D( _BaseNormal, texCoord571 ), _BaseNormalIntensity ) , UnpackScaleNormal( tex2D( _BaseNormal, texCoord572 ), _BaseNormalIntensity ) , VColor_Blue_Tree_Mask561);
					float3 Base_Normal355 = lerpResult573;
					float3 tanToWorld0 = float3( TangentWS.x, BitangentWS.x, NormalWS.x );
					float3 tanToWorld1 = float3( TangentWS.y, BitangentWS.y, NormalWS.y );
					float3 tanToWorld2 = float3( TangentWS.z, BitangentWS.z, NormalWS.z );
					float3 tanNormal776 = Base_Normal355;
					float3 worldNormal776 = float3( dot( tanToWorld0, tanNormal776 ), dot( tanToWorld1, tanNormal776 ), dot( tanToWorld2, tanNormal776 ) );
					float temp_output_616_0 = ( _TopLayerOffset * ASPT_TopLayerOffset );
					float saferPower17_g23 = abs( abs( ( saturate( worldNormal776.y ) + temp_output_616_0 ) ) );
					float temp_output_617_0 = ( _TopLayerContrast * ASPT_TopLayerContrast );
					float temp_output_615_0 = ( _TopLayerIntensity * ASPT_TopLayerIntensity );
					float3 tanNormal629 = Base_Normal355;
					float3 worldNormal629 = float3( dot( tanToWorld0, tanNormal629 ), dot( tanToWorld1, tanNormal629 ), dot( tanToWorld2, tanNormal629 ) );
					float3 Global_Arrow_Direction655 = ASPW_WindDirection;
					float dotResult633 = dot( worldNormal629 , Global_Arrow_Direction655 );
					float dotResult682 = dot( worldNormal629 , float3( 0, 1, 0 ) );
					float2 texCoord668 = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
					float Arrow_Direction_Mask693 = ( ( ( 1.0 - dotResult633 ) * ( _TopArrowDirectionOffset * ASPT_TopLayerArrowDirection ) ) * ( ( ( 3.0 - ( 1.0 - dotResult682 ) ) * ( 1.0 - texCoord668.y ) ) + 0.05 ) );
					float2 texCoord705 = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
					float temp_output_706_0 = ( 1.0 - texCoord705.y );
					float temp_output_711_0 = ( ( ( temp_output_706_0 * temp_output_706_0 ) * ( temp_output_706_0 * temp_output_706_0 ) ) * _TopLayerBottomOffset * ASPT_TopLayerBottomOffset );
					float3 tanNormal715 = Base_Normal355;
					float3 worldNormal715 = float3( dot( tanToWorld0, tanNormal715 ), dot( tanToWorld1, tanNormal715 ), dot( tanToWorld2, tanNormal715 ) );
					float dotResult716 = dot( worldNormal715 , float3( 0, 1, 0 ) );
					float Top_Layer_Bottom_Offset721 = ( ( temp_output_711_0 * temp_output_711_0 ) + ( dotResult716 - 1.0 ) );
					float clampResult1046 = clamp( ( Arrow_Direction_Mask693 + Top_Layer_Bottom_Offset721 ) , 0.0 , 5.0 );
					float Top_Layer_Offset696 = temp_output_616_0;
					float saferPower698 = abs( ( clampResult1046 * Top_Layer_Offset696 ) );
					float Top_Layer_Contrast699 = temp_output_617_0;
					float Top_Layer_Intensity702 = temp_output_615_0;
					float Top_Layer_Direction1057 = saturate( ( pow( saferPower698 , Top_Layer_Contrast699 ) * Top_Layer_Intensity702 ) );
					float Top_Layer_Mask621 = saturate( ( ( ( saturate( pow( saferPower17_g23 , temp_output_617_0 ) ) * temp_output_615_0 ) + Top_Layer_Direction1057 ) * saturate(  (0.0 + ( PositionWS.y - ASPT_TopLayerHeightStart ) * ( 1.0 - 0.0 ) / ( ( ASPT_TopLayerHeightStart + ASPT_TopLayerHeightFade ) - ASPT_TopLayerHeightStart ) ) ) ) );
					float3 lerpResult605 = lerp( Base_Albedo350 , ( (( blendOpDest599 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest599 ) * ( 1.0 - blendOpSrc599 ) ) : ( 2.0 * blendOpDest599 * blendOpSrc599 ) ) * Base_AO348 ) , Top_Layer_Mask621);
					float3 Top_Layer_Albedo625 = lerpResult605;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch623 = Top_Layer_Albedo625;
					#else
					float3 staticSwitch623 = Base_Albedo350;
					#endif
					float3 Output_Albedo627 = staticSwitch623;
					
					float3 tex2DNode646 = UnpackScaleNormal( tex2D( _TopLayerNormal, Top_Layer_UV_Scale597 ), _TopLayerNormalIntensity );
					float3 lerpResult649 = lerp( BlendNormals( tex2DNode646 , Base_Normal355 ) , tex2DNode646 , _TopLayerNormalInfluence);
					float3 lerpResult652 = lerp( Base_Normal355 , lerpResult649 , Top_Layer_Mask621);
					float3 Top_Layer_Normal653 = lerpResult652;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch738 = Top_Layer_Normal653;
					#else
					float3 staticSwitch738 = Base_Normal355;
					#endif
					float3 Output_Normal742 = staticSwitch738;
					
					float Base_Metallic364 = ( break582.y * _BaseMetallicIntensity );
					float lerpResult747 = lerp( Base_Metallic364 , 0.0 , Top_Layer_Mask621);
					float Top_Layer_Metallic749 = lerpResult747;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float staticSwitch758 = Top_Layer_Metallic749;
					#else
					float staticSwitch758 = Base_Metallic364;
					#endif
					float Output_Metallic761 = staticSwitch758;
					
					float Texture_Smoothness1079 = break582.x;
					float lerpResult1082 = lerp( _BaseSmoothnessMin , _BaseSmoothnessMax , Texture_Smoothness1079);
					float Base_Smoothness361 = saturate( lerpResult1082 );
					float lerpResult1086 = lerp( _TopLayerSmoothnessMin , _TopLayerSmoothnessMax , tex2D( _TopLayerSmoothness, Top_Layer_UV_Scale597 ).r);
					float lerpResult734 = lerp( Base_Smoothness361 , lerpResult1086 , Top_Layer_Mask621);
					float Top_Layer_Smoothness737 = saturate( lerpResult734 );
					#ifdef _ENABLETOPLAYERBLEND_ON
					float staticSwitch741 = Top_Layer_Smoothness737;
					#else
					float staticSwitch741 = Base_Smoothness361;
					#endif
					float Output_Smoothness745 = staticSwitch741;
					
					float lerpResult751 = lerp( Base_AO348 , 1.0 , Top_Layer_Mask621);
					float Top_Layer_AO753 = lerpResult751;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float staticSwitch762 = Top_Layer_AO753;
					#else
					float staticSwitch762 = Base_AO348;
					#endif
					float Output_AO765 = staticSwitch762;
					
					float lerpResult587 = lerp( tex2DNode577.a , tex2DNode576.a , VColor_Blue_Tree_Mask561);
					float Emissive_Mask371 = lerpResult587;
					float saferPower373 = abs( Emissive_Mask371 );
					float3 Base_Emissive377 = ( ( _BaseEmissiveColor * pow( saferPower373 , _BaseEmissiveMaskContrast ) ) * _BaseEmissiveIntensity );
					float3 lerpResult755 = lerp( Base_Emissive377 , float3( 0,0,0 ) , Top_Layer_Mask621);
					float3 Top_Layer_Emissive757 = lerpResult755;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch766 = Top_Layer_Emissive757;
					#else
					float3 staticSwitch766 = Base_Emissive377;
					#endif
					float3 Output_Emissive769 = staticSwitch766;
					

					o.Albedo = Output_Albedo627;
					o.Normal = Output_Normal742;

					half3 Specular = half3( 0, 0, 0 );
					half Metallic = Output_Metallic761;
					half Smoothness = Output_Smoothness745;
					half Occlusion = Output_AO765;

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

					o.Emission = Output_Emissive769;
					o.Alpha = 1;
					half AlphaClipThreshold = 0.5;
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
				#define ASE_NEEDS_TEXTURE_COORDINATES3
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES3
				#define ASE_NEEDS_FRAG_COLOR
				#define ASE_NEEDS_VERT_TANGENT
				#define ASE_NEEDS_VERT_NORMAL
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES2
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLESTATICMESHSUPPORT_ON
				#pragma shader_feature_local _ENABLETOPLAYERBLEND_ON


				struct appdata
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					half4 tangent : TANGENT;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					float4 ase_texcoord3 : TEXCOORD3;
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
					float4 ase_texcoord7 : TEXCOORD7;
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

				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform half ASPW_WindTreeSpeed;
				uniform half ASPW_WindTreeAmplitude;
				uniform float _WindTreeFlexibility;
				uniform half ASPW_WindTreeFlexibility;
				uniform float _WindTreeBaseRigidity;
				uniform float ASPW_WindToggle;
				uniform sampler2D _BaseAlbedo;
				uniform float _BaseAlbedoDesaturation;
				uniform float _BaseAlbedoBrightness;
				uniform half4 _BaseAlbedoColor;
				uniform sampler2D _TopLayerAlbedo;
				uniform float _TopLayerUVScale;
				uniform half4 _TopLayerAlbedoColor;
				uniform sampler2D _BaseSMAE;
				uniform half _BaseBarkAOIntensity;
				uniform float _BaseTreeAOIntensity;
				uniform sampler2D _BaseNormal;
				uniform half _BaseNormalIntensity;
				uniform half _TopLayerOffset;
				uniform half ASPT_TopLayerOffset;
				uniform half _TopLayerContrast;
				uniform half ASPT_TopLayerContrast;
				uniform half _TopLayerIntensity;
				uniform half ASPT_TopLayerIntensity;
				uniform half _TopArrowDirectionOffset;
				uniform half ASPT_TopLayerArrowDirection;
				uniform float _TopLayerBottomOffset;
				uniform half ASPT_TopLayerBottomOffset;
				uniform half ASPT_TopLayerHeightStart;
				uniform float ASPT_TopLayerHeightFade;
				uniform half3 _BaseEmissiveColor;
				uniform float _BaseEmissiveMaskContrast;
				uniform float _BaseEmissiveIntensity;


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
					float3 Global_Wind_Direction493_g22 = ASPW_WindDirection;
					float3 worldToObjDir269_g22 = mul( unity_WorldToObject, float4( Global_Wind_Direction493_g22, 0.0 ) ).xyz;
					float3 normalizeResult268_g22 = ASESafeNormalize( worldToObjDir269_g22 );
					float3 Wind_Direction_Leaf85_g22 = normalizeResult268_g22;
					float3 break86_g22 = Wind_Direction_Leaf85_g22;
					float3 appendResult89_g22 = (float3(break86_g22.z , 0.0 , ( break86_g22.x * -1.0 )));
					float3 Wind_Direction91_g22 = appendResult89_g22;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Tree_Randomization149_g22 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_56_0_g22 = ( Wind_Tree_Randomization149_g22 + _Time.y );
					float Global_Wind_Tree_Speed491_g22 = ASPW_WindTreeSpeed;
					float temp_output_213_0_g22 = ( Global_Wind_Tree_Speed491_g22 * 7.0 );
					float Global_Wind_Tree_Amplitude464_g22 = ASPW_WindTreeAmplitude;
					float Global_Wind_Tree_Flexibility490_g22 = ASPW_WindTreeFlexibility;
					float temp_output_383_0_g22 = ( ( _WindTreeFlexibility * Global_Wind_Tree_Flexibility490_g22 ) * 0.3 );
					float Wind_Tree_257_g22 = ( ( ( ( ( sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.1 ) ) ) + sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.3 ) ) ) + sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.5 ) ) ) ) + Global_Wind_Tree_Amplitude464_g22 ) * Global_Wind_Tree_Amplitude464_g22 ) * 0.01 ) * temp_output_383_0_g22 );
					float3 rotatedValue99_g22 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction91_g22, Wind_Tree_257_g22 );
					float3 Rotate_About_Axis354_g22 = ( rotatedValue99_g22 - v.vertex.xyz );
					float3 break452_g22 = normalizeResult268_g22;
					float3 appendResult454_g22 = (float3(break452_g22.x , 0.0 , break452_g22.z));
					float3 Wind_Direction_SM_Support453_g22 = appendResult454_g22;
					float Wind_Tree_Flexibility483_g22 = temp_output_383_0_g22;
					#ifdef _ENABLESTATICMESHSUPPORT_ON
					float3 staticSwitch482_g22 = ( Global_Wind_Tree_Amplitude464_g22 * Wind_Tree_257_g22 * Wind_Direction_SM_Support453_g22 * Wind_Tree_Flexibility483_g22 );
					#else
					float3 staticSwitch482_g22 = Rotate_About_Axis354_g22;
					#endif
					float3 Wind_Global450_g22 = staticSwitch482_g22;
					float2 texCoord433_g22 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float Wind_Mask_UV_CH2_VChannel434_g22 = texCoord433_g22.y;
					float saferPower113_g22 = abs( Wind_Mask_UV_CH2_VChannel434_g22 );
					float Wind_Tree_Mask114_g22 = pow( saferPower113_g22 , _WindTreeBaseRigidity );
					float3 temp_output_385_0_g22 = ( Wind_Global450_g22 * Wind_Tree_Mask114_g22 );
					float Global_Wind_Toggle504_g22 = ASPW_WindToggle;
					float3 lerpResult124_g22 = lerp( float3( 0,0,0 ) , temp_output_385_0_g22 , Global_Wind_Toggle504_g22);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch468_g22 = lerpResult124_g22;
					#else
					float3 staticSwitch468_g22 = temp_cast_0;
					#endif
					
					float3 ase_tangentWS = UnityObjectToWorldDir( v.tangent );
					o.ase_texcoord3.xyz = ase_tangentWS;
					float3 ase_normalWS = UnityObjectToWorldNormal( v.normal );
					o.ase_texcoord4.xyz = ase_normalWS;
					float ase_tangentSign = v.tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
					float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
					o.ase_texcoord5.xyz = ase_bitangentWS;
					float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( v.vertex ).xyz, 1 ) ).xyz;
					o.ase_texcoord7.xyz = ase_positionWS;
					
					o.ase_texcoord2.xy = v.texcoord.xyzw.xy;
					o.ase_texcoord2.zw = v.ase_texcoord3.xy;
					o.ase_color = v.ase_color;
					o.ase_texcoord6.xy = v.texcoord2.xyzw.xy;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;
					o.ase_texcoord4.w = 0;
					o.ase_texcoord5.w = 0;
					o.ase_texcoord6.zw = 0;
					o.ase_texcoord7.w = 0;

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch468_g22;
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
					float4 ase_texcoord3 : TEXCOORD3;
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
					o.texcoord = v.texcoord;
					o.ase_texcoord3 = v.ase_texcoord3;
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
					o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
					o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
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

					float2 texCoord563 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
					float2 texCoord565 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
					float VColor_Blue_Tree_Mask561 = IN.ase_color.b;
					float3 lerpResult566 = lerp( tex2D( _BaseAlbedo, texCoord563 ).rgb , tex2D( _BaseAlbedo, texCoord565 ).rgb , VColor_Blue_Tree_Mask561);
					float3 desaturateInitialColor1070 = lerpResult566;
					float desaturateDot1070 = dot( desaturateInitialColor1070, float3( 0.299, 0.587, 0.114 ));
					float3 desaturateVar1070 = lerp( desaturateInitialColor1070, desaturateDot1070.xxx, _BaseAlbedoDesaturation );
					float3 blendOpSrc345 = ( desaturateVar1070 * _BaseAlbedoBrightness );
					float3 blendOpDest345 = _BaseAlbedoColor.rgb;
					float3 Base_Albedo350 = ( saturate( (( blendOpDest345 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest345 ) * ( 1.0 - blendOpSrc345 ) ) : ( 2.0 * blendOpDest345 * blendOpSrc345 ) ) ));
					float2 texCoord594 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
					float2 Top_Layer_UV_Scale597 = ( texCoord594 * _TopLayerUVScale );
					float3 blendOpSrc599 = tex2D( _TopLayerAlbedo, Top_Layer_UV_Scale597 ).rgb;
					float3 blendOpDest599 = _TopLayerAlbedoColor.rgb;
					float2 texCoord580 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
					float4 tex2DNode577 = tex2D( _BaseSMAE, texCoord580 );
					float2 texCoord581 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
					float4 tex2DNode576 = tex2D( _BaseSMAE, texCoord581 );
					float3 lerpResult579 = lerp( tex2DNode577.rgb , tex2DNode576.rgb , VColor_Blue_Tree_Mask561);
					float3 break582 = lerpResult579;
					float lerpResult365 = lerp( 1.0 , break582.z , _BaseBarkAOIntensity);
					float lerpResult559 = lerp( 1.0 , IN.ase_color.a , _BaseTreeAOIntensity);
					float VColor_Alpha_Tree_AO562 = lerpResult559;
					float Base_AO348 = ( lerpResult365 * VColor_Alpha_Tree_AO562 );
					float2 texCoord571 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
					float2 texCoord572 = IN.ase_texcoord2.zw * float2( 1,1 ) + float2( 0,0 );
					float3 lerpResult573 = lerp( UnpackScaleNormal( tex2D( _BaseNormal, texCoord571 ), _BaseNormalIntensity ) , UnpackScaleNormal( tex2D( _BaseNormal, texCoord572 ), _BaseNormalIntensity ) , VColor_Blue_Tree_Mask561);
					float3 Base_Normal355 = lerpResult573;
					float3 ase_tangentWS = IN.ase_texcoord3.xyz;
					float3 ase_normalWS = IN.ase_texcoord4.xyz;
					float3 ase_bitangentWS = IN.ase_texcoord5.xyz;
					float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
					float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
					float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
					float3 tanNormal776 = Base_Normal355;
					float3 worldNormal776 = float3( dot( tanToWorld0, tanNormal776 ), dot( tanToWorld1, tanNormal776 ), dot( tanToWorld2, tanNormal776 ) );
					float temp_output_616_0 = ( _TopLayerOffset * ASPT_TopLayerOffset );
					float saferPower17_g23 = abs( abs( ( saturate( worldNormal776.y ) + temp_output_616_0 ) ) );
					float temp_output_617_0 = ( _TopLayerContrast * ASPT_TopLayerContrast );
					float temp_output_615_0 = ( _TopLayerIntensity * ASPT_TopLayerIntensity );
					float3 tanNormal629 = Base_Normal355;
					float3 worldNormal629 = float3( dot( tanToWorld0, tanNormal629 ), dot( tanToWorld1, tanNormal629 ), dot( tanToWorld2, tanNormal629 ) );
					float3 Global_Arrow_Direction655 = ASPW_WindDirection;
					float dotResult633 = dot( worldNormal629 , Global_Arrow_Direction655 );
					float dotResult682 = dot( worldNormal629 , float3( 0, 1, 0 ) );
					float2 texCoord668 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
					float Arrow_Direction_Mask693 = ( ( ( 1.0 - dotResult633 ) * ( _TopArrowDirectionOffset * ASPT_TopLayerArrowDirection ) ) * ( ( ( 3.0 - ( 1.0 - dotResult682 ) ) * ( 1.0 - texCoord668.y ) ) + 0.05 ) );
					float2 texCoord705 = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
					float temp_output_706_0 = ( 1.0 - texCoord705.y );
					float temp_output_711_0 = ( ( ( temp_output_706_0 * temp_output_706_0 ) * ( temp_output_706_0 * temp_output_706_0 ) ) * _TopLayerBottomOffset * ASPT_TopLayerBottomOffset );
					float3 tanNormal715 = Base_Normal355;
					float3 worldNormal715 = float3( dot( tanToWorld0, tanNormal715 ), dot( tanToWorld1, tanNormal715 ), dot( tanToWorld2, tanNormal715 ) );
					float dotResult716 = dot( worldNormal715 , float3( 0, 1, 0 ) );
					float Top_Layer_Bottom_Offset721 = ( ( temp_output_711_0 * temp_output_711_0 ) + ( dotResult716 - 1.0 ) );
					float clampResult1046 = clamp( ( Arrow_Direction_Mask693 + Top_Layer_Bottom_Offset721 ) , 0.0 , 5.0 );
					float Top_Layer_Offset696 = temp_output_616_0;
					float saferPower698 = abs( ( clampResult1046 * Top_Layer_Offset696 ) );
					float Top_Layer_Contrast699 = temp_output_617_0;
					float Top_Layer_Intensity702 = temp_output_615_0;
					float Top_Layer_Direction1057 = saturate( ( pow( saferPower698 , Top_Layer_Contrast699 ) * Top_Layer_Intensity702 ) );
					float3 ase_positionWS = IN.ase_texcoord7.xyz;
					float Top_Layer_Mask621 = saturate( ( ( ( saturate( pow( saferPower17_g23 , temp_output_617_0 ) ) * temp_output_615_0 ) + Top_Layer_Direction1057 ) * saturate(  (0.0 + ( ase_positionWS.y - ASPT_TopLayerHeightStart ) * ( 1.0 - 0.0 ) / ( ( ASPT_TopLayerHeightStart + ASPT_TopLayerHeightFade ) - ASPT_TopLayerHeightStart ) ) ) ) );
					float3 lerpResult605 = lerp( Base_Albedo350 , ( (( blendOpDest599 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest599 ) * ( 1.0 - blendOpSrc599 ) ) : ( 2.0 * blendOpDest599 * blendOpSrc599 ) ) * Base_AO348 ) , Top_Layer_Mask621);
					float3 Top_Layer_Albedo625 = lerpResult605;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch623 = Top_Layer_Albedo625;
					#else
					float3 staticSwitch623 = Base_Albedo350;
					#endif
					float3 Output_Albedo627 = staticSwitch623;
					
					float lerpResult587 = lerp( tex2DNode577.a , tex2DNode576.a , VColor_Blue_Tree_Mask561);
					float Emissive_Mask371 = lerpResult587;
					float saferPower373 = abs( Emissive_Mask371 );
					float3 Base_Emissive377 = ( ( _BaseEmissiveColor * pow( saferPower373 , _BaseEmissiveMaskContrast ) ) * _BaseEmissiveIntensity );
					float3 lerpResult755 = lerp( Base_Emissive377 , float3( 0,0,0 ) , Top_Layer_Mask621);
					float3 Top_Layer_Emissive757 = lerpResult755;
					#ifdef _ENABLETOPLAYERBLEND_ON
					float3 staticSwitch766 = Top_Layer_Emissive757;
					#else
					float3 staticSwitch766 = Base_Emissive377;
					#endif
					float3 Output_Emissive769 = staticSwitch766;
					

					o.Albedo = Output_Albedo627;
					o.Normal = half3( 0, 0, 1 );
					o.Emission = Output_Emissive769;
					o.Alpha = 1;
					half AlphaClipThreshold = 0.5;

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
				#pragma shader_feature_local _ENABLEWIND_ON
				#pragma shader_feature_local _ENABLESTATICMESHSUPPORT_ON


				struct appdata
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					half4 tangent : TANGENT;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord2 : TEXCOORD2;
					
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					V2F_SHADOW_CASTER;
					
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

				uniform sampler2D ASP_GlobalTintNoiseTexture;
				uniform half3 ASPW_WindDirection;
				uniform half ASPW_WindTreeSpeed;
				uniform half ASPW_WindTreeAmplitude;
				uniform float _WindTreeFlexibility;
				uniform half ASPW_WindTreeFlexibility;
				uniform float _WindTreeBaseRigidity;
				uniform float ASPW_WindToggle;


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
					float3 Global_Wind_Direction493_g22 = ASPW_WindDirection;
					float3 worldToObjDir269_g22 = mul( unity_WorldToObject, float4( Global_Wind_Direction493_g22, 0.0 ) ).xyz;
					float3 normalizeResult268_g22 = ASESafeNormalize( worldToObjDir269_g22 );
					float3 Wind_Direction_Leaf85_g22 = normalizeResult268_g22;
					float3 break86_g22 = Wind_Direction_Leaf85_g22;
					float3 appendResult89_g22 = (float3(break86_g22.z , 0.0 , ( break86_g22.x * -1.0 )));
					float3 Wind_Direction91_g22 = appendResult89_g22;
					float3 ase_objectPosition = UNITY_MATRIX_M._m03_m13_m23;
					float Wind_Tree_Randomization149_g22 = frac( ( ( ase_objectPosition.x + ase_objectPosition.y + ase_objectPosition.z ) * 1.23 ) );
					float temp_output_56_0_g22 = ( Wind_Tree_Randomization149_g22 + _Time.y );
					float Global_Wind_Tree_Speed491_g22 = ASPW_WindTreeSpeed;
					float temp_output_213_0_g22 = ( Global_Wind_Tree_Speed491_g22 * 7.0 );
					float Global_Wind_Tree_Amplitude464_g22 = ASPW_WindTreeAmplitude;
					float Global_Wind_Tree_Flexibility490_g22 = ASPW_WindTreeFlexibility;
					float temp_output_383_0_g22 = ( ( _WindTreeFlexibility * Global_Wind_Tree_Flexibility490_g22 ) * 0.3 );
					float Wind_Tree_257_g22 = ( ( ( ( ( sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.1 ) ) ) + sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.3 ) ) ) + sin( ( temp_output_56_0_g22 * ( temp_output_213_0_g22 * 0.5 ) ) ) ) + Global_Wind_Tree_Amplitude464_g22 ) * Global_Wind_Tree_Amplitude464_g22 ) * 0.01 ) * temp_output_383_0_g22 );
					float3 rotatedValue99_g22 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, Wind_Direction91_g22, Wind_Tree_257_g22 );
					float3 Rotate_About_Axis354_g22 = ( rotatedValue99_g22 - v.vertex.xyz );
					float3 break452_g22 = normalizeResult268_g22;
					float3 appendResult454_g22 = (float3(break452_g22.x , 0.0 , break452_g22.z));
					float3 Wind_Direction_SM_Support453_g22 = appendResult454_g22;
					float Wind_Tree_Flexibility483_g22 = temp_output_383_0_g22;
					#ifdef _ENABLESTATICMESHSUPPORT_ON
					float3 staticSwitch482_g22 = ( Global_Wind_Tree_Amplitude464_g22 * Wind_Tree_257_g22 * Wind_Direction_SM_Support453_g22 * Wind_Tree_Flexibility483_g22 );
					#else
					float3 staticSwitch482_g22 = Rotate_About_Axis354_g22;
					#endif
					float3 Wind_Global450_g22 = staticSwitch482_g22;
					float2 texCoord433_g22 = v.texcoord2.xyzw.xy * float2( 1,1 ) + float2( 0,0 );
					float Wind_Mask_UV_CH2_VChannel434_g22 = texCoord433_g22.y;
					float saferPower113_g22 = abs( Wind_Mask_UV_CH2_VChannel434_g22 );
					float Wind_Tree_Mask114_g22 = pow( saferPower113_g22 , _WindTreeBaseRigidity );
					float3 temp_output_385_0_g22 = ( Wind_Global450_g22 * Wind_Tree_Mask114_g22 );
					float Global_Wind_Toggle504_g22 = ASPW_WindToggle;
					float3 lerpResult124_g22 = lerp( float3( 0,0,0 ) , temp_output_385_0_g22 , Global_Wind_Toggle504_g22);
					#ifdef _ENABLEWIND_ON
					float3 staticSwitch468_g22 = lerpResult124_g22;
					#else
					float3 staticSwitch468_g22 = temp_cast_0;
					#endif
					

					#ifdef ASE_ABSOLUTE_VERTEX_POS
						float3 defaultVertexValue = v.vertex.xyz;
					#else
						float3 defaultVertexValue = float3(0, 0, 0);
					#endif
					float3 vertexValue = staticSwitch468_g22;
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

					

					o.Normal = half3( 0, 0, 1 );

					o.Alpha = 1;
					half AlphaClipThreshold = 0.5;
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
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1059;-560,4560;Inherit;False;1988;291;;12;694;725;697;724;1046;695;703;701;782;700;698;1057;Top Layer Mask - Bottom + Direction;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;974;5313,-3122;Inherit;False;1021.699;815.5093;;6;593;592;591;590;589;588;Output;1,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;977;-576,3536;Inherit;False;2110;811;;16;721;778;710;719;713;718;711;716;709;717;715;708;707;714;706;705;Top Layer Mask - Bottom Offset;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;976;-576,2384;Inherit;False;2233;1006;;20;693;631;632;671;692;637;690;636;635;691;666;633;668;689;663;630;682;629;683;628;Top Layer Mask - Arrow Direction;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;975;-574.2462,1104;Inherit;False;2492.488;934.5208;;20;621;704;1054;1058;657;1056;608;609;613;610;699;776;617;611;614;702;696;615;616;612;Top Layer Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;973;3140,-3122;Inherit;False;1083;1552;;24;743;744;765;762;764;763;769;761;745;742;627;766;758;741;738;623;768;767;760;759;740;739;626;624;Switches;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;972;-575.3077,592;Inherit;False;829.8768;302.2256;;4;597;596;595;594;Top Layer UVs;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;969;-574.2872,64;Inherit;False;1089.959;319.5745;;4;757;755;756;754;Top Layer Emissive;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;968;-576,-448;Inherit;False;1086.533;320.595;;4;753;751;752;750;Top Layer AO;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;967;-574.2872,-944;Inherit;False;1084.492;306.3078;;4;749;747;748;746;Top Layer Metallic;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;966;-572,-1584;Inherit;False;2236;468;;10;1086;1085;1084;736;735;949;734;730;737;731;Top Layer Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;965;-573,-2224;Inherit;False;2237.935;432.6967;;11;644;648;646;653;652;650;651;649;647;645;643;Top Layer Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;964;-571,-2992;Inherit;False;1979;558;;10;601;600;625;605;607;606;604;599;603;602;Top Layer Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;963;-4016,1232;Inherit;False;570;238;;2;655;654;Global Arrow Direction;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;962;-4032,720;Inherit;False;960;341;Comment;5;561;562;783;559;558;VColor Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;961;-4032,-48;Inherit;False;1467;561;;8;376;374;377;375;370;369;373;372;Base Emissive;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;960;-4030,-1328;Inherit;False;2364;1063;[R] Smoothness | [G] Metallic | [B] AmbientOcclusion | [A] Emissive;25;363;367;1083;1080;950;1082;1081;361;1079;348;575;576;364;362;371;587;583;584;365;582;579;578;577;581;580;Base SMAE;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;959;-4031,-2224;Inherit;False;1723;687;;9;568;354;355;573;574;570;569;572;571;Base Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;958;-4030,-2994;Inherit;False;2370;594;;14;350;345;481;1069;1072;1068;1070;557;566;567;556;564;563;565;Base Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;565;-3968,-2560;Inherit;False;3;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;563;-3968,-2688;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;564;-3456,-2752;Inherit;True;Property;_TextureSample1;Texture Sample 0;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;556;-3456,-2944;Inherit;True;Property;_TextureSample0;Texture Sample 0;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;567;-3456,-2560;Inherit;False;561;VColor Blue Tree Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;566;-2944,-2944;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;571;-3968,-1920;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;572;-3968,-1792;Inherit;False;3;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;569;-3456,-2176;Inherit;True;Property;_TextureSample2;Texture Sample 0;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;570;-3456,-1952;Inherit;True;Property;_TextureSample3;Texture Sample 0;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;574;-3456,-1760;Inherit;False;561;VColor Blue Tree Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;573;-2944,-2176;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;355;-2560,-2176;Inherit;False;Base Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;354;-3968,-1664;Half;False;Property;_BaseNormalIntensity;Base Normal Intensity;6;0;Create;True;0;0;0;False;0;False;1;1.22;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;568;-3968,-2176;Inherit;True;Property;_BaseNormal;Base Normal;13;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;None;a4739660e9799a44ab3ef54b5e0573ae;True;bump;Auto;Texture2D;False;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;580;-3968,-1024;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;581;-3968,-896;Inherit;False;3;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;577;-3456,-1280;Inherit;True;Property;_TextureSample5;Texture Sample 0;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;578;-3456,-768;Inherit;False;561;VColor Blue Tree Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;579;-2944,-1280;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;582;-2688,-1280;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;365;-2432,-1024;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;584;-2432,-896;Inherit;False;562;VColor Alpha Tree AO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;583;-2176,-1024;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;587;-2944,-768;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;371;-2432,-768;Inherit;False;Emissive Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;362;-2432,-1152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;364;-1920,-1152;Inherit;False;Base Metallic;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;576;-3456,-1088;Inherit;True;Property;_TextureSample4;Texture Sample 0;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;575;-3968,-1280;Inherit;True;Property;_BaseSMAE;Base SMAE;14;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;None;f78e6efa310d4d741a29de6228c8fb80;False;gray;Auto;Texture2D;False;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;348;-1920,-1024;Inherit;False;Base AO;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;602;-512,-2944;Inherit;False;597;Top Layer UV Scale;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;603;384,-2816;Inherit;False;348;Base AO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;599;384,-2944;Inherit;False;Overlay;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;604;640,-2944;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;606;640,-2816;Inherit;False;350;Base Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;607;640,-2688;Inherit;False;621;Top Layer Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;605;896,-2944;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;625;1152,-2944;Inherit;False;Top Layer Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;600;0,-2944;Inherit;True;Property;_TopLayerAlbedo;Top Layer Albedo;27;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;f6dc7c0cfcd8cb54cb5f4dbe15bb7047;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;601;0,-2688;Half;False;Property;_TopLayerAlbedoColor;Top Layer Albedo Color;16;1;[HDR];Create;True;0;0;0;False;0;False;0.5019608,0.5019608,0.5019608,0.5019608;0.5019608,0.5019608,0.5019608,0.5019608;False;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;643;-512,-2176;Inherit;False;597;Top Layer UV Scale;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;645;0,-1920;Inherit;False;355;Base Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendNormalsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;647;384,-2048;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;649;768,-2176;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;651;768,-2048;Inherit;False;355;Base Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;650;768,-1920;Inherit;False;621;Top Layer Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;652;1152,-2176;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;653;1408,-2176;Inherit;False;Top Layer Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;646;0,-2176;Inherit;True;Property;_TopLayerNormal;Top Layer Normal;28;2;[NoScaleOffset];[Normal];Create;True;0;0;0;False;0;False;-1;None;ce33c7bbfcc6ff044979c54b9fb01fae;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;648;384,-1920;Half;False;Property;_TopLayerNormalInfluence;Top Layer Normal Influence;21;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;644;-512,-2048;Half;False;Property;_TopLayerNormalIntensity;Top Layer Normal Intensity;20;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;731;-512,-1536;Inherit;False;597;Top Layer UV Scale;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;737;1408,-1536;Inherit;False;Top Layer Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;730;0,-1536;Inherit;True;Property;_TopLayerSmoothness;Top Layer Smoothness;29;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;8e8ac0c4acbd18f45891d51182feebda;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;746;-512,-896;Inherit;False;364;Base Metallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;748;-512,-768;Inherit;False;621;Top Layer Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;747;-128,-896;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;749;256,-896;Inherit;False;Top Layer Metallic;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;750;-512,-384;Inherit;False;348;Base AO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;752;-512,-256;Inherit;False;621;Top Layer Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;751;-128,-384;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;753;256,-384;Inherit;False;Top Layer AO;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;754;-512,128;Inherit;False;377;Base Emissive;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;756;-512,256;Inherit;False;621;Top Layer Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;755;-128,128;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;757;256,128;Inherit;False;Top Layer Emissive;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;594;-512,640;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;595;-512,768;Inherit;False;Property;_TopLayerUVScale;Top Layer UV Scale;17;0;Create;True;0;0;0;False;0;False;1;6.2;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;596;-256,640;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;597;0,640;Inherit;False;Top Layer UV Scale;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;624;3200,-3072;Inherit;False;350;Base Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;626;3200,-2976;Inherit;False;625;Top Layer Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;739;3200,-2816;Inherit;False;355;Base Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;740;3200,-2720;Inherit;False;653;Top Layer Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;759;3200,-2304;Inherit;False;364;Base Metallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;760;3200,-2208;Inherit;False;749;Top Layer Metallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;767;3200,-1792;Inherit;False;377;Base Emissive;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;768;3200,-1696;Inherit;False;757;Top Layer Emissive;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;738;3584,-2816;Inherit;False;Property;_Keyword0;Keyword 0;15;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;623;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;741;3584,-2560;Inherit;False;Property;_Keyword1;Keyword 0;15;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;623;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;758;3584,-2304;Inherit;False;Property;_Keyword2;Keyword 0;15;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;623;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;766;3584,-1792;Inherit;False;Property;_Keyword4;Keyword 0;15;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;623;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;627;3968,-3072;Inherit;False;Output Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;742;3968,-2816;Inherit;False;Output Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;745;3968,-2560;Inherit;False;Output Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;761;3968,-2304;Inherit;False;Output Metallic;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;769;3968,-1792;Inherit;False;Output Emissive;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;763;3200,-2048;Inherit;False;348;Base AO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;764;3200,-1952;Inherit;False;753;Top Layer AO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;762;3584,-2048;Inherit;False;Property;_Keyword3;Keyword 0;15;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Reference;623;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;765;3968,-2048;Inherit;False;Output AO;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;744;3200,-2464;Inherit;False;737;Top Layer Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;743;3200,-2560;Inherit;False;361;Base Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;588;5376,-3072;Inherit;False;627;Output Albedo;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;589;5376,-2976;Inherit;False;742;Output Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;590;5376,-2880;Inherit;False;769;Output Emissive;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;591;5376,-2784;Inherit;False;761;Output Metallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;592;5376,-2688;Inherit;False;745;Output Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;593;5376,-2592;Inherit;False;765;Output AO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;612;-512,1698;Half;False;Global;ASPT_TopLayerOffset;ASPT_TopLayerOffset;27;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;616;-128,1602;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;615;-128,1410;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;696;128,1602;Inherit;False;Top Layer Offset;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;702;128,1410;Inherit;False;Top Layer Intensity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;614;-512,1154;Inherit;False;355;Base Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;611;-512,1890;Half;False;Global;ASPT_TopLayerContrast;ASPT_TopLayerContrast;26;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;617;-128,1794;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;776;-256,1154;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;699;128,1794;Inherit;False;Top Layer Contrast;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;610;-512,1410;Half;False;Property;_TopLayerIntensity;Top Layer Intensity;22;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;613;-512,1506;Half;False;Global;ASPT_TopLayerIntensity;ASPT_TopLayerIntensity;27;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;609;-512,1602;Half;False;Property;_TopLayerOffset;Top Layer Offset;23;0;Create;True;0;0;0;False;0;False;0.5;0.485;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;608;-512,1794;Half;False;Property;_TopLayerContrast;Top Layer Contrast;24;0;Create;True;0;0;0;False;0;False;10;10;0;30;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1052;5376,-2496;Inherit;False;MF_ASP_Global_WindTrees;30;;22;587b51c6cec05a64bb99d28802483760;0;0;2;FLOAT3;49;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1056;128,1154;Inherit;False;MF_ASP_Global_TopLayer;-1;;23;6dc1725aa9649cc439f99987e8365ea2;0;4;22;FLOAT;0;False;24;FLOAT;1;False;23;FLOAT;0.5;False;25;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;628;-512,2432;Inherit;False;355;Base Normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;683;-256,2944;Inherit;False;Constant;_Vector0;Vector 0;25;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;629;-256,2432;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;682;0,2944;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;630;-256,2624;Inherit;False;655;Global Arrow Direction;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;663;256,2944;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;689;256,3072;Inherit;False;Constant;_Float2;Float 2;25;0;Create;True;0;0;0;False;0;False;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;668;256,3200;Inherit;False;2;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;633;0,2432;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;666;512,2944;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;691;512,3200;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;635;576,2560;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;636;256,2432;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;690;768,2944;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;637;768,2432;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;692;1024,2944;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;671;1152,2432;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;632;256,2560;Half;False;Property;_TopArrowDirectionOffset;Top Arrow Direction Offset;25;0;Create;True;0;0;0;False;0;False;0;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;631;256,2656;Half;False;Global;ASPT_TopLayerArrowDirection;ASPT_TopLayerArrowDirection;19;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;693;1408,2432;Inherit;False;Arrow Direction Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;705;-512,3584;Inherit;False;2;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;706;-256,3584;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;714;0,3968;Inherit;False;355;Base Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;707;0,3584;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;708;0,3712;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;715;256,3968;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;717;256,4128;Inherit;False;Constant;_Vector1;Vector 0;25;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;709;256,3584;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;716;512,3968;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;711;512,3584;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;718;768,3968;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;713;768,3584;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;719;1024,3584;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;710;256,3712;Inherit;False;Property;_TopLayerBottomOffset;Top Layer Bottom Offset;26;0;Create;True;0;0;0;False;0;False;1;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;778;256,3808;Half;False;Global;ASPT_TopLayerBottomOffset;ASPT_TopLayerBottomOffset;19;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;721;1280,3584;Inherit;False;Top Layer Bottom Offset;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;694;-512,4608;Inherit;False;693;Arrow Direction Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;725;-512,4736;Inherit;False;721;Top Layer Bottom Offset;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;697;-256,4736;Inherit;False;696;Top Layer Offset;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;724;-256,4608;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1046;-128,4608;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;695;128,4608;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;703;384,4736;Inherit;False;702;Top Layer Intensity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;701;640,4608;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;782;896,4608;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;700;128,4736;Inherit;False;699;Top Layer Contrast;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;698;384,4608;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1057;1152,4608;Inherit;False;Top Layer Direction;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;657;896,1152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1054;1152,1152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;704;1408,1152;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;621;1664,1152;Inherit;False;Top Layer Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1058;512,1280;Inherit;False;1057;Top Layer Direction;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1064;512,1408;Inherit;False;MF_ASP_Global_TopLayerHeight;-1;;29;0851e7dd80a2406479a4b23dfe36fe1f;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;345;-2176,-2944;Inherit;False;Overlay;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;350;-1920,-2944;Inherit;False;Base Albedo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DesaturateOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1070;-2688,-2944;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1072;-2944,-2816;Inherit;False;Property;_BaseAlbedoDesaturation;Base Albedo Desaturation;2;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1069;-2944,-2688;Inherit;False;Property;_BaseAlbedoBrightness;Base Albedo Brightness;1;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1068;-2432,-2944;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;557;-3968,-2944;Inherit;True;Property;_BaseAlbedo;Base Albedo;12;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;None;c0b2daca0354f734d91cbf280e0a6523;False;gray;Auto;Texture2D;False;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;481;-2432,-2816;Half;False;Property;_BaseAlbedoColor;Base Albedo Color;0;1;[HDR];Create;True;0;0;0;False;1;Header(Base);False;0.5019608,0.5019608,0.5019608,0.5019608;0.5019608,0.5019608,0.5019608,0.5019608;False;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;372;-3968,256;Inherit;False;371;Emissive Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;373;-3584,256;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;369;-3968,0;Half;False;Property;_BaseEmissiveColor;Base Emissive Color;9;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,1;False;False;0;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;370;-3456,0;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;375;-3072,0;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;377;-2816,0;Inherit;False;Base Emissive;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;374;-3968,384;Inherit;False;Property;_BaseEmissiveMaskContrast;Base Emissive Mask Contrast;11;0;Create;True;0;0;0;False;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;376;-3328,256;Inherit;False;Property;_BaseEmissiveIntensity;Base Emissive Intensity;10;0;Create;True;0;0;0;False;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;558;-3968,768;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;559;-3584,768;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;783;-3968,960;Inherit;False;Property;_BaseTreeAOIntensity;Base Tree AO Intensity;8;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;654;-3968,1280;Half;False;Global;ASPW_WindDirection;ASPW_WindDirection;20;0;Create;True;0;0;0;False;0;False;0,0,0;0.9490499,-0.3151265,-1.881272E-07;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;562;-3328,768;Inherit;False;VColor Alpha Tree AO;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;561;-3584,896;Inherit;False;VColor Blue Tree Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;655;-3712,1280;Inherit;False;Global Arrow Direction;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1082;-2432,-576;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;950;-2176,-576;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1079;-1920,-1280;Inherit;False;Texture Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;361;-1920,-576;Inherit;False;Base Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1081;-2816,-384;Inherit;False;1079;Texture Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;734;768,-1536;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;949;1024,-1536;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;735;512,-1408;Inherit;False;361;Base Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;736;512,-1280;Inherit;False;621;Top Layer Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1086;384,-1536;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;367;-2816,-992;Half;False;Property;_BaseBarkAOIntensity;Base Bark AO Intensity;7;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;363;-2816,-1088;Half;False;Property;_BaseMetallicIntensity;Base Metallic Intensity;3;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1080;-2816,-576;Half;False;Property;_BaseSmoothnessMin;Base Smoothness Min;4;0;Create;True;0;0;0;False;0;False;0;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1083;-2816,-480;Half;False;Property;_BaseSmoothnessMax;Base Smoothness Max;5;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1084;0,-1312;Half;False;Property;_TopLayerSmoothnessMin;Top Layer Smoothness Min;18;0;Create;True;0;0;0;False;0;False;0;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1085;0,-1216;Half;False;Property;_TopLayerSmoothnessMax;Top Layer Smoothness Max;19;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;623;3584,-3072;Inherit;False;Property;_EnableTopLayerBlend;Enable Top Layer Blend;15;0;Create;True;0;0;0;False;1;Header(Top Layer);False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1073;6016,-3072;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;1;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ExtraPrePass;0;0;ExtraPrePass;6;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1074;6016,-3072;Float;False;True;-1;3;;0;4;ANGRYMESH/Stylized Pack/Tree Bark;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ForwardBase;0;1;ForwardBase;17;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;4;False;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;44;Category;0;0;  Instanced Terrain Normals;1;0;Workflow;1;0;Surface;0;0;  Blend;0;0;  Dither Shadows;1;0;Two Sided;1;0;Alpha Clipping;0;0;  Use Shadow Threshold;0;0;Deferred Pass;1;0;Normal Space,InvertActionOnDeselection;0;0;Transmission;0;0;  Transmission Shadow;0.5,False,;0;Translucency;0;0;  Translucency Strength;1,True,_BaseSSSIntensity;0;  Normal Distortion;0.5,True,_BaseSSSNormalDistortion;0;  Scattering;2,True,_BaseSSSScattering;0;  Direct;0.9,True,_BaseSSSDirect;0;  Ambient;0.1,True,_BaseSSSAmbiet;0;  Shadow;0.5,True,_BaseSSSShadow;0;Cast Shadows;1;0;Receive Shadows;1;0;Receive Specular;2;0;Receive Reflections;2;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;Ambient Light;1;0;Meta Pass;1;0;Add Pass;1;0;Override Baked GI;0;0;Write Depth;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Disable Batching;0;0;Vertex Position,InvertActionOnDeselection;1;0;0;6;False;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1075;6016,-3072;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;1;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ForwardAdd;0;2;ForwardAdd;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;True;4;1;False;;1;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;True;1;LightMode=ForwardAdd;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1076;6016,-3072;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;1;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;Deferred;0;3;Deferred;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Deferred;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1077;6016,-3072;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;1;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;Meta;0;4;Meta;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1078;6016,-3072;Float;False;False;-1;3;AmplifyShaderEditor.MaterialInspector;0;1;New Amplify Shader;ed95fe726fd7b4644bb42f4d1ddd2bcd;True;ShadowCaster;0;5;ShadowCaster;0;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;False;True;3;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;DisableBatching=False=DisableBatching;True;3;False;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
WireConnection;564;0;557;0
WireConnection;564;1;565;0
WireConnection;556;0;557;0
WireConnection;556;1;563;0
WireConnection;566;0;556;5
WireConnection;566;1;564;5
WireConnection;566;2;567;0
WireConnection;569;0;568;0
WireConnection;569;1;571;0
WireConnection;569;5;354;0
WireConnection;570;0;568;0
WireConnection;570;1;572;0
WireConnection;570;5;354;0
WireConnection;573;0;569;0
WireConnection;573;1;570;0
WireConnection;573;2;574;0
WireConnection;355;0;573;0
WireConnection;577;0;575;0
WireConnection;577;1;580;0
WireConnection;579;0;577;5
WireConnection;579;1;576;5
WireConnection;579;2;578;0
WireConnection;582;0;579;0
WireConnection;365;1;582;2
WireConnection;365;2;367;0
WireConnection;583;0;365;0
WireConnection;583;1;584;0
WireConnection;587;0;577;4
WireConnection;587;1;576;4
WireConnection;587;2;578;0
WireConnection;371;0;587;0
WireConnection;362;0;582;1
WireConnection;362;1;363;0
WireConnection;364;0;362;0
WireConnection;576;0;575;0
WireConnection;576;1;581;0
WireConnection;348;0;583;0
WireConnection;599;0;600;5
WireConnection;599;1;601;5
WireConnection;604;0;599;0
WireConnection;604;1;603;0
WireConnection;605;0;606;0
WireConnection;605;1;604;0
WireConnection;605;2;607;0
WireConnection;625;0;605;0
WireConnection;600;1;602;0
WireConnection;647;0;646;0
WireConnection;647;1;645;0
WireConnection;649;0;647;0
WireConnection;649;1;646;0
WireConnection;649;2;648;0
WireConnection;652;0;651;0
WireConnection;652;1;649;0
WireConnection;652;2;650;0
WireConnection;653;0;652;0
WireConnection;646;1;643;0
WireConnection;646;5;644;0
WireConnection;737;0;949;0
WireConnection;730;1;731;0
WireConnection;747;0;746;0
WireConnection;747;2;748;0
WireConnection;749;0;747;0
WireConnection;751;0;750;0
WireConnection;751;2;752;0
WireConnection;753;0;751;0
WireConnection;755;0;754;0
WireConnection;755;2;756;0
WireConnection;757;0;755;0
WireConnection;596;0;594;0
WireConnection;596;1;595;0
WireConnection;597;0;596;0
WireConnection;738;1;739;0
WireConnection;738;0;740;0
WireConnection;741;1;743;0
WireConnection;741;0;744;0
WireConnection;758;1;759;0
WireConnection;758;0;760;0
WireConnection;766;1;767;0
WireConnection;766;0;768;0
WireConnection;627;0;623;0
WireConnection;742;0;738;0
WireConnection;745;0;741;0
WireConnection;761;0;758;0
WireConnection;769;0;766;0
WireConnection;762;1;763;0
WireConnection;762;0;764;0
WireConnection;765;0;762;0
WireConnection;616;0;609;0
WireConnection;616;1;612;0
WireConnection;615;0;610;0
WireConnection;615;1;613;0
WireConnection;696;0;616;0
WireConnection;702;0;615;0
WireConnection;617;0;608;0
WireConnection;617;1;611;0
WireConnection;776;0;614;0
WireConnection;699;0;617;0
WireConnection;1056;22;776;2
WireConnection;1056;24;615;0
WireConnection;1056;23;616;0
WireConnection;1056;25;617;0
WireConnection;629;0;628;0
WireConnection;682;0;629;0
WireConnection;682;1;683;0
WireConnection;663;0;682;0
WireConnection;633;0;629;0
WireConnection;633;1;630;0
WireConnection;666;0;689;0
WireConnection;666;1;663;0
WireConnection;691;0;668;2
WireConnection;635;0;632;0
WireConnection;635;1;631;0
WireConnection;636;0;633;0
WireConnection;690;0;666;0
WireConnection;690;1;691;0
WireConnection;637;0;636;0
WireConnection;637;1;635;0
WireConnection;692;0;690;0
WireConnection;671;0;637;0
WireConnection;671;1;692;0
WireConnection;693;0;671;0
WireConnection;706;0;705;2
WireConnection;707;0;706;0
WireConnection;707;1;706;0
WireConnection;708;0;706;0
WireConnection;708;1;706;0
WireConnection;715;0;714;0
WireConnection;709;0;707;0
WireConnection;709;1;708;0
WireConnection;716;0;715;0
WireConnection;716;1;717;0
WireConnection;711;0;709;0
WireConnection;711;1;710;0
WireConnection;711;2;778;0
WireConnection;718;0;716;0
WireConnection;713;0;711;0
WireConnection;713;1;711;0
WireConnection;719;0;713;0
WireConnection;719;1;718;0
WireConnection;721;0;719;0
WireConnection;724;0;694;0
WireConnection;724;1;725;0
WireConnection;1046;0;724;0
WireConnection;695;0;1046;0
WireConnection;695;1;697;0
WireConnection;701;0;698;0
WireConnection;701;1;703;0
WireConnection;782;0;701;0
WireConnection;698;0;695;0
WireConnection;698;1;700;0
WireConnection;1057;0;782;0
WireConnection;657;0;1056;0
WireConnection;657;1;1058;0
WireConnection;1054;0;657;0
WireConnection;1054;1;1064;0
WireConnection;704;0;1054;0
WireConnection;621;0;704;0
WireConnection;345;0;1068;0
WireConnection;345;1;481;5
WireConnection;350;0;345;0
WireConnection;1070;0;566;0
WireConnection;1070;1;1072;0
WireConnection;1068;0;1070;0
WireConnection;1068;1;1069;0
WireConnection;373;0;372;0
WireConnection;373;1;374;0
WireConnection;370;0;369;0
WireConnection;370;1;373;0
WireConnection;375;0;370;0
WireConnection;375;1;376;0
WireConnection;377;0;375;0
WireConnection;559;1;558;4
WireConnection;559;2;783;0
WireConnection;562;0;559;0
WireConnection;561;0;558;3
WireConnection;655;0;654;0
WireConnection;1082;0;1080;0
WireConnection;1082;1;1083;0
WireConnection;1082;2;1081;0
WireConnection;950;0;1082;0
WireConnection;1079;0;582;0
WireConnection;361;0;950;0
WireConnection;734;0;735;0
WireConnection;734;1;1086;0
WireConnection;734;2;736;0
WireConnection;949;0;734;0
WireConnection;1086;0;1084;0
WireConnection;1086;1;1085;0
WireConnection;1086;2;730;1
WireConnection;623;1;624;0
WireConnection;623;0;626;0
WireConnection;1074;0;588;0
WireConnection;1074;1;589;0
WireConnection;1074;4;591;0
WireConnection;1074;5;592;0
WireConnection;1074;6;593;0
WireConnection;1074;2;590;0
WireConnection;1074;15;1052;49
ASEEND*/
//CHKSM=7F043714F77AFCB533FEB72438A86DBCF938E942