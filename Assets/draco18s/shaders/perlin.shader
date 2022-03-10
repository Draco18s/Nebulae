Shader "Noise/Nebula Clouds"
{
    Properties
    {
        _MaskTex ("Mask", 2D) = "white" {}
        _PerlinInputs ("Perlin Inputs", Vector) = (.25, .5, .5, 1)
        _Distort ("Distortion", float) = 0.3
        _Pow ("Strength", float) = 0.3
        _Color("Color", Color) = (0, 1, 0, 1)
        _Highlight("Highlight", Color) = (0, 0.5, 1, 1)
        _Scale ("Scale", float) = 1
        _NebulaCenter("Focal Center (World.xyz)", Vector) = (0, 1, 0, 0)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull back 
        LOD 100

        Pass
        {
            CGPROGRAM
            // Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
            #pragma exclude_renderers d3d11 gles
            #pragma vertex vert
            #pragma fragment frag
            #define PI 3.14159265358979323846

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : TEXCOORD1;
            };

            v2f vert (float4 vertex : POSITION, float2 uv : TEXCOORD0, out float4 outpos : SV_POSITION)
            {
                v2f o;
                o.uv = uv;
                o.vertex = vertex;
                outpos = UnityObjectToClipPos(vertex);
                return o;
            }

            sampler2D _MaskTex;
            float4 _PerlinInputs;
            float4 _Highlight;
            float4 _Color;
            float4 _NebulaCenter;
            uniform float _Distort;
            uniform float4 _MaskTex_ST;
            uniform float4 _MaskTex_TexelSize;
            uniform float _Pow;
            uniform float _Scale;

            float rand(float2 c){
                return frac(sin(dot(c.xy ,float2(12.9898,78.233))) * 43758.5453);
            }

            float bicubicInterpolation(float4 p, float x){
                return p.y + 0.5f * x*(p.z - p.x + x*(2.0f*p.x - 5.0f*p.y + 4.0f*p.z - p.w + x*(3.0f*(p.y - p.z) + p.w - p.x)));
            }

            float noise(float2 p, float freq ){
                int s = sign(p);
                p = abs(p);
                float unit = 1/freq;
                float2 ij = floor(p/unit);
                float2 xy = (p%unit)/unit * s;
                xy = .5*(1.-cos(PI*xy));
                float a = rand((ij+float2(0.,0.)));
                float b = rand((ij+float2(1.,0.)));
                float c = rand((ij+float2(0.,1.)));
                float d = rand((ij+float2(1.,1.)));
                
                float x1 = lerp(a, b, xy.x);
                float x2 = lerp(c, d, xy.x);
                return lerp(x1, x2, xy.y);
            }

            float bicubicNoise(float2 p, float freq ){
                int s = sign(p);
                p = abs(p);
                float unit = 1/freq;
                float2 ij = floor(p/unit);
                float2 xy = (p%unit)/unit * s;
                xy = .5*(1.-cos(PI*xy));
                
                return bicubicInterpolation(float4(
                    bicubicInterpolation(float4(rand((ij+float2(-1.,-1.))),rand((ij+float2(0.,-1.))),rand((ij+float2(1.,-1.))),rand((ij+float2(2.,-1.)))),xy.x),
                    bicubicInterpolation(float4(rand((ij+float2(-1.,0.))),rand((ij+float2(0.,0.))),rand((ij+float2(1.,0.))),rand((ij+float2(2.,0.)))),xy.x),
                    bicubicInterpolation(float4(rand((ij+float2(-1.,1.))),rand((ij+float2(0.,1.))),rand((ij+float2(1.,1.))),rand((ij+float2(2.,1.)))),xy.x),
                    bicubicInterpolation(float4(rand((ij+float2(-1.,2.))),rand((ij+float2(0.,2.))),rand((ij+float2(1.,2.))),rand((ij+float2(2.,2.)))),xy.x)
                ), xy.y);
            }

            float bNoise(float2 p, int res, float f){
                float persistance = .5;
                float n = 0.;
                float normK = 0.;
                float amp = 1.;
                for (int iCount = 0; iCount<=res; iCount++){
                    n+=amp*bicubicNoise(p, f);
                    f*=2.;
                    normK+=amp;
                    amp*=persistance;
                }
                float nf = n/normK;
                return nf*nf;
            }

            float pNoise(float2 p, int res, float f){
                float persistance = .5;
                float n = 0.;
                float normK = 0.;
                float amp = 1.;
                for (int iCount = 0; iCount<=res; iCount++){
                    n+=amp*noise(p, f);
                    f*=2.;
                    normK+=amp;
                    amp*=persistance;
                }
                float nf = n/normK;
                return nf*nf;
            }

            float4 when_lt(float4 x, float4 y) {
                return max(sign(y - x), 0.0);
            }
            float4 when_ge(float4 x, float4 y) {
                return 1.0 - when_lt(x, y);
            }

            float easeInOutQuad(float x) {
                x = clamp(x, 0, 1);
                return (2 * x * x)*when_lt(x, 0.5) + (1 - pow(-2 * x + 2, 2) / 2) * when_ge(x, 0.5);
            }

            float4 ColorToHSV(float4 color)
            {
                float _min, _max, delta;
                _min = min(min( color.r, color.g), color.b );
                _max = max(max( color.r, color.g), color.b );
                float v = _max;				// v
                float s;
                float h;
                delta = _max - _min;
                if(_max != 0 )
                    s = delta / _max;		// s
                else {
                    // r = g = b = 0		// s = 0, v is undefined
                    s = 0;
                    h = -1;
                    return float4(h, s, v, 0);
                }
                if( color.r == _max )
                    h = ( color.g - color.b ) / delta;		// between yellow & magenta
                else if( color.g == _max )
                    h = 2 + ( color.b - color.r ) / delta;	// between cyan & yellow
                else
                    h = 4 + ( color.r - color.g ) / delta;	// between magenta & cyan
                h *= 60;				// degrees
                if( h < 0 )
                    h += 360;
                return float4(h, s, v, color.a);
            }

            float4 HSVtoRGB(float4 color) {
                int i;
                float h = color.x;
                float s = color.y;
                float v = color.z;
                float f, p, q, t, r, g, b;
                if( s == 0 ) {
                    // achromatic (grey)
                    r = g = b = v;
                    return float4(r, g, b, color.a);
                }
                h /= 60;			// sector 0 to 5
                i = floor( h );
                f = h - i;			// factorial part of h
                p = v * ( 1 - s );
                q = v * ( 1 - s * f );
                t = v * ( 1 - s * ( 1 - f ) );
                switch( i ) {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;
                    default:		// case 5:
                        r = v;
                        g = p;
                        b = q;
                        break;
                }

                return float4(r, g, b, color.a);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uvVal = i.uv*_Scale;
                float4 wPos = mul(unity_ObjectToWorld, i.vertex)/16.;

                float3 col = float3(
                    1.25*(bNoise(uvVal+wPos.xy, 3, _PerlinInputs.w+1)+bNoise(uvVal+wPos.zy, 3, _PerlinInputs.w+1)),
                    1.25*(bNoise(uvVal+wPos.xz, 3, _PerlinInputs.w+1)+bNoise(uvVal+wPos.yx, 3, _PerlinInputs.w+1)),
                    1.25*(bNoise(uvVal+wPos.yz, 3, _PerlinInputs.w+1)+bNoise(uvVal+wPos.zx, 3, _PerlinInputs.w+1)));

                float2 offset = float2(_Distort*(col.r-.5),_Distort*(col.g-.5));

                float3 col2 = float3(
                    1.25*(pNoise(uvVal+offset+_PerlinInputs.yx, 3, _PerlinInputs.w*2)+pNoise(uvVal+offset+_PerlinInputs.yz, 3, _PerlinInputs.w*2)),
                    1.25*(pNoise(uvVal+offset+_PerlinInputs.zy, 3, _PerlinInputs.w*2)+pNoise(uvVal+offset+_PerlinInputs.xz, 3, _PerlinInputs.w*2)),
                    1.25*(pNoise(uvVal+offset+_PerlinInputs.zx, 3, _PerlinInputs.w*2)+pNoise(uvVal+offset+_PerlinInputs.xy, 3, _PerlinInputs.w*2)));

                float2 p1 = (i.uv-0.5)*2;

                float distw = col.b/(1.25*1.25);

                /// This will cause a circular gradient effect from world origin
                //float4 posOffset = (_NebulaCenter-wPos);
                //float distw = clamp(sqrt(posOffset.x*posOffset.x+posOffset.y*posOffset.y+posOffset.z*posOffset.z), 0, 1);
                
                float dist = sqrt(p1.x*p1.x+p1.y*p1.y);
                float radA = (when_lt(dist,0.8) + (easeInOutQuad(1 - ((dist-.8) * 5)) * when_ge(dist,0.8)));

                float s = _MaskTex_TexelSize.x;
                float t = _MaskTex_TexelSize.y;
                /// If texture uses point interpolation:
                /// Does not allow texture scale as doing so messes with this computed bilinear interpolation
                /*float3 tl = tex2D(_MaskTex,((i.uv+float2(-0,-0)) + _MaskTex_ST.zw)%1);
                float3 tr = tex2D(_MaskTex,((i.uv+float2(+s,-0)) + _MaskTex_ST.zw)%1);
                float3 bl = tex2D(_MaskTex,((i.uv+float2(-0,+t)) + _MaskTex_ST.zw)%1);
                float3 br = tex2D(_MaskTex,((i.uv+float2(+s,+t)) + _MaskTex_ST.zw)%1);

                float2 frac = ((i.uv*_MaskTex_TexelSize.zw)-floor(i.uv*_MaskTex_TexelSize.zw));

                float3 mask = lerp(lerp(tl, tr, frac.x), lerp(bl, br, frac.x), frac.y);*/

                // If texture uses bilinear interpolation:
                // Texture must be seamless
                float3 mask = tex2D(_MaskTex,((i.uv+float2(-0,-0)) * _MaskTex_ST.xy + _MaskTex_ST.zw)%1);
                
                float3 final_color = HSVtoRGB(lerp(ColorToHSV(_Color), ColorToHSV(_Highlight), distw)).rgb;
                float final_alpha = min(col2.r,0.99)*min(col2.g,0.99)*min(col.b,0.99);
                final_alpha *= min(mask.r,0.99)*min(mask.g,0.99);
                final_alpha = pow(final_alpha, _Pow);
                final_alpha *= radA;
                final_alpha = min(final_alpha, 0.9);
                return float4(final_color,final_alpha);
            }
            ENDCG
        }
    }
}
