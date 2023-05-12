Shader "Custom/BillboardYFog" {
    Properties{
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
    }
        SubShader{
            Tags 
            { 
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
                "DisableBatching" = "True"
                "PreviewType" = "Plane"
                "IgnoreProjector" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            Blend One OneMinusSrcAlpha

            CGPROGRAM
            #pragma surface surf Lambert vertex:vert nolightmap nodynlightmap keepalpha noinstancing finalcolor:applyFog
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma multi_compile_fog

            sampler2D _MainTex;
            struct Input {
                float2 uv_MainTex;
                float fogCoord;
            };
            fixed4 _RendererColor;

            void vert(inout appdata_full v, out Input o)
            {
                UNITY_INITIALIZE_OUTPUT(Input, o);

                // apply object scale
                v.vertex.xy *= float2(length(unity_ObjectToWorld._m00_m10_m20), length(unity_ObjectToWorld._m01_m11_m21));

                // get the camera basis vectors
                float3 forward = -normalize(UNITY_MATRIX_V._m20_m21_m22);
                //float3 up = normalize(UNITY_MATRIX_V._m10_m11_m12);
                float3 right = normalize(UNITY_MATRIX_V._m00_m01_m02);

                // rotate to face camera
                float4x4 rotationMatrix = float4x4(right, 0,
                    0, 1, 0, 0,
                    forward, 0,
                    0, 0, 0, 1);
                v.vertex = mul(v.vertex, rotationMatrix);
                v.normal = mul(v.normal, rotationMatrix);

                // undo object to world transform surface shader will apply
                v.vertex.xyz = mul((float3x3)unity_WorldToObject, v.vertex.xyz);
                v.normal = mul(v.normal, (float3x3)unity_ObjectToWorld);
                UNITY_TRANSFER_FOG(o, UnityObjectToClipPos(v.vertex));
            }

            void surf(Input IN, inout SurfaceOutput o) {
                // Albedo comes from a texture tinted by color
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _RendererColor;
                o.Albedo = c.rgb * c.a;
                o.Alpha = c.a;
            }

            void applyFog (Input IN, SurfaceOutput o, inout fixed4 color)
		    {
			    // apply fog
			    UNITY_APPLY_FOG(IN.fogCoord, color.rgb);
			    color.rgb *= o.Alpha;
		    }

            ENDCG
        }
            FallBack "Transparent/VertexLit"
}