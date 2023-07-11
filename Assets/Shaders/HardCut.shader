// logic from https://github.com/Broxxar/PixelArtPipeline/blob/master/Assets/Shaders/ToonLitSprite.shader
Shader "Custom/HardCut3" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset]
        _NormalTex ("Normal Map", 2D) = "bump" {}
    }
    
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            
            sampler2D _MainTex;
            sampler2D _NormalTex;
            
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 tangent : TEXCOORD2;
                float3 bitangent : TEXCOORD3;
            };
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.tangent = UnityObjectToWorldNormal(v.tangent);
                o.bitangent = cross(-o.tangent, o.normal) * v.tangent.w;
                return o;
            }
            
            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target {
                fixed4 diffuseTex = tex2D(_MainTex, float2(0, 0));
                clip(diffuseTex.a > 0 ? 1 : -1);
                float3 normalTex = normalize(tex2D(_NormalTex, i.uv) * 2 - 1);
                normalTex.z *= facing;
                float3 N = normalize(i.tangent) * normalTex.r + normalize(i.bitangent) * normalTex.g + normalize(i.normal) * normalTex.b;
                half3 toonLight = saturate(dot(N, _WorldSpaceLightPos0)) > 0.3 ? _LightColor0 : 0.35/*unity_AmbientSky*/;
                half3 diffuse = diffuseTex * toonLight;
                return fixed4(diffuse, 0);
            }
            ENDCG
        }
    }
}
