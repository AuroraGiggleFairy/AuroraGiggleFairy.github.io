using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxBool : XUiC_ComboBox<bool>
{
	[XuiXmlAttribute("localization_prefix", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string LocalizationPrefix { get; set; }

	public override bool Value
	{
		get
		{
			return currentValue;
		}
		set
		{
			if (currentValue != value)
			{
				currentValue = value;
				IsDirty = true;
				UpdateLabel();
			}
		}
	}

	public override double RelativeValue
	{
		get
		{
			return Value ? 1 : 0;
		}
		set
		{
			if (UsesIndexMarkers)
			{
				Value = value >= 0.5;
			}
		}
	}

	public override long ValueGeneric
	{
		get
		{
			return Value ? 1 : 0;
		}
		set
		{
			Value = value != 0;
		}
	}

	public override long ValueMinGeneric
	{
		get
		{
			return 0L;
		}
		set
		{
		}
	}

	public override long ValueMaxGeneric
	{
		get
		{
			return 1L;
		}
		set
		{
		}
	}

	public override int IndexElementCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return 2;
		}
	}

	public override int IndexMarkerIndex
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!currentValue)
			{
				return 0;
			}
			return 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateLabel()
	{
		base.ValueText = ((!string.IsNullOrEmpty(LocalizationPrefix)) ? Localization.Get(LocalizationPrefix + (currentValue ? "On" : "Off")) : (currentValue ? "Yes" : "No"));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDifferentValue(bool _oldVal, bool _currentValue)
	{
		return _oldVal != _currentValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		currentValue = !currentValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		currentValue = !currentValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEmpty()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		TryPageUp();
	}
}
