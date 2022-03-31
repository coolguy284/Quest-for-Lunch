Shader "Custom/PostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            float sq ( float val )
            {
                return val * val;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float multfact = 1 - min(max(1 - pow(sqrt(sq(i.uv[0] - 0.5) + sq(i.uv[1] - 0.5)) * 1.5, 3), 0), 1);
                float multfact2 = 1 - min(max(1 - pow(max(abs(i.uv[0] - 0.5), abs(i.uv[1] - 0.5)) * 1.5, 3), 0), 1);
                col.r = lerp(col.r, 0, multfact);
                col.g = lerp(col.g, 0, multfact);
                col.b = lerp(col.b, 0, multfact);
                col.r = lerp(col.r, 0, multfact2);
                col.g = lerp(col.g, 0, multfact2);
                col.b = lerp(col.b, 0, multfact2);
                return col;
            }
            ENDCG
        }
    }
}
