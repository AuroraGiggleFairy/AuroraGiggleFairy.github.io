using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_AdvancedColorPicker : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static int _Dimensions = Shader.PropertyToID("_Dimensions");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _Saturation = Shader.PropertyToID("_Saturation");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _Value = Shader.PropertyToID("_Value");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _Hue = Shader.PropertyToID("_Hue");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _Color = Shader.PropertyToID("_Color");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _CursorSize = Shader.PropertyToID("_CursorSize");

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color? copyBuffer;

	[XuiBindComponent("svPicker", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Texture svPicker;

	[XuiBindComponent("hPicker", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Texture hPicker;

	[XuiBindComponent("aPicker", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Texture aPicker;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect svPickerRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect hPickerRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect aPickerRect;

	[XuiBindComponent("selectedColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite selectedColorSprite;

	[XuiBindComponent("samplerColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite samplerColorSprite;

	[XuiBindComponent("txtColorR", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtColorR;

	[XuiBindComponent("txtColorG", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtColorG;

	[XuiBindComponent("txtColorB", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtColorB;

	[XuiBindComponent("txtColorA", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtColorA;

	[XuiBindComponent("txtHex", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtHex;

	[XuiBindComponent("btnCopy", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SimpleButton btnCopy;

	[XuiBindComponent("btnPaste", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SimpleButton btnPaste;

	[XuiBindComponent("btnSampler", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController btnSampler;

	[XuiBindComponent("btnSamplerOverlay", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController btnSamplerOverlay;

	public Action<Color> OnColorChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public float hue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float saturation = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float value = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float alpha = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cursorRadius = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool editAlpha = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pickersDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bindingsDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool samplerActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color samplerColor = Color.clear;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Texture2D samplerTexture = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hoverOverSampler;

	[XuiXmlAttribute("cursor_radius", false)]
	public float CursorRadius
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cursorRadius;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (!Mathf.Approximately(cursorRadius, value))
			{
				cursorRadius = value;
			}
		}
	}

	[XuiXmlAttribute("edit_alpha", false)]
	[XuiXmlBinding("show_alpha")]
	public bool EditAlpha
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return editAlpha;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (editAlpha != value)
			{
				editAlpha = value;
				bindingsDirty = true;
			}
		}
	}

	[XuiXmlBinding("has_copy")]
	public bool HasCopyBuffer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return copyBuffer.HasValue;
		}
	}

	[XuiXmlBinding("sampler_active")]
	public bool SamplerActive
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return samplerActive;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (samplerActive != value)
			{
				samplerActive = value;
				bindingsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		svPicker.CreateMaterial("Game/UI/ColorPickerSV");
		hPicker.CreateMaterial("Game/UI/ColorPickerH");
		aPicker.CreateMaterial("Game/UI/ColorPickerA");
		XUiV_Texture xUiV_Texture = svPicker;
		xUiV_Texture.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xUiV_Texture.OnRenderTexture, new UIDrawCall.OnRenderCallback(svPicker_OnRender));
		XUiV_Texture xUiV_Texture2 = hPicker;
		xUiV_Texture2.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xUiV_Texture2.OnRenderTexture, new UIDrawCall.OnRenderCallback(hPicker_OnRender));
		XUiV_Texture xUiV_Texture3 = aPicker;
		xUiV_Texture3.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xUiV_Texture3.OnRenderTexture, new UIDrawCall.OnRenderCallback(aPicker_OnRender));
		svPicker.Texture = Texture2D.whiteTexture;
		hPicker.Texture = Texture2D.whiteTexture;
		aPicker.Texture = Texture2D.whiteTexture;
		SetupPickerEvents(svPicker.Controller, svPicker_MouseUpdate);
		SetupPickerEvents(hPicker.Controller, hPicker_MouseUpdate);
		SetupPickerEvents(aPicker.Controller, aPicker_MouseUpdate);
		samplerColorSprite.Controller.OnHover += SamplerColor_OnHover;
		SetColor(Color.cyan);
		[PublicizedFrom(EAccessModifier.Internal)]
		static void SetupPickerEvents(XUiController _ctrl, Action _update)
		{
			_ctrl.OnPress += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, int _) =>
			{
				_update();
			};
			_ctrl.OnDrag += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, EDragType _, Vector2 _) =>
			{
				_update();
			};
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		bindingsDirty = true;
		pickersDirty = true;
		hoverOverSampler = false;
		if (!EditAlpha)
		{
			alpha = 1f;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (bindingsDirty)
		{
			RefreshBindings();
			bindingsDirty = false;
		}
		if (pickersDirty)
		{
			updatePickerRects();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePickerRects()
	{
		svPickerRect = svPicker.GetXUiRect();
		hPickerRect = hPicker.GetXUiRect();
		aPickerRect = aPicker.GetXUiRect();
		pickersDirty = false;
	}

	public void SetColor(Color _color, bool _notifyListeners = false)
	{
		selectedColorSprite.Color = _color;
		updateHsva(_color);
		updateRgba(_color);
		updateHex(_color);
		if (_notifyListeners)
		{
			OnColorChanged?.Invoke(_color);
		}
	}

	[XuiBindEvent("OnPressed", "btnCopy")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCopy_OnPressed(XUiController _sender, int _mouseButton)
	{
		copyBuffer = getCurrentColor();
		RefreshBindings();
	}

	[XuiBindEvent("OnPressed", "btnPaste")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPaste_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (copyBuffer.HasValue)
		{
			SetColor(copyBuffer.Value, _notifyListeners: true);
		}
	}

	[XuiBindEvent("OnPress", "btnSampler")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSampler_OnPressed(XUiController _sender, int _mouseButton)
	{
		samplerActive = true;
		GameManager.Instance.StartCoroutine(samplerCoroutine());
		bindingsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator samplerCoroutine()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		samplerColorSprite.Color = selectedColorSprite.Color;
		while (samplerActive)
		{
			yield return new WaitForEndOfFrame();
			if (!hoverOverSampler && Input.mousePosition.x >= 0f && Input.mousePosition.y >= 0f && Input.mousePosition.x < (float)Screen.width && Input.mousePosition.y < (float)Screen.height)
			{
				samplerTexture.ReadPixels(new Rect(Input.mousePosition, Vector2.one), 0, 0);
				samplerTexture.Apply();
				samplerColor = samplerTexture.GetPixel(0, 0);
				samplerColorSprite.Color = samplerColor;
			}
		}
	}

	[XuiBindEvent("OnPress", "btnSamplerOverlay")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSamplerOverlay_OnPressed(XUiController _sender, int _mouseButton)
	{
		SetColor(samplerColor, _notifyListeners: true);
		samplerActive = false;
		bindingsDirty = true;
	}

	[XuiBindEvent("OnRightPress", "btnSamplerOverlay")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSamplerOverlay_OnRightPressed(XUiController _sender, int _mouseButton)
	{
		samplerActive = false;
		bindingsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SamplerColor_OnHover(XUiController _sender, bool _isOver)
	{
		hoverOverSampler = _isOver;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void aPicker_MouseUpdate()
	{
		alpha = Mathf.Clamp01(getRelativeMousePos(aPickerRect).x);
		OnHSVAChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void hPicker_MouseUpdate()
	{
		hue = Mathf.Clamp01(getRelativeMousePos(hPickerRect).x);
		OnHSVAChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void svPicker_MouseUpdate()
	{
		Vector2 relativeMousePos = getRelativeMousePos(svPickerRect);
		saturation = Mathf.Clamp01(relativeMousePos.x);
		value = Mathf.Clamp01(relativeMousePos.y);
		OnHSVAChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 getRelativeMousePos(Rect _controllerXUiRect)
	{
		return (xui.GetMouseXUiPosition().AsVector2() - _controllerXUiRect.min) / _controllerXUiRect.size;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void aPicker_OnRender(Material _mat)
	{
		_mat.SetVector(_Dimensions, aPicker.Size.AsVector2());
		Color currentColor = getCurrentColor();
		_mat.SetColor(_Color, currentColor);
		_mat.SetFloat(_CursorSize, CursorRadius);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void hPicker_OnRender(Material _mat)
	{
		_mat.SetVector(_Dimensions, hPicker.Size.AsVector2());
		_mat.SetFloat(_Hue, hue);
		_mat.SetFloat(_CursorSize, CursorRadius);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void svPicker_OnRender(Material _mat)
	{
		_mat.SetVector(_Dimensions, svPicker.Size.AsVector2());
		_mat.SetFloat(_Hue, hue);
		_mat.SetFloat(_Saturation, saturation);
		_mat.SetFloat(_Value, value);
		_mat.SetFloat(_CursorSize, CursorRadius);
	}

	[XuiBindEvent("OnChangeHandler", "txtHex")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHexChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_text.Length == 6 || _text.Length == 8)
		{
			if (!ColorUtility.TryParseHtmlString("#" + _text, out var color))
			{
				Log.Error("Failed to parse hex string `" + _text + "`.");
			}
			else if (!_changeFromCode)
			{
				selectedColorSprite.Color = color;
				updateHsva(color);
				updateRgba(color);
				OnColorChanged?.Invoke(color);
			}
		}
	}

	[XuiBindEvent("OnChangeHandler", "txtColorR")]
	[XuiBindEvent("OnChangeHandler", "txtColorG")]
	[XuiBindEvent("OnChangeHandler", "txtColorB")]
	[XuiBindEvent("OnChangeHandler", "txtColorA")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRGBAChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!int.TryParse(_text, out var result))
		{
			return;
		}
		result = Mathf.Clamp(result, 0, 255);
		if (!_text.Equals(result.ToString()))
		{
			((XUiC_TextInput)_sender).Text = result.ToString();
		}
		if (!_changeFromCode && byte.TryParse(txtColorR.Text, out var result2) && byte.TryParse(txtColorG.Text, out var result3) && byte.TryParse(txtColorB.Text, out var result4) && byte.TryParse(txtColorA.Text, out var result5))
		{
			Color color = new Color32(result2, result3, result4, result5);
			if (!EditAlpha)
			{
				color.a = 1f;
			}
			selectedColorSprite.Color = color;
			updateHsva(color);
			updateHex(color);
			OnColorChanged?.Invoke(color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHSVAChanged()
	{
		Color currentColor = getCurrentColor();
		selectedColorSprite.Color = currentColor;
		updateRgba(currentColor);
		updateHex(currentColor);
		OnColorChanged?.Invoke(currentColor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color getCurrentColor()
	{
		Color result = Color.HSVToRGB(hue, saturation, value);
		result.a = alpha;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateHsva(Color _color)
	{
		Color.RGBToHSV(_color, out hue, out saturation, out value);
		alpha = _color.a;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRgba(Color _color)
	{
		Color32 color = _color;
		txtColorR.Text = color.r.ToString();
		txtColorG.Text = color.g.ToString();
		txtColorB.Text = color.b.ToString();
		txtColorA.Text = color.a.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateHex(Color _color)
	{
		txtHex.Text = (EditAlpha ? ColorUtility.ToHtmlStringRGBA(_color) : ColorUtility.ToHtmlStringRGB(_color));
	}

	[Conditional("DEBUG_LOG_PICKERS")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void debugLog(string _message)
	{
		UnityEngine.Debug.Log("[AdvancedColorPicker] " + _message);
	}
}
