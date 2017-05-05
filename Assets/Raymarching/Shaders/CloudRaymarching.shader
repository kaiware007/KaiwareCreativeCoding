Shader "Raymarching/Cloud"
{

Properties
{
    _MainTex ("Main Texture", 2D) = "" {}
	_GradientTex("Gradient Texture", 2D) = "" {}
}

SubShader
{

//Tags { "RenderType" = "Opaque" "DisableBatching" = "True" "Queue" = "Geometry+10" }
Cull Off
ZWrite Off
Blend SrcAlpha One

Pass
{
    //Tags { "LightMode" = "Deferred" }

	// Deferred Rendering のベースとライティングのパスでは Stencil バッファはライティング用途に使われます。
    //Stencil 
    //{
    //    Comp Always
    //    Pass Replace
    //    Ref 128
    //}

    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 5.0
    #pragma multi_compile ___ UNITY_HDR_ON

    #include "UnityCG.cginc"
    #include "Assets/Shaders/Libs/Utils.cginc"
    #include "Assets/Shaders/Libs/Primitives.cginc"
	//#include "Assets/Shaders/Libs/ClassicNoise3D.cginc"
	//#include "Assets/Shaders/Libs/SimplexNoiseGrad3D.cginc"
	#include "Assets/Shaders/Libs/SimplexNoise3D.cginc"
	
	struct ShaderData
	{
		float noiseScale;
		float clipThreashold;
		float noiseRange;
	};

	//float hex(float2 p, float2 h)
	//{
	//	float2 q = abs(p);
	//	return max(q.x - h.y, max(q.x + q.y*0.57735, q.y*1.1547) - h.x);
	//}

	float mod2(float x, float y)
	{
		return x - y * floor(x / y);
	}

	float2 mod2(float2 x, float2 y)
	{
		return x - y * floor(x / y);
	}

	// 距離関数
    float DistanceFunc(float3 pos)
    {

		//float tenjou = floor(pos - float3(0, 5, 0));
		//float yuka = floor(abs(pos - float3(0, -2.5, 0)));
		//return smoothMin(h, kwr, 3);
		//return (hit)?smoothMin(yuka, kwr, 3) : yuka;
		//return min(yuka, kwr);
		//return h;

		//float r = abs(sin(2 * PI * _Time.y / 2.0));
  //      float d1 = roundBox(repeat(pos, float3(6, 6, 6)), 1, r);
  //      float d2 = sphere(pos, 3.0);


  //      float d3 = floor(pos - float3(0, -3, 0));
  //      return smoothMin(smoothMin(d1, d2, 1.0), d3, 1.0);

		return 0;	// test
    }

	// 上でDistanceFuncを定義しているのでここでインクルードしないとだめ
    #include "Assets/Shaders/Libs/Raymarching.cginc"

    sampler2D _MainTex;
	sampler2D _GradientTex;
	StructuredBuffer<ShaderData> _ShaderData;

	static const float distanceArray[3] = { 2.5, 50, 100 };
	static const float intensityArray[3] = { 0.2, 0.3, 0.5 };
	//static const float intensityArray[3] = { 1.25, 1, 0.75};

	fixed GetNoise(float3 pos, float scale, float threashold, float invThreashold) {
		fixed c1 = snoise(pos / scale) * 0.5 + 0.5;
		fixed c2 = (c1 - threashold) * invThreashold;
		return step(threashold, c1) * c2;
	}

    //GBufferOut frag(VertOutput i)
	fixed4 frag(VertOutput i) : SV_Target
    {
		//float distanceArray[3] = { 2.5, 50, 100 };

        float3 rayDir = GetRayDirection(i.screenPos);	// レイの方向

        float3 camPos = GetCameraPosition() + float3(0, 0, 0);	// カメラの位置
        float maxDist = GetCameraMaxDistance();	// 最大移動距離

		float div = 10;
        //float distance = _ShaderData[0].noiseScale / div;
		float distance = _ShaderData[0].noiseRange;
		float len = 0.0;
        float3 pos = camPos + _ProjectionParams.y * rayDir;	// Near Planeから計算開始
		int marchCount = 0;
		int maxMarch = 512;

		//pos += rayDir * distance;

		float3 posArray[3] = {pos, pos, pos};
		float d = 1.0 / div;
		float c1, c2;
		float ct = _ShaderData[0].clipThreashold;
		float bt = 1.0 / ct;

		//float3 col = snoise_grad(pos);
		//return fixed4(col,1);
		float r = 0;
		//float g = 0;
		//float b = 0;
		//float bg = GetNoise(pos, 200, ct, bt);
		float bg = (snoise(rayDir * _ProjectionParams.y * 2) * 0.5 + 0.5);

		for (int i = 0; i < div; i++) {
			c1 = 0;
			for (int j = 0; j < 3; j++) {
				//r += GetNoise(posArray[j], distanceArray[j], ct, bt) * d / 3;
				//c1 += GetNoise(pos, distanceArray[j], ct, bt) * d * intensityArray[j];
				c1 += GetNoise(pos, distanceArray[j], ct, bt) * intensityArray[j];
				/*posArray[j] += rayDir * distanceArray[j];*/
				//posArray[j] += rayDir * distance;
			}
			r += c1 * d;

			//c1 = snoise(pos / _ShaderData[0].noiseScale + float3(0, 0, 0)) * 0.5 + 0.5;
			//c2 = (c1 - ct) * bt;
			//r += step(ct, c1) * c2 * d;
			//r += step(_ShaderData[0].clipThreashold, c) * c * ((div - i) / div) * 0.01;
			//r += clamp(0, 1, snoise(pos + float3(0, 0, 0)) * 0.5 + 0.5);
			//g += clamp(0, 1, snoise(pos + float3(1, 100, 0)) * 0.5 + 0.5);
			//b += clamp(0, 1, snoise(pos + float3(0, 0, 100)) * 0.5 + 0.5);
			pos += rayDir * distance;
		}
		//return fixed4(r / div, g / div, b / div, 1);
		//r += bg;
		//r = bg;
		//r = clamp(r, 0, 1);
		return tex2D(_GradientTex, fixed2(clamp(bg, 0, 1), 0.5));
		//return tex2D(_GradientTex, fixed2(clamp(bg, 0, 1), 0.5)) * 0.5 + tex2D(_GradientTex, fixed2(clamp(r, 0, 1), 0.5)) * 0.5;
		//return tex2D(_GradientTex, fixed2(clamp(r, 0, 1), 0.5));
		//return fixed4(r, r, r, 1);

		//float c = cnoise(pos) * 0.5 + 0.5;
		//return fixed4(c, 0, 0, 1);
//
//		// Raymarching
//        for (int i = 0; i < maxMarch; ++i) {
//            distance = DistanceFunc(pos);
//            len += distance;
//            pos += rayDir * distance;	//レイを進ませる
//			marchCount++;
//            if (distance < 0.001 || len > maxDist) break;	// 何かに衝突した or 最大移動距離に到達したら終了
//        }
//
//        if (distance > 0.001) discard;	// 近くに物体がない場合は終了
//
//        float depth = GetDepth(pos);	// デプス取得
//        float3 normal = GetNormal(pos);	// 法線取得
//
//		// グリッド
//		//float u = (1.0 - floor(fmod(pos.x, 2.0))) * 2;
//		//float v = (1.0 - floor(fmod(pos.y, 2.0))) * 2;
//
//		// 自前グリッド
//		float span = 1;
//		float width = 0.9;
//		//float r = step(width, fmod(pos.x + 10000 + _Time.y, span));
//		//float g = step(width, fmod(pos.y + 10000 + _Time.y, span));
//		//float b = step(width, fmod(pos.z + 10000 + _Time.y, span));
//		//float r = step(width, fmod(pos.x + 10000, span));
//		//float g = step(width, fmod(pos.y + 10000, span));
//		//float b = step(width, fmod(pos.z + 10000, span));
//
//		float glow = 0.0;
//		{
//			const float s = 0.0075;
//			float3 p = pos;
//			float3 n1 = GetNormal(pos);
//			float3 n2 = GetNormal(pos + float3(s, 0.0, 0.0));
//			float3 n3 = GetNormal(pos + float3(0.0, s, 0.0));
//			glow = (1.0 - abs(dot(rayDir, n1)))*0.5;
//			if (dot(n1, n2)<0.8 || dot(n1, n3)<0.8) {
//				glow += 0.6;
//			}
//		}
//
//		//{
//		//	float3 p = pos;
//		//	float grid1 = max(0.0, max((mod((p.x + p.y + p.z*2.0) - _Time.y*3.0, 5.0) - 4.0)*1.5, 0.0));
//		//	float grid2 = max(0.0, max((mod((p.x + p.y*2.0 + p.z) - _Time.y*2.0, 7.0) - 6.0)*1.2, 0.0));
//		//	float3 gp1 = abs(mod(p, float3(0.24, 0.24, 0.24)));
//		//	float3 gp2 = abs(mod(p, float3(0.32, 0.32, 0.32)));
//		//	if (gp1.x<0.23 && gp1.z<0.23) {
//		//		grid1 = 0.0;
//		//	}
//		//	if (gp2.y<0.31 && gp2.z<0.31) {
//		//		grid2 = 0.0;
//		//	}
//		//	glow += grid1 + grid2;
//		//}
//
//		float fog = min(1.0, (1.0 / float(maxMarch)) * float(marchCount))*1.5;
//		float3  fog2 = 0.001 * float3(1, 3, 10) * len;
//		glow *= min(1.0, 8.0 - (8.0 / float(maxMarch - 1)) * float(marchCount));
//
//        GBufferOut o;
//        o.diffuse  = float4(1.0, 1.0, 1.0, 1.0);
//		o.specular = float4(0.0, 0.0, 0.0, 1.0);
//		//o.specular = float4(0.5, 0.5, 0.5, 1.0);
//        //o.emission = tex2D(_MainTex, float2(u, v)) * 2;
//		//o.emission = float4(r, g, b, 1);
//		//o.emission = float4(0.25, 0.5, 1, 1) * max(max(r,g),b) * 2;
//		//o.emission = float4(0, 0, 0, 0);
//		o.emission = float4(float3(1.5 + glow*0.75, 1.5 + glow*0.75, 1.75 + glow)*fog + fog2, 1.0) * 1.0;
//		o.depth    = depth;
//        o.normal   = float4(normal, 1.0);
//
//#ifndef UNITY_HDR_ON
//        o.emission = exp2(-o.emission);
//#endif
//
//        return o;
    }

    ENDCG
}

}

Fallback Off
}