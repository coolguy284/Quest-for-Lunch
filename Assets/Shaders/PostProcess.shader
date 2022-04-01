Shader "Custom/PostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _vignette ("Vignette", Range(0, 1)) = 0
        _psychedelic ("Psychedelic", Range(0, 1)) = 0
        _bowl ("Bowl", Range(0, 1)) = 0
        _mandel ("Mandel", Range(0, 1)) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define PI 3.1415926535

            sampler2D _MainTex;
            float _vignette;
            float _psychedelic;
            float _bowl;
            float _mandel;

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
                float2 shiftvector = { 0.0f, 0.0f };
                o.uv = v.uv - shiftvector;
                return o;
            }

            float sq(float val) {
                return val * val;
            }

            float hypot(float x, float y) {
                return sqrt(x * x + y * y);
            }

            fixed4 frag(v2f i) : SV_Target {
                if (_psychedelic > 0.5 || _bowl > 0.5) {
                    float xorig = i.uv[0] - 0.5;
                    float yorig = i.uv[1] - 0.5;
                    float ang = atan2(yorig, xorig);
                    float dist = hypot(xorig, yorig);
                    if (_psychedelic > 0.5) {
                        ang = (ang * 4) - (PI / 2);
                    }
                    if (_bowl > 0.5) {
                        dist = tan(dist * 3) / 10;
                    }
                    float x = cos(ang) * dist + 0.5;
                    float y = sin(ang) * dist + 0.5;
                    i.uv[0] = x;
                    i.uv[1] = y;
                }
                
                fixed4 col = tex2D(_MainTex, i.uv);
                if (_vignette > 0.5) {
                    float multfact = 1 - saturate(1 - pow(hypot(i.uv[0] - 0.5, i.uv[1] - 0.5) * 1.4, 3));
                    float multfact2 = 1 - saturate(1 - pow(max(abs(i.uv[0] - 0.5), abs(i.uv[1] - 0.5)) * 1.6, 3));
                    col.r = lerp(col.r, 0, multfact);
                    col.g = lerp(col.g, 0, multfact);
                    col.b = lerp(col.b, 0, multfact);
                    col.r = lerp(col.r, 0, multfact2);
                    col.g = lerp(col.g, 0, multfact2);
                    col.b = lerp(col.b, 0, multfact2);
                }

                if (_mandel > 0.0) {
                    float x = i.uv[0] * 4 - 2;
                    float y = i.uv[1] * 4 - 2 + sin(i.uv[0] * 50 + _Time[1] * 50);

                    if (x * x + y * y > sq((1 - _mandel)) * 8) {
                        float zx = 0;
                        float zy = 0;
                        float zx2 = 0;
                        float zy2 = 0;
                        int iters = 0;
                        while (iters < 100 && zx * zx + zy * zy < 4) {
                            zx2 = zx * zx - zy * zy;
                            zy2 = 2 * zx * zy;
                            zx = zx2 + x;
                            zy = zy2 + y;
                            iters++;
                        }

                        if (zx * zx + zy * zy > 4) {
                            col.r = 0;
                            col.g = float(iters) / 20;
                            col.b = 0;
                        } else {
                            col = 0;
                        }
                    }
                }

                return col;
            }
            ENDCG
        }
    }
}
