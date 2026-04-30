using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Slider : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SliderThumb thumbController;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SliderBar barController;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string name;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float val;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Func<float, string> valueFormatter;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool initialized;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float left = float.NaN;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float width;

	public string Tag;

	public float Step = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat internalValueFormatter = new CachedStringFormatterFloat("0.00");

	public Func<float, string> ValueFormatter
	{
		get
		{
			return valueFormatter;
		}
		set
		{
			valueFormatter = value;
			IsDirty = true;
		}
	}

	public string Label
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
			IsDirty = true;
		}
	}

	public float Value
	{
		get
		{
			return val;
		}
		set
		{
			if (!thumbController.IsDragging && value != val)
			{
				val = Mathf.Clamp01(value);
				updateThumb();
				IsDirty = true;
			}
		}
	}

	public event XUiEvent_SliderValueChanged OnValueChanged;

	public override void Init()
	{
		base.Init();
		thumbController = GetChildById("thumb") as XUiC_SliderThumb;
		if (thumbController == null)
		{
			Log.Error("Thumb slider not found!");
			return;
		}
		thumbController.ViewComponent.IsNavigatable = (thumbController.ViewComponent.IsSnappable = false);
		barController = GetChildById("bar") as XUiC_SliderBar;
		if (barController == null)
		{
			Log.Error("Thumb bar not found!");
			return;
		}
		left = barController.ViewComponent.Position.x;
		width = barController.ViewComponent.Size.x;
		thumbController.SetDimensions(left, width);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (thumbController != null && thumbController.ViewComponent != null && !float.IsNaN(left))
		{
			if (IsDirty)
			{
				RefreshBindings();
				IsDirty = false;
			}
			if (base.xui.playerUI.CursorController.navigationTarget == barController.ViewComponent)
			{
				XUi.HandlePaging(base.xui, barController.PageUpAction, barController.PageDownAction);
			}
		}
	}

	public void Reset()
	{
		initialized = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (!(bindingName == "name"))
		{
			if (bindingName == "value")
			{
				if (valueFormatter != null)
				{
					value = valueFormatter(val);
				}
				else
				{
					value = internalValueFormatter.Format(val);
				}
				return true;
			}
			return false;
		}
		value = name;
		return true;
	}

	public void ValueChanged(float _newVal)
	{
		val = Mathf.Clamp01(_newVal);
		if (this.OnValueChanged != null)
		{
			this.OnValueChanged(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void updateThumb()
	{
		thumbController.ThumbPosition = val;
	}
}
