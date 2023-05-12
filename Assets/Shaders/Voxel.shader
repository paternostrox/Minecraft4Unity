﻿Shader "Custom/Voxel"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _AOColor ("AO Color", Color) = (0,0,0,1)
        _AOIntensity ("AO Intensity", Range(0, 1)) = 1.0
		_AOPower ("AO Power", Range(1, 10)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows 
        #pragma vertex vert        
        
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float3 position;
            float4 custom_uv;
            float4 color : COLOR;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        
        int _AtlasX;
        int _AtlasY;
        fixed4 _AtlasRec;
        
        half4 _AOColor;
		float _AOIntensity;
		float _AOPower;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.custom_uv = v.texcoord;
            o.position = v.vertex;
            
            
            v.color.rgb = _AOColor;
            v.color.a   = pow((1-v.color.a) * _AOIntensity, _AOPower );
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {                 
            fixed2 atlasOffset = IN.custom_uv.zw;
            fixed2 scaledUV = IN.custom_uv.xy;
            fixed2 atlasUV = scaledUV;
            atlasUV.x = (atlasOffset.x * _AtlasRec.x) + frac(atlasUV.x) * _AtlasRec.x;
            atlasUV.y = (((_AtlasY - 1) - atlasOffset.y) * _AtlasRec.y) + frac(atlasUV.y) * _AtlasRec.y;
                
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2Dgrad(_MainTex, atlasUV, ddx(atlasUV * _AtlasRec), ddy(atlasUV * _AtlasRec)) * _Color;
            //fixed4 c = tex2D(_MainTex, atlasUV) * _Color;
            o.Albedo = lerp(c.rgb, IN.color.rgb, IN.color.a);
			o.Alpha  = c.a;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }

        
        ENDCG
    }
    FallBack "Diffuse"
}
