Shader "Custom/TestEfct" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
    SubShader {
        // No culling or depth
        Cull Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            sampler2D _MainTex;
            
            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                //float multfact = pow(sin(((i.vertex.x - i.vertex.y)) * 6.28 * 5.5), 4);
                //float multfact2 = pow(sin(((i.vertex.x + i.vertex.y)) * 6.28 * 5.5), 4);
                //col.rgb = col.rgb * (1 + ((multfact + multfact2) * 0.3));
                return col;
            }
            ENDCG
        }
    }
}
