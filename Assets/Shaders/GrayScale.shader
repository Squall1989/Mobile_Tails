Shader "Surface/Grayscale" {
    Properties{
         _MainTex("Texture", 2D) = "white" {}
    }
        SubShader{
          Tags { "RenderType" = "Opaque" }

          CGPROGRAM
          #pragma surface surf Lambert

          struct Input {
              float2 uv_MainTex;
          };

          sampler2D _MainTex;

          void surf(Input IN, inout SurfaceOutput o) {
              o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
              o.Albedo = 0.299 * o.Albedo.r + 0.587 * o.Albedo.g + 0.184 * o.Albedo.b;
          }
          ENDCG
    }
        Fallback "Diffuse"
}