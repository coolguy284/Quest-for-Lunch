Shader "Custom/Pixelation" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _pixelsWidth ("_pixelsWidth", Float) = 0
        _pixelsHeight ("_pixelsHeight", Float) = 0
    }
    SubShader {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _pixelsWidth;
            float _pixelsHeight;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                i.uv.x = floor(i.uv.x * _pixelsWidth) / _pixelsWidth;
                i.uv.y = floor(i.uv.y * _pixelsHeight) / _pixelsHeight;
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
