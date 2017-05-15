Shader "Custom/CloudComposite"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_CloudTex("Cloud Texture", 2D) = "black" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _CloudTex;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 cloud = tex2D(_CloudTex, i.uv);

				// just invert the colors
				//col = 1 - col;
				//return col + cloud;
				return fixed4(col.rgb * col.a + cloud.rgb * (1 - col.a), 1);
				//return lerp(cloud, col, col.a);
				//return lerp(col, cloud, cloud.a);
			}
			ENDCG
		}
	}
}
