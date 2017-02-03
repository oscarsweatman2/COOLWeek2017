Shader "Custom/VoxelTerrainShader" 
{
	Properties 
	{
		[NoScaleOffset] _MainTex ("Tileset", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "Lighting.cginc"

			// compile shader into multiple variants, with and without shadows
			// (we don't care about any lightmaps yet, so skip these variants)
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			// shadow helper functions and macros
			#include "AutoLight.cginc"

			sampler2D _MainTex;

			// The vertex data passed from the application
			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float3 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			// The data passed from the vertex shader to the fragment shader
			struct v2f 
			{
				float4 pos : SV_POSITION;
				fixed3 diffuse : COLOR0;
				fixed3 ambient : COLOR1;
				float2 uv : TEXCOORD0;
				SHADOW_COORDS(1)
			};

			// The vertex shader
			v2f vert(appdata v)
			{
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0.4, dot(worldNormal, _WorldSpaceLightPos0.xyz));

				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				o.diffuse = v.color * nl * _LightColor0.rgb;
				o.ambient = ShadeSH9(half4(worldNormal, 1));

				TRANSFER_SHADOW(o);

				return o;
			}

			// The fragment shader
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, i.uv);
				fixed shadow = max(0, SHADOW_ATTENUATION(i));

				fixed4 outColor = fixed4(1, 1, 1, 1);

				outColor.rgb = texColor.rgb * i.diffuse * (i.ambient + shadow);

				return outColor;
			}

			ENDCG
		}

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}
