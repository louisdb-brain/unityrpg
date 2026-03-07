Shader "Custom/SpriteShadowSkew"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color     ("Tint", Color) = (1,1,1,1)

        _SkewAmount ("Skew Amount", Float) = 20
        _SkewDir    ("Skew Direction (XY)", Vector) = (1, 0, 0, 0)

        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            float _SkewAmount;
            float4 _SkewDir;
            float _AlphaCutoff;

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color    : COLOR;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color    : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;

                // Use the provided 2D direction
                float2 dir = _SkewDir.xy;
                float len = length(dir);
                if (len < 1e-5)
                {
                    dir = float2(1, 0); // default direction if zero
                }
                else
                {
                    dir /= len;
                }

                // Bottom of sprite (uv.y = 0) has no skew
                // Top of sprite (uv.y = 1) has full skew
                float skew = _SkewAmount * v.texcoord.y;

                // Sprite quad is in X Y in object space
                v.vertex.xy += dir * skew;

                #ifdef PIXELSNAP_ON
                    o.vertex = UnityPixelSnap(UnityObjectToClipPos(v.vertex));
                #else
                    o.vertex = UnityObjectToClipPos(v.vertex);
                #endif

                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.texcoord) * i.color;
                clip(c.a - _AlphaCutoff);
                return c;
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}