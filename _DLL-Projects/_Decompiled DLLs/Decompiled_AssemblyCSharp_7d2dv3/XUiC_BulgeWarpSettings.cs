using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BulgeWarpSettings : XUiC_SignWarpSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.BulgeWarp currentWarp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxAmount;

	public override SignData.SignWarp CurrentWarp
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return currentWarp;
		}
	}

	public override void Init()
	{
		base.Init();
		cbxOffsetX = (XUiC_ComboBoxFloat)GetChildById("cbxOffsetX").GetChildById("value");
		cbxOffsetX.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Bulge Offset X");
		};
		cbxOffsetY = (XUiC_ComboBoxFloat)GetChildById("cbxOffsetY").GetChildById("value");
		cbxOffsetY.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Bulge Offset Y");
		};
		cbxAmount = (XUiC_ComboBoxFloat)GetChildById("cbxAmount").GetChildById("value");
		cbxAmount.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Bulge Amount");
		};
		SignData.BulgeWarp bulgeWarp = new SignData.BulgeWarp();
		SetDefaultValue("cbxOffsetX", bulgeWarp.offset.x);
		SetDefaultValue("cbxOffsetY", bulgeWarp.offset.y);
		SetDefaultValue("cbxAmount", bulgeWarp.amount);
	}

	public override void SetWarp(SignData.SignWarp warp)
	{
		currentWarp = warp as SignData.BulgeWarp;
		if (currentWarp == null)
		{
			cbxOffsetX.Value = 0.0;
			cbxOffsetY.Value = 0.0;
			cbxAmount.Value = 0.0;
		}
		else
		{
			cbxOffsetX.Value = currentWarp.offset.x;
			cbxOffsetY.Value = currentWarp.offset.y;
			cbxAmount.Value = currentWarp.amount;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (currentWarp != null)
		{
			OnPreLayerSettingsChanged?.Invoke(_changeDescription, arg2: false);
			currentWarp.offset = new Vector2((float)cbxOffsetX.Value, (float)cbxOffsetY.Value);
			currentWarp.amount = (float)cbxAmount.Value;
			OnLayerSettingsChanged?.Invoke();
		}
	}
}
