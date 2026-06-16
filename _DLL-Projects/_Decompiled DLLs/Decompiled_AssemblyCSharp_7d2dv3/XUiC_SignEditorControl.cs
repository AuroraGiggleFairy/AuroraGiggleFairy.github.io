using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignEditorControl : XUiController
{
	public XUiV_Label label;

	public XUiController control;

	public XUiController controlB;

	[PublicizedFrom(EAccessModifier.Private)]
	public object _defaultValue;

	public object defaultValue
	{
		get
		{
			return _defaultValue;
		}
		set
		{
			label.EventOnPress = true;
			_defaultValue = value;
		}
	}

	public override void Init()
	{
		base.Init();
		label = GetChildById("label").ViewComponent as XUiV_Label;
		label.Controller.OnRightPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController sender, int button) =>
		{
			SetDefault();
		};
		label.EventOnPress = false;
		control = GetChildById("value");
		if (control == null)
		{
			control = GetChildById("valueA");
			controlB = GetChildById("valueB");
		}
	}

	public virtual void SetDefault()
	{
		if (defaultValue == null)
		{
			return;
		}
		XUiController xUiController = control;
		if (!(xUiController is XUiC_ComboBoxInt { Value: var value } xUiC_ComboBoxInt))
		{
			if (!(xUiController is XUiC_ComboBoxFloat { Value: var value2 } xUiC_ComboBoxFloat))
			{
				if (xUiController is XUiC_TextInput xUiC_TextInput)
				{
					if (controlB is XUiC_TextInput xUiC_TextInput2)
					{
						(float, float) tuple = ((float, float))defaultValue;
						xUiC_TextInput.Text = tuple.Item1.ToString();
						xUiC_TextInput.TriggerOnChangeHandler();
						xUiC_TextInput2.Text = tuple.Item2.ToString();
						xUiC_TextInput2.TriggerOnChangeHandler();
					}
					else
					{
						xUiC_TextInput.Text = defaultValue.ToString();
						xUiC_TextInput.TriggerOnChangeHandler();
					}
				}
			}
			else
			{
				xUiC_ComboBoxFloat.Value = (float)defaultValue;
				xUiC_ComboBoxFloat.TriggerValueChangedEvent(value2);
			}
		}
		else
		{
			xUiC_ComboBoxInt.Value = (int)defaultValue;
			xUiC_ComboBoxInt.TriggerValueChangedEvent(value);
		}
	}
}
