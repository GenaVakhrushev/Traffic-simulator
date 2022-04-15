Shader "Custom/BrushOnTerrain" {
    Properties{
        _MainTex("Base (RGB)", 2D) = "white" {}
        _DrawTexture("Draw texture", 2D) = "white" {}
        _Position("Position", Vector) = (0,0,0,0)
        _Radius("Radius", Range(0, 100)) = 20
        _IsActive("IsActive", Float) = 1
        _AreaColor("Area Color", Color) = (1, 1, 1)
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }

            CGPROGRAM
            #pragma surface surf Lambert

            sampler2D _MainTex;
            sampler2D _DrawTexture;
            float3 _Position;
            float _Border;
            float _Radius;
            float _IsActive;
            fixed3 _AreaColor;

            struct Input {
                float2 uv_MainTex;
                float3 worldPos;
            };

            void surf(Input IN, inout SurfaceOutput o) {
                float4 mainTexColor = tex2D(_MainTex, IN.uv_MainTex);
                float texX = abs(_Position.x - _Radius - IN.worldPos.x) / (2 * _Radius);
                float texY = abs(_Position.z - _Radius - IN.worldPos.z) / (2 * _Radius);
                float2 drawTexCoord = float2(texX, texY);
                float4 drawTexColor = tex2D(_DrawTexture, drawTexCoord);
                float distX = abs(_Position.x - IN.worldPos.x);
                float distZ = abs(_Position.z - IN.worldPos.z);

                if (distX < _Radius && distZ < _Radius && _IsActive == 1) {
                    o.Albedo = mainTexColor.rgb * drawTexColor.a + mainTexColor.rgb * _AreaColor * (1 - drawTexColor.a);
                    o.Alpha = drawTexColor.a;
                }
                else {
                    o.Albedo = mainTexColor.rgb;
                    o.Alpha = mainTexColor.a;
                }  
            }
            ENDCG
        }
            FallBack "Diffuse"
}