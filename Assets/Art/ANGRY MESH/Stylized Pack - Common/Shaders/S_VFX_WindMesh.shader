// Made with Amplify Shader Editor v1.9.9.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ANGRYMESH/Stylized Pack/VFX/VFX WindMesh"
{
	Properties
	{
		_OpacityIntensity( "Opacity Intensity", Range( 0, 1 ) ) = 1
		[HDR] _EmissiveColor( "Emissive Color", Color ) = ( 0.7490196, 0.7490196, 0.7490196 )
		_EmissiveIntensity( "Emissive Intensity", Range( 0, 10 ) ) = 1
		_AnimationFadeSpeed( "Animation Fade Speed", Range( 0, 10 ) ) = 1
		_AnimationFadeOffset( "Animation Fade Offset", Range( 0.01, 10 ) ) = 1
		_CameraDepthFadeDistance( "Camera Depth Fade Distance", Range( 0, 10 ) ) = 1
		_CameraDepthFadeOffset( "Camera Depth Fade Offset", Range( 0, 10 ) ) = 1
		_MaxRenderDistance( "Max Render Distance", Range( 0, 1000 ) ) = 100

	}

	SubShader
	{
		

		Tags { "RenderType"="Opaque" }

	LOD 100

		

		Blend One One
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		Offset 0 , 0
		

		CGINCLUDE
			#pragma target 3.5

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
			Name "Unlit"

			CGPROGRAM
				#define ASE_VERSION 19903

				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
					#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
				#include "UnityCG.cginc"

				#define ASE_NEEDS_TEXTURE_COORDINATES0
				#define ASE_NEEDS_VERT_POSITION
				#define ASE_NEEDS_FRAG_COLOR


				struct appdata
				{
					float4 vertex : POSITION;
					float4 ase_color : COLOR;
					float4 ase_texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float4 ase_color : COLOR;
					float4 ase_texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				uniform float3 _EmissiveColor;
				uniform float _EmissiveIntensity;
				uniform float _AnimationFadeSpeed;
				uniform float _AnimationFadeOffset;
				uniform float _CameraDepthFadeDistance;
				uniform float _CameraDepthFadeOffset;
				uniform float _OpacityIntensity;
				uniform float _MaxRenderDistance;


				
				v2f vert ( appdata v )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID( v );
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
					UNITY_TRANSFER_INSTANCE_ID( v, o );

					float3 objectToViewPos = UnityObjectToViewPos( v.vertex.xyz );
					float eyeDepth = -objectToViewPos.z;
					o.ase_texcoord.w = eyeDepth;
					
					o.ase_color = v.ase_color;
					o.ase_texcoord.xyz = v.ase_texcoord.xyz;

					float3 vertexValue = float3( 0, 0, 0 );
					#if ASE_ABSOLUTE_VERTEX_POS
						vertexValue = v.vertex.xyz;
					#endif
					vertexValue = vertexValue;
					#if ASE_ABSOLUTE_VERTEX_POS
						v.vertex.xyz = vertexValue;
					#else
						v.vertex.xyz += vertexValue;
					#endif

					o.pos = UnityObjectToClipPos( v.vertex );
					return o;
				}

				half4 frag( v2f IN  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( IN );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
					half4 finalColor;

					float4 ScreenPosNorm = float4( IN.pos.xy * ( _ScreenParams.zw - 1.0 ), IN.pos.zw );
					float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, IN.pos.z ) * IN.pos.w;
					float4 ScreenPos = ComputeScreenPos( ClipPos );

					float4 Output_Emissive93 = ( float4( _EmissiveColor , 0.0 ) * IN.ase_color * _EmissiveIntensity );
					float3 texCoord65 = IN.ase_texcoord.xyz;
					texCoord65.xy = IN.ase_texcoord.xyz.xy * float2( 1,1 ) + float2( 0,0 );
					float2 appendResult90 = (float2(texCoord65.x , texCoord65.y));
					float2 appendResult69 = (float2(( 0.25 * texCoord65.z * _AnimationFadeSpeed ) , 0.0));
					float2 appendResult76 = (float2(_AnimationFadeOffset , 0.0));
					float eyeDepth = IN.ase_texcoord.w;
					float cameraDepthFade83 = (( eyeDepth -_ProjectionParams.y - _CameraDepthFadeOffset ) / _CameraDepthFadeDistance);
					float clampResult132 = clamp( cameraDepthFade83 , 0.0 , 1.0 );
					float Camera_Depth_Fade91 = clampResult132;
					float VColor_Alpha159 = IN.ase_color.a;
					float cameraDepthFade149 = (( eyeDepth -_ProjectionParams.y - 10.0 ) / _MaxRenderDistance);
					float clampResult148 = clamp( cameraDepthFade149 , 0.0 , 1.0 );
					float Max_Depth_Distance154 = ( 1.0 - clampResult148 );
					float Output_Opacity39 = ( ( saturate( ( ( appendResult90 - appendResult69 ) * appendResult76 ) ).x * ( 1.0 - saturate( ( ( appendResult90 - appendResult69 ) * appendResult76 ) ).x ) ) * Camera_Depth_Fade91 * VColor_Alpha159 * _OpacityIntensity * Max_Depth_Distance154 );
					

					finalColor = ( Output_Emissive93 * Output_Opacity39 );

					return finalColor;
				}
			ENDCG
		}
	}
	
	
	Fallback Off
}
/*ASEBEGIN
Version=19903
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;99;-4284,1102;Inherit;False;3132.256;561.4174;;19;89;160;92;39;84;79;78;77;72;76;75;32;70;69;90;81;65;71;156;Opacity;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;71;-4224,1408;Inherit;False;Property;_AnimationFadeSpeed;Animation Fade Speed;3;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;65;-4224,1152;Inherit;False;0;-1;3;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;81;-3840,1408;Inherit;False;3;3;0;FLOAT;0.25;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;155;-4287,2384;Inherit;False;1598;302;;6;146;154;153;150;149;148;Max Render Distance;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;90;-3840,1152;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;69;-3584,1408;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;32;-4224,1536;Inherit;False;Property;_AnimationFadeOffset;Animation Fade Offset;4;0;Create;True;0;0;0;False;0;False;1;1;0.01;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;100;-4288,1872;Inherit;False;1601;301;;5;91;83;132;87;86;Camera Depth Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;153;-4224,2560;Inherit;False;Constant;_Float1;Float 1;9;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;146;-4224,2432;Inherit;False;Property;_MaxRenderDistance;Max Render Distance;7;0;Create;True;0;0;0;False;0;False;100;1;0;1000;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;70;-3328,1152;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;76;-3328,1536;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;86;-4224,1920;Inherit;False;Property;_CameraDepthFadeDistance;Camera Depth Fade Distance;5;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;87;-4224,2048;Inherit;False;Property;_CameraDepthFadeOffset;Camera Depth Fade Offset;6;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.CameraDepthFade, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;149;-3840,2432;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;75;-3072,1152;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CameraDepthFade, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;83;-3840,1920;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;148;-3584,2432;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;98;-4288,336;Inherit;False;1217;558;;6;159;93;97;158;17;11;Emissive;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;72;-2816,1152;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;132;-3584,1920;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;150;-3328,2432;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;77;-2560,1152;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;159;-3328,640;Inherit;False;VColor Alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;91;-3072,1920;Inherit;False;Camera Depth Fade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;78;-2432,1280;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;154;-3072,2432;Inherit;False;Max Depth Distance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;158;-4224,544;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;97;-4224,736;Inherit;False;Property;_EmissiveIntensity;Emissive Intensity;2;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;11;-4224,384;Inherit;False;Property;_EmissiveColor;Emissive Color;1;1;[HDR];Create;True;0;0;0;False;0;False;0.7490196,0.7490196,0.7490196,0;0.8160377,0.9511108,1,0;True;False;0;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;79;-2176,1152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;92;-2176,1280;Inherit;False;91;Camera Depth Fade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;160;-2176,1376;Inherit;False;159;VColor Alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;89;-2176,1472;Inherit;False;Property;_OpacityIntensity;Opacity Intensity;0;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;156;-2176,1568;Inherit;False;154;Max Depth Distance;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;17;-3840,384;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;84;-1664,1152;Inherit;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;101;-560,336;Inherit;False;818;304.9999;;3;200;64;94;Output;1,0,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;93;-3328,384;Inherit;False;Output Emissive;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;39;-1408,1152;Inherit;False;Output Opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;94;-512,384;Inherit;False;93;Output Emissive;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;64;-512,512;Inherit;False;39;Output Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;200;-256,384;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;215;0,384;Float;False;True;-1;3;;100;5;ANGRYMESH/Stylized Pack/VFX/VFX WindMesh;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;4;1;False;;1;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;3;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
WireConnection;81;1;65;3
WireConnection;81;2;71;0
WireConnection;90;0;65;1
WireConnection;90;1;65;2
WireConnection;69;0;81;0
WireConnection;70;0;90;0
WireConnection;70;1;69;0
WireConnection;76;0;32;0
WireConnection;149;0;146;0
WireConnection;149;1;153;0
WireConnection;75;0;70;0
WireConnection;75;1;76;0
WireConnection;83;0;86;0
WireConnection;83;1;87;0
WireConnection;148;0;149;0
WireConnection;72;0;75;0
WireConnection;132;0;83;0
WireConnection;150;0;148;0
WireConnection;77;0;72;0
WireConnection;159;0;158;4
WireConnection;91;0;132;0
WireConnection;78;0;77;0
WireConnection;154;0;150;0
WireConnection;79;0;77;0
WireConnection;79;1;78;0
WireConnection;17;0;11;0
WireConnection;17;1;158;0
WireConnection;17;2;97;0
WireConnection;84;0;79;0
WireConnection;84;1;92;0
WireConnection;84;2;160;0
WireConnection;84;3;89;0
WireConnection;84;4;156;0
WireConnection;93;0;17;0
WireConnection;39;0;84;0
WireConnection;200;0;94;0
WireConnection;200;1;64;0
WireConnection;215;0;200;0
ASEEND*/
//CHKSM=4934DA704309A3DD7EE535DA67339170FE3DD6AA