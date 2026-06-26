using System.Collections;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ColorPicker : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture svPicker;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture hPicker;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color selectedColor = Color.red;

	[PublicizedFrom(EAccessModifier.Private)]
	public float saturation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float vibrance;

	[PublicizedFrom(EAccessModifier.Private)]
	public float hue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocalDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite selectedColorSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite selectedColorSVPointerSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingCallouts;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool calloutStateHeld;

	public Color SelectedColor
	{
		get
		{
			return selectedColor;
		}
		set
		{
			selectedColor = value;
			hue = (float)HSVUtil.ConvertRgbToHsv(selectedColor).H;
			saturation = (float)HSVUtil.ConvertRgbToHsv(selectedColor).S;
			vibrance = (float)HSVUtil.ConvertRgbToHsv(selectedColor).V;
			SetupSaturationVibranceTexture();
			float num = saturation * (float)svPicker.Size.x;
			float num2 = vibrance * (float)svPicker.Size.y;
			selectedColorSVPointerSprite.UiTransform.gameObject.SetActive(value: true);
			selectedColorSVPointerSprite.Position = new Vector2i((int)num, -(int)num2);
			selectedColorSVPointerSprite.UiTransform.localPosition = new Vector3(num, 0f - num2);
			selectedColorSprite.Color = selectedColor;
		}
	}

	public event XUiEvent_SelectedColorChanged OnSelectedColorChanged;

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("svPicker");
		XUiController childById2 = GetChildById("hPicker");
		XUiController childById3 = GetChildById("selectedColorSVPointer");
		if (childById != null && childById.ViewComponent is XUiV_Texture)
		{
			svPicker = childById.ViewComponent as XUiV_Texture;
			childById.OnPress += SvPickerC_OnPress;
			childById.OnDrag += SvPickerC_OnDrag;
		}
		if (childById2 != null && childById2.ViewComponent is XUiV_Texture)
		{
			hPicker = childById2.ViewComponent as XUiV_Texture;
			childById2.OnPress += HPickerC_OnPress;
			childById2.OnDrag += HPickerC_OnDrag;
		}
		XUiController childById4 = GetChildById("selectedColor");
		if (childById4 != null && childById4.ViewComponent is XUiV_Sprite)
		{
			selectedColorSprite = childById4.ViewComponent as XUiV_Sprite;
		}
		if (childById3 != null && childById3.ViewComponent is XUiV_Sprite)
		{
			selectedColorSVPointerSprite = childById3.ViewComponent as XUiV_Sprite;
		}
		SetupHueTexture();
		SetupSaturationVibranceTexture();
	}

	public override void OnClose()
	{
		base.OnClose();
		HideCallouts();
		base.xui.playerUI.CursorController.Locked = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.playerUI.CursorController.navigationTarget == svPicker && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			if (base.xui.playerUI.playerInput.GUIActions.Submit.IsPressed)
			{
				ShowCallouts(_held: true);
				base.xui.playerUI.CursorController.Locked = true;
				Vector2 value = base.xui.playerUI.playerInput.GUIActions.Nav.Value;
				if (value != Vector2.zero)
				{
					vibrance = Mathf.Clamp01(vibrance - value.y * 0.05f);
					saturation = Mathf.Clamp01(saturation + value.x * 0.05f);
					isLocalDirty = true;
				}
				float num = hue;
				hue = Mathf.Clamp(hue + base.xui.playerUI.playerInput.GUIActions.PageUp.Value, 0f, 255f);
				hue = Mathf.Clamp(hue - base.xui.playerUI.playerInput.GUIActions.PageDown.Value, 0f, 255f);
				if (hue != num)
				{
					isLocalDirty = true;
				}
			}
			else
			{
				ShowCallouts(_held: false);
				base.xui.playerUI.CursorController.Locked = false;
			}
		}
		else
		{
			HideCallouts();
			base.xui.playerUI.CursorController.Locked = false;
		}
		if (isLocalDirty)
		{
			isLocalDirty = false;
			selectedColor = HSVUtil.ConvertHsvToRgb(hue, saturation, vibrance, 1f);
			selectedColorSprite.Color = selectedColor;
			if (this.OnSelectedColorChanged != null)
			{
				this.OnSelectedColorChanged(selectedColor);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowCallouts(bool _held)
	{
		if (!showingCallouts || calloutStateHeld != _held)
		{
			base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
			if (_held)
			{
				base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftStick, "igcoColorPickerSaturationVibrance", XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
				base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoColorPickerHueMinus", XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
				base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoColorPickerHuePlus", XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
			}
			else
			{
				base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelectColorPicker", XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
			}
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
			calloutStateHeld = _held;
			showingCallouts = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HideCallouts()
	{
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
		showingCallouts = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setSV()
	{
		Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		selectedColorSVPointerSprite.UiTransform.gameObject.SetActive(value: false);
		yield return new WaitForEndOfFrame();
		Color colorUnderMouse = getColorUnderMouse(tex);
		saturation = (float)HSVUtil.ConvertRgbToHsv(colorUnderMouse).S;
		vibrance = (float)HSVUtil.ConvertRgbToHsv(colorUnderMouse).V;
		float num = saturation * (float)svPicker.Size.x;
		float num2 = vibrance * (float)svPicker.Size.y;
		selectedColorSVPointerSprite.UiTransform.gameObject.SetActive(value: true);
		selectedColorSVPointerSprite.Position = new Vector2i((int)num, -(int)num2);
		selectedColorSVPointerSprite.UiTransform.localPosition = new Vector3(num, 0f - num2);
		isLocalDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setHue()
	{
		Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		yield return new WaitForEndOfFrame();
		Color colorUnderMouse = getColorUnderMouse(tex);
		hue = (float)HSVUtil.ConvertRgbToHsv(colorUnderMouse).H;
		SetupSaturationVibranceTexture();
		isLocalDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color getColorUnderMouse(Texture2D _tex)
	{
		_tex.ReadPixels(new Rect(Input.mousePosition, Vector2.one), 0, 0);
		_tex.Apply();
		return _tex.GetPixel(0, 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupSaturationVibranceTexture()
	{
		Texture2D texture2D = new Texture2D(svPicker.Size.x, svPicker.Size.y);
		for (int i = 0; i < svPicker.Size.y; i++)
		{
			float num = 1f - (float)(i + 1) / (float)svPicker.Size.y;
			for (int j = 0; j < svPicker.Size.x; j++)
			{
				float num2 = (float)(j + 1) / (float)svPicker.Size.x;
				texture2D.SetPixel(j, i, HSVUtil.ConvertHsvToRgb(hue, num2, num, 1f));
			}
		}
		texture2D.Apply();
		svPicker.Texture = texture2D;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupHueTexture()
	{
		Texture2D texture2D = new Texture2D(hPicker.Size.x, hPicker.Size.y);
		for (int i = 0; i < hPicker.Size.y; i++)
		{
			float num = (float)i / ((float)hPicker.Size.y - 1f);
			for (int j = 0; j < hPicker.Size.x; j++)
			{
				texture2D.SetPixel(j, i, HSVUtil.ConvertHsvToRgb(num * 360f, 1.0, 1.0, 1f));
			}
		}
		texture2D.Apply();
		hPicker.Texture = texture2D;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SvPickerC_OnDrag(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		GameManager.Instance.StartCoroutine(setSV());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SvPickerC_OnPress(XUiController _sender, int _mouseButton)
	{
		GameManager.Instance.StartCoroutine(setSV());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HPickerC_OnDrag(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		GameManager.Instance.StartCoroutine(setHue());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HPickerC_OnPress(XUiController _sender, int _mouseButton)
	{
		GameManager.Instance.StartCoroutine(setHue());
	}
}
