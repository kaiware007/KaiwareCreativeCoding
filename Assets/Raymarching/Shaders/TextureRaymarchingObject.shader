Shader "Raymarching/TextureRaymarchingObject"
{

Properties
{
    _MainTex ("Main Texture", 2D) = "black" {}
	_Intensity("Intensity", Range(0,1)) = 0.125
}

SubShader
{

//Tags { "RenderType" = "Opaque" "DisableBatching" = "True" "Queue" = "Geometry+10" }
//Cull Off
ZWrite Off
//Blend SrcAlpha One
Blend OneMinusDstColor One // Soft Additive

Pass
{
    //Tags { "LightMode" = "Deferred" }
	Tags{ "LightMode" = "ForwardBase" }

	// Deferred Rendering のベースとライティングのパスでは Stencil バッファはライティング用途に使われます。
    //Stencil 
    //{
    //    Comp Always
    //    Pass Replace
    //    Ref 128
    //}

    CGPROGRAM
    #pragma vertex vert_object
    #pragma fragment frag
    #pragma target 5.0
    #pragma multi_compile ___ UNITY_HDR_ON

    #include "UnityCG.cginc"
	#include "UnityLightingCommon.cginc" // for _LightColor0
    #include "Assets/Shaders/Libs/Utils.cginc"
    #include "Assets/Shaders/Libs/Primitives.cginc"
	//#include "Assets/Shaders/Libs/ClassicNoise3D.cginc"
	//#include "Assets/Shaders/Libs/SimplexNoiseGrad3D.cginc"
	#include "Assets/Shaders/Libs/SimplexNoise3D.cginc"

	// 距離関数
    float DistanceFunc(float3 pos)
    {
		return 0;	// test
    }

	// 上でDistanceFuncを定義しているのでここでインクルードしないとだめ
    #include "Assets/Shaders/Libs/Raymarching.cginc"

    sampler2D _MainTex;

	float _Intensity;

	//float map5(float3 p, float t)
	//{
	//	p.y = 0;
	//	float3 q = p - float3(0.0, 0.1, 1.0) * t;
	//	//q *= _NoiseScale;
	//	float f;
	//	f = 0.50000*snoise(q); q = q*2.02;
	//	f += 0.25000*snoise(q); q = q*2.03;
	//	f += 0.12500*snoise(q); q = q*2.01;
	//	f += 0.06250*snoise(q); q = q*2.02;
	//	f += 0.03125*snoise(q);
	//	//return clamp(1.5 - p.y - 2.0 + 1.75*f, 0.0, 1.0);
	//	return clamp(1*f, 0.0, 1.0);
	//}

	inline bool isInnerBox(float3 pos, float3 scale)
	{
		return all(max(scale * 0.5 - abs(pos), 0.0));
	}

	float4 getTexColor(float3 pos, float3 localPos) {

		float2 uv = pos.xz / _Scale.xz + float2(0.5 + _Time.x,0.5);
		//float2 uv = pos.xz;
		//uv = saturate(uv);
		//return tex2D(_MainTex, uv);
		//return map5((pos + _Time.y * 100)/ _Scale, 1) * smoothstep(0, 1, (1.0 - abs(localPos.y)));
		return tex2D(_MainTex, uv) * smoothstep(0,1,(1.0 - abs(localPos.y)));
		//return tex2D(_MainTex, uv) * (pos.y / _Scale.y + 0.5) * 0.1;
	}

	float4 raymarch(float3 position, float3 localPosition, float3 ro, float3 rd)
	{
		float4 sum = float4(0, 0, 0, 0);
		float t = 0;

		int steps = 30;
		
		int count = 0;
		for (int i = 0; i < steps; i++) {
			float3 pos = position + ro + t*rd * _Scale;
			float3 localPos = localPosition + ro + t*rd;
			//if (sum.a > 0.5) break;
			count = i;
			if (!isInnerBox(localPos, _Scale)) break;

			float4 col = getTexColor(pos, localPos);
			float len = length(col.rgb);
			//if (len > 0.01) 
			{
				//sum += pow(float4(col.rgb, len) * 2, 3) * 0.2;
				sum += float4(col.rgb, len) * _Intensity;
			}
			//t += max(0.05, 0.02 * t) / _Scale.xz;
			t += max(0.0125, 0.0125 * t);
			//t += 2;
			//t += 0.025;
		}
		//sum /= count;

		//return saturate(sum);
		return clamp(sum, 0.0, 0.8);
	}

	float4 render(float3 position, float3 localPosition, float3 ro, float3 rd) {
		float4 res = raymarch(position, localPosition, ro, rd);

		return res;
	}

	fixed4 frag(VertObjectOutput i) : SV_Target
    {
        float3 rayDir = GetRayDirection(i.screenPos);	// レイの方向

		float3 pos = i.worldPos;
		float3 localPos = ToLocal(i.worldPos);

		float3 ro = GetCameraForward();

		return render(pos, localPos, ro, rayDir);
    }

    ENDCG
}

}

Fallback Off
}