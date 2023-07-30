Shader "Custom/Shader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main (RGB)", 2D) = "white" {}

        _Occlusion("Occlusion (RGB)", 2D) = "red" {}
        _Roughness("Roughness (RGB)", 2D) = "green" {}
        _Metallic("Metallic (RGB)", 2D) = "blue" {}
        _Height("Height (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Occlusion;
        sampler2D _Roughness;
        sampler2D _Metallic;
        sampler2D _Height;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Occlusion;
            float2 uv_Roughness;
            float2 uv_Metallic;
            float2 uv_Height;
        };

        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 roughnessTex = tex2D(_Roughness, IN.uv_Roughness);
            fixed4 occlusionTex = tex2D(_Occlusion, IN.uv_Occlusion);

            mainTex.rgb = lerp(roughnessTex.rgb, occlusionTex.rgb, mainTex.a);

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            o.Albedo = c.rgb;

        }
        ENDCG
    }
    FallBack "Diffuse"
}
