using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_UiLimitsWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float availableXuiHeight = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string bindingWidthPrefix = "width_";

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		int manualHeight = UnityEngine.Object.FindObjectOfType<UIRoot>().manualHeight;
		float scale = base.xui.GetScale();
		availableXuiHeight = (float)manualHeight / scale;
		RefreshBindings(_forceAll: true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName.StartsWith("width_", StringComparison.Ordinal))
		{
			return handleArBinding(ref _value, _bindingName);
		}
		if (_bindingName == "height")
		{
			_value = Mathf.FloorToInt(availableXuiHeight).ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool handleArBinding(ref string _value, string _bindingName)
	{
		int num = _bindingName.IndexOf('_', "width_".Length);
		if (num < 0)
		{
			return false;
		}
		ReadOnlySpan<char> s = _bindingName.AsSpan("width_".Length, num - "width_".Length);
		ReadOnlySpan<char> s2 = _bindingName.AsSpan(num + 1);
		if (!int.TryParse(s, out var result))
		{
			return false;
		}
		if (!int.TryParse(s2, out var result2))
		{
			return false;
		}
		double uiSizeLimit = GameOptionsManager.GetUiSizeLimit((double)result / (double)result2);
		int num2 = Mathf.RoundToInt((float)((double)(availableXuiHeight / (float)result2 * (float)result) / uiSizeLimit));
		if (num2 % 2 > 0)
		{
			num2--;
		}
		_value = num2.ToString();
		return true;
	}
}
