Shader "Custom/Unlit Drawing Shader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _RenderTexture;
            float4 _RenderTexture_ST;

            fixed4 _BackgroundColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _RenderTexture);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // return tex2D(_RenderTexture, i.uv);

                fixed4 curColor = tex2D(_RenderTexture, i.uv);
                
                if (curColor.a > 0)
                {
                    return curColor;
                }
                else
                {
                    return _BackgroundColor;
                }
            }
            ENDCG
        }
    }
}
