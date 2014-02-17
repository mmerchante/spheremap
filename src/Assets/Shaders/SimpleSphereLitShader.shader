Shader "SphereLit (Simple)" 
{
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "white" {}

	}

	SubShader 
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float3 normal : TEXCOORD0; 
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.normal = mul(UNITY_MATRIX_IT_MV, float4(v.normal, 1.0)).xyz;
				return o;
			}
			
			sampler2D _MainTex;

			fixed4 frag (v2f i) : COLOR
			{
				return tex2D (_MainTex, float2(i.normal.x, i.normal.y) * .5 + .5);
			}
			ENDCG

		}

	} 

	FallBack "Mobile/Diffuse"
}
