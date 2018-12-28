// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/MaskedTexture"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_MaskTex("Mask Texture", 2D) = "white" {}
		_SkinColor("Skin", Color) = (1,1,1,1)
		_TshirtColor("Tshirt", Color) = (1,1,1,1)
		_PantsColor("Pants", Color) = (0.3245,0.3348,0.3407,1)
		_ShoesColor("Shoes", Color) = (0.7421875,0.796875,0.81640625,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
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

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
				};

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);

	#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
	#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					return color;
				}

				sampler2D _MaskTex;
		
				fixed4 SampleMaskTexture(float2 uv)
				{
					fixed4 color = tex2D(_MaskTex, uv);
#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					return color;
				}

				fixed4 _TshirtColor;
				fixed4 _PantsColor;
				fixed4 _ShoesColor;
				fixed4 _SkinColor;

				fixed4 frag(v2f IN) : SV_Target
				{
					float4 text = SampleSpriteTexture(IN.texcoord);
					float4 color = text;
					float4 mask = SampleMaskTexture(IN.texcoord.xy);

					color.rgb = _SkinColor * text.rgb;

					color.rgb = lerp(color.rgb, _TshirtColor * text.rgb, mask.r * mask.a);
					color.rgb = lerp(color.rgb, _PantsColor * text.rgb, mask.g * mask.a);
					color.rgb = lerp(color.rgb, _ShoesColor * text.rgb, mask.b * mask.a);
					
					color.rgb *= color.a;
					return color;
				}
			ENDCG
			}
		}
}