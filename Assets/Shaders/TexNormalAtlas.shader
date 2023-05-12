Shader "Custom/TexNormalAtlas"
{
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
	}

	SubShader 
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf WrapLambert
		#pragma vertex vert 

		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		fixed4 _Color;

		half4 LightingWrapLambert(SurfaceOutput s, half3 lightDir, half atten)
		{
			half NdotL = dot(s.Normal, lightDir);
			half diff = NdotL * 0.5 + 0.5;
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten);
			c.a = s.Alpha;
			return c;
		}

		struct Input
		{
			float3 position;
			float4 custom_uv;
		};

		int _AtlasX;
		int _AtlasY;
		fixed4 _AtlasRec;

		void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.custom_uv = v.texcoord;
            o.position = v.vertex;
        }

		void surf(Input IN, inout SurfaceOutput o) {
			fixed2 atlasOffset = IN.custom_uv.zw;
			fixed2 scaledUV = IN.custom_uv.xy;
			fixed2 atlasUV = scaledUV;
			atlasUV.x = (atlasOffset.x * _AtlasRec.x) + frac(atlasUV.x) * _AtlasRec.x;
			atlasUV.y = (((_AtlasY - 1) - atlasOffset.y) * _AtlasRec.y) + frac(atlasUV.y) * _AtlasRec.y;

			fixed4 c = tex2D(_MainTex, atlasUV) * _Color;
			o.Normal = UnpackNormal(tex2D(_BumpMap, atlasUV));

			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
		Fallback "Diffuse"
}