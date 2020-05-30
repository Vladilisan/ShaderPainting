Shader "Unlit/FixIlsandEdges"
{
	
	SubShader
	{
		// =====================================================================================================================
		// TAGS AND SETUP ------------------------------------------
		Tags { "RenderType"="Opaque" }
		LOD 100
		ZTest  Off
		ZWrite Off
		Cull   Off
		Lighting Off
		Fog { Mode Off }

		Pass
		{

			
		// =====================================================================================================================
		// DEFINE AND INCLUDE ----------------------------------
		    CGPROGRAM


			#pragma  vertex   vert
			#pragma  fragment frag
			
			#include "UnityCG.cginc"

			// =====================================================================================================================
			// DECLERANTIONS ----------------------------------
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv	  : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv	  : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

					sampler2D	_MainTex;
			uniform	float4		_MainTex_TexelSize;
					sampler2D   _IlsandMap;
			
			// =====================================================================================================================
			// VERTEX FRAGMENT ----------------------------------
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv     = v.uv;
				#if UNITY_UV_STARTS_AT_TOP
				 if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1. - o.uv.y;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

			fixed4 col = tex2D(_MainTex, i.uv);
			half map  = tex2D(_IlsandMap,i.uv);
			
			half3 average = col;

			if (map.x < .2) {									// only take an average if it is not in a uv ilsand
				int n = 0;
				average = half3(0., 0., 0.);

				for (half x = -1.5; x <= 1.5; x++) {
					for (half y = -1.5; y <= 1.5; y++) {

						half3 c =  tex2Dlod(_MainTex, half4(i.uv + _MainTex_TexelSize*half2(x, y),0,0));
						half  m =  tex2Dlod(_IlsandMap, half4(i.uv + _MainTex_TexelSize*half2(x, y),0,0));

							   n += step(0.1, m);
						 average += c * step(0.1, m);

					}
				}
				average /= n;
				col.xyz = average;
			}
		
				return col;
			}
			ENDCG
		}
	}
}