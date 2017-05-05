Shader "Raymarching/DarkMatter"
{

Properties
{
    _MainTex ("Main Texture", 2D) = "" {}
}

SubShader
{

Tags { "RenderType" = "Opaque" "DisableBatching" = "True" "Queue" = "Geometry+10" }
Cull Off

Pass
{
    Tags { "LightMode" = "Deferred" }

	// Deferred Rendering のベースとライティングのパスでは Stencil バッファはライティング用途に使われます。
    Stencil 
    {
        Comp Always
        Pass Replace
        Ref 128
    }

    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 5.0
    #pragma multi_compile ___ UNITY_HDR_ON

    #include "UnityCG.cginc"
    #include "Assets/Shaders/Libs/Utils.cginc"
    #include "Assets/Shaders/Libs/Primitives.cginc"
	//#include "Libs/ClassicNoise3D.cginc"
    
	struct ShaderData
	{
		float beat;
		int count;
	};

	struct KaiwareData
	{
		float3 position;
		float3 velocity;
		float starttScale;
		//float scale;
		float3 axis;
		float angle;
		float rotateSpeed;
		//float duration;
		//float time;
	};
	
	StructuredBuffer<ShaderData> _ShaderData;
	StructuredBuffer<KaiwareData> _KaiwareData;

	//float hex(float2 p, float2 h)
	//{
	//	float2 q = abs(p);
	//	return max(q.x - h.y, max(q.x + q.y*0.57735, q.y*1.1547) - h.x);
	//}

	// カイワレ用距離関数
	float DistanceFuncKaiware(float3 pos, KaiwareData k)
	{
		//// スケール
		float sc = 1.0 + _ShaderData[0].beat;

		float3 p = pos - k.position;

		// 回転
		p = rotate(p, k.angle, k.axis);

		// スケール
		p = p / sc;

		// 頭部
		float d1 = roundBox(p, float3(1, 0.8, 1), 0.1);

		// くちばし
		float d2_0 = roundBox(p - float3(0, -0.2, 0.7), float3(0.8, 0.25, 0.4), 0.1);
		//float d2_0 = box(p - float3(0, -0.2, 0.7), float3(0.8, 0.25, 0.4));
		float d2_1 = box(p - float3(0, -0.0, 0.7), float3(1.1, 0.35, 1.1));	// 上半分
		float d2_2 = box(p - float3(0, -0.4, 0.7), float3(1.1, 0.35, 1.1));	// 下半分
		float d2_3 = roundBox(p - float3(0, -0.2, 0.7), float3(0.75, 0.1, 0.35), 0.1);	// 溝

		float d2_top = max(d2_0, d2_1);
		float d2_bottom = max(d2_0, d2_2);
		float d2 = min(min(d2_top, d2_bottom), d2_3);

		// はっぱの茎
		float d3_0 = Capsule(p, float3(0, 0.5, 0), float3(0, 1, 0), 0.05);
		// 葉っぱ
		float d3_1 = ellipsoid(p - float3(0.2,1,0), float3(0.2, 0.025, 0.1));
		float d3_2 = ellipsoid(p - float3(-0.2, 1, 0), float3(0.2, 0.025, 0.1));
		float d3 = min(d3_0, min(d3_1, d3_2));

		// 目
		float d4_0 = Capsule(p, float3(0.2, 0.25, 0.6), float3(0.4, 0.2, 0.6), 0.025);
		float d4_1 = Capsule(p, float3(-0.2, 0.25, 0.6), float3(-0.4, 0.2, 0.6), 0.025);
		float d4 = min(d4_0, d4_1);

		// 合成
		float sum = max(min(min(d1, d2), d3), -d4);

		sum *= sc;

		return sum;
	}

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
		float kwr = 100000;	// 最大移動距離;
		float sc = 1 + _ShaderData[0].beat;
		for (int i = 0; i < _ShaderData[0].count; i++) {
			//float k = length(_KaiwareData[i].position - pos);
			float k = sphere(pos - _KaiwareData[i].position, sc);
			if (k <= sc) {
				k = DistanceFuncKaiware(pos, _KaiwareData[i]);
			}
			if (kwr >= k) {
				kwr = k;
			}
		}
		
		// test
		//float kwr = DistanceFuncKaiware(pos, float3(0, 0, 0));

#if false
		// 六角形地面
		float h = -abs(pos.y) + 2.5;	// 天井と床
		float scale = max(1.0, min(abs(pos.y)*0.5, 1.2)) * 4;
		float2 grid = float2(0.692, 0.4) * scale;
		float radius = 0.22 * scale;

		float2 p1 = mod2(pos.xz, grid) - grid*float2(0.5,0.5);
		float c1 = hex(p1, float2(radius, radius));

		float2 p2 = mod2(pos.xz + grid*0.5, grid) - grid*float2(0.5,0.5);
		float c2 = hex(p2, float2(radius, radius));

		float hexd = min(c1, c2);
		h += max(hexd, -0.005)*0.75;
#else
		// 六角形地面２
		float radius = 1.5;
		float space = 0.25;
		float wave = 0.5;
		float3 _Scale = float3(1, 2.5, 1);
		float height = abs(_Scale.y) * 0.5 - wave;
		float3 scale = abs(_Scale * 0.5);

		float pitch = radius * 2 + space;
		float3 offset = float3(pitch * 0.5, 0.0, pitch * 0.866);
		float3 loop = float3(offset.x * 2, 1.0, offset.z * 2);

		float3 p1 = pos;
		float3 p2 = pos + offset;

		// calculate indices
		float3 pi1_ = floor(p1 / loop);
		float2 pi1 = pi1_.xz;
		float3 pi2_ = floor(p2 / loop);
		float2 pi2 = pi2_.xz;
		
		p1 = repeat(p1, loop);
		p2 = repeat(p2, loop);

		// draw hexagonal prisms with random heights
		float ti = 5 * PI * _Time.x;
		float ws = wave * _ShaderData[0].beat;
		float dy1 = sin(10 * nrand(pi1) + ti) * ws;
		float dy2 = sin(10 * nrand(pi2) + ti) * ws;
		float d1 = hexagonalPrismY(float3(p1.x, pos.y + dy1 + 5, p1.z), float2(radius, height));
		float d2 = hexagonalPrismY(float3(p2.x, pos.y + dy2 + 5, p2.z), float2(radius, height));
		float d1top = hexagonalPrismY(float3(p1.x, pos.y - dy1 - 5, p1.z), float2(radius, height));
		float d2top = hexagonalPrismY(float3(p2.x, pos.y - dy2 - 5, p2.z), float2(radius, height));
		d1 = min(d1, d1top);
		d2 = min(d2, d2top);

		float h = min(d1, d2);
#endif

		//float tenjou = floor(pos - float3(0, 5, 0));
		//float yuka = floor(abs(pos - float3(0, -2.5, 0)));
		return smoothMin(h, kwr, 3);
		//return (hit)?smoothMin(yuka, kwr, 3) : yuka;
		//return min(yuka, kwr);
		//return h;

		//float r = abs(sin(2 * PI * _Time.y / 2.0));
  //      float d1 = roundBox(repeat(pos, float3(6, 6, 6)), 1, r);
  //      float d2 = sphere(pos, 3.0);


  //      float d3 = floor(pos - float3(0, -3, 0));
  //      return smoothMin(smoothMin(d1, d2, 1.0), d3, 1.0);
    }

	// 上でDistanceFuncを定義しているのでここでインクルードしないとだめ
    #include "Assets/Shaders/Libs/Raymarching.cginc"

    sampler2D _MainTex;

    GBufferOut frag(VertOutput i)
    {
        float3 rayDir = GetRayDirection(i.screenPos);	// レイの方向

        float3 camPos = GetCameraPosition() + float3(0, 0, 0);	// カメラの位置
        float maxDist = GetCameraMaxDistance();	// 最大移動距離

        float distance = 0.0;
        float len = 0.0;
        float3 pos = camPos + _ProjectionParams.y * rayDir;	// Near Planeから計算開始
		int marchCount = 0;
		int maxMarch = 512;

		// Raymarching
        for (int i = 0; i < maxMarch; ++i) {
            distance = DistanceFunc(pos);
            len += distance;
            pos += rayDir * distance;	//レイを進ませる
			marchCount++;
            if (distance < 0.001 || len > maxDist) break;	// 何かに衝突した or 最大移動距離に到達したら終了
        }

        if (distance > 0.001) discard;	// 近くに物体がない場合は終了

        float depth = GetDepth(pos);	// デプス取得
        float3 normal = GetNormal(pos);	// 法線取得

		// グリッド
		//float u = (1.0 - floor(fmod(pos.x, 2.0))) * 2;
		//float v = (1.0 - floor(fmod(pos.y, 2.0))) * 2;

		// 自前グリッド
		float span = 1;
		float width = 0.9;
		//float r = step(width, fmod(pos.x + 10000 + _Time.y, span));
		//float g = step(width, fmod(pos.y + 10000 + _Time.y, span));
		//float b = step(width, fmod(pos.z + 10000 + _Time.y, span));
		//float r = step(width, fmod(pos.x + 10000, span));
		//float g = step(width, fmod(pos.y + 10000, span));
		//float b = step(width, fmod(pos.z + 10000, span));

		float glow = 0.0;
		{
			const float s = 0.0075;
			float3 p = pos;
			float3 n1 = GetNormal(pos);
			float3 n2 = GetNormal(pos + float3(s, 0.0, 0.0));
			float3 n3 = GetNormal(pos + float3(0.0, s, 0.0));
			glow = (1.0 - abs(dot(rayDir, n1)))*0.5;
			if (dot(n1, n2)<0.8 || dot(n1, n3)<0.8) {
				glow += 0.6;
			}
		}

		//{
		//	float3 p = pos;
		//	float grid1 = max(0.0, max((mod((p.x + p.y + p.z*2.0) - _Time.y*3.0, 5.0) - 4.0)*1.5, 0.0));
		//	float grid2 = max(0.0, max((mod((p.x + p.y*2.0 + p.z) - _Time.y*2.0, 7.0) - 6.0)*1.2, 0.0));
		//	float3 gp1 = abs(mod(p, float3(0.24, 0.24, 0.24)));
		//	float3 gp2 = abs(mod(p, float3(0.32, 0.32, 0.32)));
		//	if (gp1.x<0.23 && gp1.z<0.23) {
		//		grid1 = 0.0;
		//	}
		//	if (gp2.y<0.31 && gp2.z<0.31) {
		//		grid2 = 0.0;
		//	}
		//	glow += grid1 + grid2;
		//}

		float fog = min(1.0, (1.0 / float(maxMarch)) * float(marchCount))*1.5;
		float3  fog2 = 0.001 * float3(1, 3, 10) * len;
		glow *= min(1.0, 8.0 - (8.0 / float(maxMarch - 1)) * float(marchCount));

        GBufferOut o;
        o.diffuse  = float4(1.0, 1.0, 1.0, 1.0);
		o.specular = float4(0.0, 0.0, 0.0, 1.0);
		//o.specular = float4(0.5, 0.5, 0.5, 1.0);
        //o.emission = tex2D(_MainTex, float2(u, v)) * 2;
		//o.emission = float4(r, g, b, 1);
		//o.emission = float4(0.25, 0.5, 1, 1) * max(max(r,g),b) * 2;
		//o.emission = float4(0, 0, 0, 0);
		o.emission = float4(float3(1.5 + glow*0.75, 1.5 + glow*0.75, 1.75 + glow)*fog + fog2, 1.0) * 1.0;
		o.depth    = depth;
        o.normal   = float4(normal, 1.0);

#ifndef UNITY_HDR_ON
        o.emission = exp2(-o.emission);
#endif

        return o;
    }

    ENDCG
}

}

Fallback Off
}