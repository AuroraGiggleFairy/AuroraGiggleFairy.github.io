using UnityEngine;

public class XUiV_TextureBased : XUiV_ImageBased
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public UITexture uiTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture texture;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material material;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect uvRect = new Rect(0f, 0f, 1f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector4 border = Vector4.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color color = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIBasicSprite.FillDirection fillDirection;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCreatedMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sourceAspectRatioRespectPivot;

	public virtual Texture Texture
	{
		get
		{
			return texture;
		}
		set
		{
			texture = value;
			SetDirty();
		}
	}

	public Material Material
	{
		get
		{
			return material;
		}
		set
		{
			material = value;
			SetDirty();
		}
	}

	public UIDrawCall.OnRenderCallback OnRenderTexture
	{
		get
		{
			return uiTexture.onRender;
		}
		set
		{
			uiTexture.onRender = value;
		}
	}

	public Rect UVRect
	{
		get
		{
			return uvRect;
		}
		set
		{
			uvRect = value;
			SetDirty();
		}
	}

	public Vector4 Border
	{
		get
		{
			return border;
		}
		set
		{
			border = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("color", false)]
	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
			SetDirty();
		}
	}

	public UIBasicSprite.FillDirection FillDirection
	{
		get
		{
			return fillDirection;
		}
		set
		{
			fillDirection = value;
			SetDirty();
		}
	}

	[XuiXmlAttribute("sourceaspectratiorespectpivot", false)]
	public bool SourceAspectRatioRespectPivot
	{
		get
		{
			return sourceAspectRatioRespectPivot;
		}
		set
		{
			if (sourceAspectRatioRespectPivot != value)
			{
				sourceAspectRatioRespectPivot = value;
				SetDirty();
			}
		}
	}

	public XUiV_TextureBased(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		base.createComponents(_go);
		_go.AddComponent<UITexture>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		widget = (uiTexture = uiTransform.gameObject.GetComponent<UITexture>());
	}

	public void CreateMaterial(string _shaderName = "Unlit/Transparent Colored Emissive TextureArray")
	{
		Shader shader = GlobalAssets.FindShader(_shaderName);
		if (shader == null)
		{
			Log.Error("Could not find shader " + _shaderName);
			return;
		}
		Material = new Material(shader);
		isCreatedMaterial = true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (isCreatedMaterial)
		{
			Object.Destroy(material);
			material = null;
			isCreatedMaterial = false;
		}
	}

	public override void InitView()
	{
		base.InitView();
		updateData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		uiTexture.enabled = texture != null;
		uiTexture.mainTexture = texture;
		uiTexture.color = opacityModColor(color);
		uiTexture.keepAspectRatio = keepAspectRatio;
		uiTexture.aspectRatio = aspectRatio;
		uiTexture.fixedAspect = keepSourceAspectRatio;
		uiTexture.fixedAspectRespectPivot = sourceAspectRatioRespectPivot;
		uiTexture.SetDimensions(size.x, size.y);
		uiTexture.type = type;
		uiTexture.border = border;
		uiTexture.uvRect = uvRect;
		uiTexture.flip = flip;
		uiTexture.centerType = (fillCenter ? UIBasicSprite.AdvancedType.Sliced : UIBasicSprite.AdvancedType.Invisible);
		uiTexture.fillDirection = fillDirection;
		uiTexture.material = material;
		base.updateData();
	}

	public void SetTextureDirty()
	{
		uiTexture.mainTexture = null;
		SetDirty();
	}

	public virtual void UnloadTexture()
	{
		if (!(Texture == null))
		{
			uiTexture.mainTexture = null;
			texture = null;
			SetDirty();
		}
	}

	[XuiXmlAttribute("material", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeMaterial(string _value)
	{
		xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (Material _o) =>
		{
			material = new Material(_o);
		});
	}

	[XuiXmlAttribute("rect_offset", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeRectOffset(Vector2 _value)
	{
		Rect uVRect = UVRect;
		uVRect.x = _value.x;
		uVRect.y = _value.y;
		UVRect = uVRect;
	}

	[XuiXmlAttribute("rect_size", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeRectSize(Vector2 _value)
	{
		Rect uVRect = UVRect;
		uVRect.width = _value.x;
		uVRect.height = _value.y;
		UVRect = uVRect;
	}
}
