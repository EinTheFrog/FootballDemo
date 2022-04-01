Shader "Unlit/ForceShader"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _Color1 ("Color 1", Color) = (1.0, 0.5, 0.5, 1.0)
        _Color2 ("Color 2", Color) = (1.0, 0.5, 0.5, 1.0)
        _ColorBorder("Color border", Float) = 0.8
        _DeltaTime ("Delta time", Float) = 1.0
        _ForceMult ("Force multiplier", Float) = 2.0
        _ForceExpStart ("Force exponent start", Float) = 3.0
        _MinForce ("Min force", Float) = 20.0
        _MaxForce ("Max Force", Float) = 400.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _Color1;
            fixed4 _Color2;
            float _ColorBorder;
            float _DeltaTime;
            float _ForceMult;
            float _ForceExpStart;
            float _MinForce;
            float _MaxForce;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float force : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                float force = _DeltaTime * _ForceMult;
                force = exp(force + _ForceExpStart);
                force = clamp(force, _MinForce, _MaxForce);
                force = (force - _MinForce) / (_MaxForce - _MinForce);
                o.force = force;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed coef = log(i.uv.y + 0.5) + _ColorBorder;
                fixed4 col = _Color1 * coef + _Color2 * (1 - coef);
                col.a = i.force - i.uv.y;
                clip(col);
                return col;
            }
            ENDCG
        }
    }
}
