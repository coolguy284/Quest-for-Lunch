Shader "Custom/Pixelation" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _pixelWidthInv ("_pixelWidthInv", Float) = 0
        _pixelHeightInv ("_pixelHeightInv", Float) = 0
        _pixelXOffset ("_pixelXOffset", Float) = 0
        _pixelYOffset ("_pixelYOffset", Float) = 0
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
            float _pixelWidthInv;
            float _pixelHeightInv;
            float _pixelXOffset;
            float _pixelYOffset;
            
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
                i.uv.x -= _pixelXOffset;
                i.uv.y -= _pixelYOffset;
                i.uv.x = floor((i.uv.x) * _pixelWidthInv) / _pixelWidthInv;
                i.uv.y = floor((i.uv.y) * _pixelHeightInv) / _pixelHeightInv;
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
