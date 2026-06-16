using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwirlWarpSettings : XUiC_SignWarpSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.TwirlWarp currentWarp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxFreq;

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
			OnValueChangedGeneric("Changed Twirl Offset X");
		};
		cbxOffsetY = (XUiC_ComboBoxFloat)GetChildById("cbxOffsetY").GetChildById("value");
		cbxOffsetY.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Twirl Offset Y");
		};
		cbxAmount = (XUiC_ComboBoxFloat)GetChildById("cbxAmount").GetChildById("value");
		cbxAmount.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Twirl Amount");
		};
		cbxFreq = (XUiC_ComboBoxFloat)GetChildById("cbxFreq").GetChildById("value");
		cbxFreq.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Twirl Frequency");
		};
		SignData.TwirlWarp twirlWarp = new SignData.TwirlWarp();
		SetDefaultValue("cbxOffsetX", twirlWarp.offset.x);
		SetDefaultValue("cbxOffsetY", twirlWarp.offset.y);
		SetDefaultValue("cbxAmount", twirlWarp.amount);
		SetDefaultValue("cbxFreq", twirlWarp.frequency);
	}

	public override void SetWarp(SignData.SignWarp warp)
	{
		currentWarp = warp as SignData.TwirlWarp;
		if (currentWarp == null)
		{
			cbxOffsetX.Value = 0.0;
			cbxOffsetY.Value = 0.0;
			cbxAmount.Value = 0.0;
			cbxFreq.Value = 0.0;
		}
		else
		{
			cbxOffsetX.Value = currentWarp.offset.x;
			cbxOffsetY.Value = currentWarp.offset.y;
			cbxAmount.Value = currentWarp.amount;
			cbxFreq.Value = currentWarp.frequency;
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
			currentWarp.frequency = (float)cbxFreq.Value;
			OnLayerSettingsChanged?.Invoke();
		}
	}
}
