using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_ComboBox<TValue> : XUiC_ComboBoxBase
{
	public delegate void XUiEvent_ValueChanged(XUiController _sender, TValue _oldValue, TValue _newValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public TValue currentValue;

	public abstract TValue Value { get; set; }

	public event XUiEvent_ValueChanged OnValueChanged;

	public void TriggerValueChangedEvent(TValue _oldVal)
	{
		UpdateIndexMarkerStates();
		invokeValueChangedGeneric();
		this.OnValueChanged?.Invoke(this, _oldVal, currentValue);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryPageUp()
	{
		bool flag = false;
		if (base.Enabled)
		{
			TValue oldVal = currentValue;
			ForwardPressed();
			UpdateLabel();
			flag = isDifferentValue(oldVal, currentValue);
			if (flag)
			{
				TriggerValueChangedEvent(oldVal);
			}
			IsDirty = true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryPageDown()
	{
		bool flag = false;
		if (base.Enabled)
		{
			TValue oldVal = currentValue;
			BackPressed();
			UpdateLabel();
			flag = isDifferentValue(oldVal, currentValue);
			if (flag)
			{
				TriggerValueChangedEvent(oldVal);
			}
			IsDirty = true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setRelativeValue(float _value)
	{
		if (base.Enabled)
		{
			TValue oldVal = currentValue;
			RelativeValue = _value;
			UpdateLabel();
			if (isDifferentValue(oldVal, currentValue))
			{
				TriggerValueChangedEvent(oldVal);
			}
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isDifferentValue(TValue _oldVal, TValue _currentValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ComboBox()
	{
	}
}
