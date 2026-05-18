// Made with Amplify Shader Editor v1.9.5.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Particles/Trail"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_StartColor("Start Color", Color) = (0.4188099,0,1,1)
		_EndColor("End Color", Color) = (0,0.4116445,1,1)
		_Emission("Emission", Float) = 2
		_Colorpower("Color power", Float) = 1
		_Colorrange("Color range", Float) = 1
		_MainTexture("MainTexture", 2D) = "white" {}
		_SpeedMainTexture("Speed Main Texture", Vector) = (0,0,0,0)
		_Noise("Noise", 2D) = "white" {}
		_SpeedNoiseTexture("Speed Noise Texture", Vector) = (0,0,0,0)
		[Toggle]_Alpha("Alpha", Float) = 1
		_Mask("Mask", 2D) = "white" {}

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha One
			ColorMask RGB
			Cull Back
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform float4 _StartColor;
				uniform float4 _EndColor;
				uniform float _Colorrange;
				uniform float _Colorpower;
				uniform float _Emission;
				uniform float _Alpha;
				uniform sampler2D _MainTexture;
				uniform float2 _SpeedMainTexture;
				uniform float4 _MainTexture_ST;
				uniform sampler2D _Noise;
				uniform float4 _Noise_ST;
				uniform float2 _SpeedNoiseTexture;
				uniform sampler2D _Mask;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 texCoord1 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float UV6 = texCoord1.x;
					float4 lerpResult3 = lerp( _StartColor , _EndColor , saturate( pow( ( UV6 * _Colorrange ) , _Colorpower ) ));
					float4 temp_cast_0 = (1.0).xxxx;
					float3 uvs3_MainTexture = i.texcoord.xyz;
					uvs3_MainTexture.xy = i.texcoord.xyz.xy * _MainTexture_ST.xy + _MainTexture_ST.zw;
					float4 Main57 = tex2D( _MainTexture, ( float3( ( _SpeedMainTexture * _Time.y ) ,  0.0 ) + uvs3_MainTexture + uvs3_MainTexture.z ).xy );
					float2 uv_Noise = i.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw;
					float clampResult44 = clamp( ( pow( ( 1.0 - UV6 ) , 0.8 ) * 1.0 ) , 0.2 , 0.6 );
					float4 Noise49 = saturate( ( tex2D( _Noise, ( uv_Noise + ( _Time.y * _SpeedNoiseTexture ) ) ) + clampResult44 ) );
					float4 temp_output_51_0 = ( i.color.a * Main57 * Noise49 * tex2D( _Mask, uv_Noise ) );
					float4 appendResult92 = (float4((( ( lerpResult3 * i.color * _Emission ) * (( _Alpha )?( temp_output_51_0 ):( temp_cast_0 )) )).rgb , temp_output_51_0.r));
					

					fixed4 col = appendResult92;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	
	
	Fallback Off
}
/*ASEBEGIN
Version=19501
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-3546.753,397.129;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;6;-3303.518,406.7481;Float;False;UV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;42;-3082.569,407.1339;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;30;-3333.521,50.82023;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;111;-3751.579,207.6256;Inherit;False;Property;_SpeedNoiseTexture;Speed Noise Texture;8;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-3096.073,262.2383;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PowerNode;60;-2904.368,406.8205;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;40;-3096.845,146.033;Inherit;False;0;27;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;107;-3739.1,-164.8446;Inherit;False;Property;_SpeedMainTexture;Speed Main Texture;6;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-2730.11,397.7697;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;36;-2845.31,226.1788;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;56;-2880.68,-30.42417;Inherit;False;0;2;3;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;44;-2577.53,387.1384;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.2;False;2;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-2886.624,-149.0239;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;27;-2717.816,199.2953;Inherit;True;Property;_Noise;Noise;7;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleAddOpNode;37;-2602.89,-135.1975;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;45;-2367.688,205.1529;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-1510.567,-163.0431;Float;False;Property;_Colorrange;Color range;4;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;7;-1522.872,-236.9327;Inherit;False;6;UV;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-2462.57,-141.4448;Inherit;True;Property;_MainTexture;MainTexture;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode;97;-2054.302,227.2418;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-1275.883,-237.93;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-1302.603,-154.2804;Float;False;Property;_Colorpower;Color power;3;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;57;-2138.951,-140.3696;Float;False;Main;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;-1895.353,223.9512;Float;False;Noise;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;118;-1280,496;Inherit;False;0;27;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;10;-1114.394,-237.9301;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-790.1919,304.5912;Inherit;False;49;Noise;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;15;-707.296,-286.4251;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;58;-790.595,206.805;Inherit;False;57;Main;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;117;-1040,480;Inherit;True;Property;_Mask;Mask;10;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;5;-1095.667,-412.8965;Float;False;Property;_EndColor;End Color;1;0;Create;True;0;0;0;False;0;False;0,0.4116445,1,1;0,0,0,0;False;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;4;-1099.159,-592.153;Float;False;Property;_StartColor;Start Color;0;0;Create;True;0;0;0;False;0;False;0.4188099,0,1,1;0,0,0,0;False;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode;98;-940.9728,-239.1694;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-501.945,204.2904;Inherit;True;4;4;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-701.7391,-25.00539;Float;False;Property;_Emission;Emission;2;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-404.9611,48.36441;Float;False;Constant;_Float1;Float 1;9;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;3;-765.0906,-495.5406;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-436.6116,-130.9249;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;99;-247.7693,54.92614;Float;False;Property;_Alpha;Alpha;9;0;Create;True;0;0;0;False;0;False;1;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-22.73198,-30.14677;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;93;225.5296,5.051892;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;92;469.0749,138.9254;Inherit;True;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;94;560,480;Float;False;True;-1;2;;0;11;Particles/Trail;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;True;8;5;False;;1;False;;0;5;False;;1;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;False;0;False;;False;False;False;False;False;False;False;False;False;True;2;False;;True;3;False;;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;6;0;1;1
WireConnection;42;0;6;0
WireConnection;35;0;30;2
WireConnection;35;1;111;0
WireConnection;60;0;42;0
WireConnection;59;0;60;0
WireConnection;36;0;40;0
WireConnection;36;1;35;0
WireConnection;44;0;59;0
WireConnection;33;0;107;0
WireConnection;33;1;30;2
WireConnection;27;1;36;0
WireConnection;37;0;33;0
WireConnection;37;1;56;0
WireConnection;37;2;56;3
WireConnection;45;0;27;0
WireConnection;45;1;44;0
WireConnection;2;1;37;0
WireConnection;97;0;45;0
WireConnection;9;0;7;0
WireConnection;9;1;8;0
WireConnection;57;0;2;0
WireConnection;49;0;97;0
WireConnection;10;0;9;0
WireConnection;10;1;12;0
WireConnection;117;1;118;0
WireConnection;98;0;10;0
WireConnection;51;0;15;4
WireConnection;51;1;58;0
WireConnection;51;2;50;0
WireConnection;51;3;117;0
WireConnection;3;0;4;0
WireConnection;3;1;5;0
WireConnection;3;2;98;0
WireConnection;14;0;3;0
WireConnection;14;1;15;0
WireConnection;14;2;16;0
WireConnection;99;0;53;0
WireConnection;99;1;51;0
WireConnection;54;0;14;0
WireConnection;54;1;99;0
WireConnection;93;0;54;0
WireConnection;92;0;93;0
WireConnection;92;3;51;0
WireConnection;94;0;92;0
ASEEND*/
//CHKSM=3C21E951EE87FE1DC434CF9D332B9DCC1EB4BF02