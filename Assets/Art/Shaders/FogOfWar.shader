﻿Shader "Projector/Fog Of War" {
	Properties {
		_FogTex ("Fog Texture", 2D) = "gray" {}
		_Color ("Color", Color) = (0,0,0,0)
	}
	Subshader {
		Tags {"Queue"="Transparent"}
		Pass {
			ZWrite Off
			Blend DstColor Zero
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			
			struct v2f {
				float4 uvShadow : TEXCOORD0;
				UNITY_FOG_COORDS(2)
				float4 pos : SV_POSITION;
			};
			
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (vertex);
				o.uvShadow = mul (unity_Projector, vertex);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			sampler2D _FogTex;
			fixed4 _Color;
			uniform float _Blend;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed g = tex2Dproj (_FogTex, UNITY_PROJ_COORD(i.uvShadow)).g;

				fixed4 col = lerp(_Color, fixed4(1,1,1,1), g);

				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(1,1,1,1));
				return col;
			}
			ENDCG
		}
	}
}
