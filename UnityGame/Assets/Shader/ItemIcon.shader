Shader "UI/ItemIcon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FxTex ("Fx Texture", 2D) = "white" {}
    }
    SubShader
    {        
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _FxTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 colFx = tex2D(_FxTex, (i.uv) * 10 + _Time.xy);

                 
                fixed is_outline = (1 - smoothstep(0.0, 0.8, length(col.rgb - fixed3(1, 1, 1)))) * col.a;


                fixed is_shine = step(0.99, sin(_Time.w * 1 + i.uv.y * 5 - i.uv.x * 1));
                is_shine *= 0.5;
                col.rgb = is_shine * fixed3(1, 1, 1) + (1 - is_shine) * col.rgb; 
                
                //return _Outline * is_outline + (1 - is_outline) * col * i.color;                
                return is_outline * i.color * colFx  + (1 - is_outline) * col;  // image color -> outline color
            }
            ENDCG
        }
    }
}
