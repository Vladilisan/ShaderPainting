Shader "Unlit/Dilation"
 {
     Properties
     {
         _MainTex ("Texture", 2D) = "white" {}  // main texture of this shader
         _PixelOffset("Distance",Range(0,1)) = 1  // use:  1 / textureDimension  (16x16 => 1/16 = 0.0625)
     }
     SubShader
     {
         Tags { "RenderType"="Opaque" }
         LOD 100
 
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
 
             sampler2D _MainTex;
             float4 _MainTex_ST;
             float _PixelOffset;
 
             v2f vert (appdata v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                 return o;
             }
             
             fixed4 frag (v2f i) : SV_Target
             {    
                 //min and max Vector
                 float2 _min = float2(0,0);
                 float2 _max = float2(1,1);
 
                 //get the color of 8 neighbour pixel
                 fixed4 U = tex2D(_MainTex,clamp(i.uv + float2(0,_PixelOffset),_min,_max));
                 fixed4 UR = tex2D(_MainTex,clamp(i.uv + float2(_PixelOffset,_PixelOffset),_min,_max));
                 fixed4 R = tex2D(_MainTex,clamp(i.uv + float2(_PixelOffset,0),_min,_max));
                 fixed4 DR = tex2D(_MainTex,clamp(i.uv + float2(_PixelOffset,-_PixelOffset),_min,_max));
                 fixed4 D = tex2D(_MainTex,clamp(i.uv + float2(0,-_PixelOffset),_min,_max));
                 fixed4 DL = tex2D(_MainTex,clamp(i.uv + float2(-_PixelOffset,-_PixelOffset),_min,_max));
                 fixed4 L = tex2D(_MainTex,clamp(i.uv + float2(-_PixelOffset,0),_min,_max));
                 fixed4 UL = tex2D(_MainTex,clamp(i.uv + float2(-_PixelOffset,_PixelOffset),_min,_max));
                 
                 //add all colors up to one final color
                 fixed4 finalColor = U + UR + R + DR + D + DL + L + UL;
 
                 //return final color
                 return finalColor;
             }
             ENDCG
         }
     }
 }
 