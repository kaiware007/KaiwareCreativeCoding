// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "FlowGrid/FlowGridDebug"
{

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "FlowGrid.cginc"

			struct v2g
			{
				float4 position : SV_POSITION;
				float3 flow : TEXCOORD0;
			};
			
			struct g2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			float3 _WallSize;
			float3 _WallCenter;
			float3 _GridCenter;

			StructuredBuffer<float3> _BufferRead;

			v2g vert (uint id : SV_VertexID)
			{
				float3 pos = GetIndexToPosition(id);

				pos = _WallCenter + _GridCenter + pos * _WallSize;

				v2g o = (v2g)0;
				o.position = UnityObjectToClipPos(float4(pos, 1.0));
				o.flow = _BufferRead[id];
				//o.color = float4(normalize(_BufferRead[id]) * 0.5 + 0.5, 1.0);
				return o;
			}
			
			[maxvertexcount(2)]
			void geom(point v2g points[1], inout LineStream<g2f> output) {
				g2f pos[2];

				float4 vertex = points[0].position;
				float4 color = float4(normalize(points[0].flow) * 0.5 + 0.5, 1.0);
				pos[0].vertex = vertex;
				pos[0].color = color;
				pos[1].vertex = vertex + float4(points[0].flow, 0);
				pos[1].color = color;

				output.Append(pos[0]);
				output.Append(pos[1]);

				output.RestartStrip();
			}

			fixed4 frag (g2f i) : SV_Target
			{
				fixed4 col = i.color;
				return col;
			}
			ENDCG
		}
	}
}
