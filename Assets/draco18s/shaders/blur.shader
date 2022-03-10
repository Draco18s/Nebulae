Shader "Cellular/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Pixels("Cells", int) = 256
    }
    SubShader
    {
        // No culling or depth
        Cull back ZWrite Off ZTest Always

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
            float4 _MainTex_TexelSize;
            float _Pixels;

            /*static float3 blurmask[3] = {
                float3(1,2,1),
                float3(2,4,2),
                float3(1,2,1)
            };

            static float masktotal = 
                blurmask[0].x+blurmask[0].y+blurmask[0].z+
                blurmask[1].x+blurmask[1].y+blurmask[1].z+
                blurmask[2].x+blurmask[2].y+blurmask[2].z;*/
            
            static float blurmask[5][5] = {
                1, 4, 7, 4,1,
                4,16,26,16,4,
                7,26,41,26,7,
                4,16,26,16,4,
                1, 4, 7, 4,1,
            };

            static float masktotal = 273;

            float4 CubicPolate(float4 v0, float4 v1, float4 v2, float4 v3, float frac) {
                //frac*=0.5;
                float4 A = (v3-v2)-(v0-v1);
                float4 B = (v0-v1)-A;
                float4 C = v2-v0;
                float4 D = v1;

                return (A*pow(frac,3)+B*pow(frac,2)+C*frac+D);
                //return lerp(v1, v2, frac);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed2 uv = round(i.uv * (_Pixels)) / (_Pixels);
                //half s = 1 / _Pixels;
                //half t = 1 / _Pixels;
                half s = 1/_Pixels;
                half t = 1/_Pixels;
                /*float3 tl = tex2D(_MainTex, (uv + fixed2(-s, -t))%1)*blurmask[0].x;    // Top Left
                float3 cl = tex2D(_MainTex, (uv + fixed2(-s, -0))%1)*blurmask[0].y;    // Centre Left
                float3 bl = tex2D(_MainTex, (uv + fixed2(-s, +t))%1)*blurmask[0].z;    // Bottom Left
                float3 tc = tex2D(_MainTex, (uv + fixed2(-0, -t))%1)*blurmask[1].x;    // Top Centre
                float3 cc = tex2D(_MainTex, (uv + fixed2(+0, -0))%1)*blurmask[1].y;    // Centre Centre
                float3 bc = tex2D(_MainTex, (uv + fixed2(+0, +t))%1)*blurmask[1].z;    // Bottom Centre
                float3 tr = tex2D(_MainTex, (uv + fixed2(+s, -t))%1)*blurmask[2].x;    // Top Right
                float3 cr = tex2D(_MainTex, (uv + fixed2(+s, +0))%1)*blurmask[2].y;    // Centre Right
                float3 br = tex2D(_MainTex, (uv + fixed2(+s, +t))%1)*blurmask[2].z;    // Bottom Right
                float3 count = tl + cl + bl + tc + bc + tr + cr + br + cc;
                count /= (masktotal);
                return float4(count, 1);*/

                float3 count = (0,0,0);
                for(int X = 0; X < 5; X++)
                    for(int Y = 0; Y < 5; Y++)
                        count += tex2D(_MainTex, (i.uv+float2(s*(X-1), t*(Y-1)))%1)*blurmask[X][Y];

                count /= (masktotal);
                return float4(count, 1);
                /*float2 frac = float2(0.5,0.5);
                int2 uv = floor(i.uv * (_MainTex_TexelSize.zw/_Pixels));
                float2 frac_real = (i.uv * (_MainTex_TexelSize.zw/_Pixels))-uv;

                half s = 1/(_MainTex_TexelSize.z/_Pixels);
                half t = 1/(_MainTex_TexelSize.w/_Pixels);

                float4 ndata[4][4];
                for(int X = 0; X < 4; X++)
                    for(int Y = 0; Y < 4; Y++)
                        ndata[X][Y] = tex2D(_MainTex, (i.uv+float2(s*(X-1), t*(Y-1)))%1);

                float4 x1 = CubicPolate(ndata[0][0], ndata[1][0], ndata[2][0], ndata[3][0], frac_real.x);
                float4 x2 = CubicPolate(ndata[0][1], ndata[1][1], ndata[2][1], ndata[3][1], frac_real.x);
                float4 x3 = CubicPolate(ndata[0][2], ndata[1][2], ndata[2][2], ndata[3][2], frac_real.x);
                float4 x4 = CubicPolate(ndata[0][3], ndata[1][3], ndata[2][3], ndata[3][3], frac_real.x);

                float4 mask = CubicPolate(x1, x2, x3, x4, frac_real.y);
                return float4(mask.xyz, 1);*/
            }
            ENDCG
        }
    }
}
