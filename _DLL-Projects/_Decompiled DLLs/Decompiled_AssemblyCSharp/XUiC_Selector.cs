using UnityEngine.Scripting;

[Preserve]
public class XUiC_Selector : XUiController
{
	public int Min;

	public int Max = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Label currentValue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int selectedIndex;

	public int SelectedIndex
	{
		get
		{
			return selectedIndex;
		}
		set
		{
			selectedIndex = value;
		}
	}

	public event XUiEvent_SelectedIndexChanged OnSelectedIndexChanged;

	public override void OnOpen()
	{
		base.OnOpen();
		currentValue.Text = selectedIndex.ToString();
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (!(name == "min"))
		{
			if (name == "max")
			{
				Max = int.Parse(value);
				return true;
			}
			return base.ParseAttribute(name, value, _parent);
		}
		Min = int.Parse(value);
		return true;
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("forward");
		XUiController childById2 = GetChildById("back");
		XUiController childById3 = GetChildById("currentValue");
		if (childById != null)
		{
			childById.OnPress += ForwardButton_OnPress;
		}
		if (childById2 != null)
		{
			childById2.OnPress += BackButton_OnPress;
		}
		if (childById3 != null && childById3.ViewComponent is XUiV_Label)
		{
			currentValue = childById3.ViewComponent as XUiV_Label;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BackButton_OnPress(XUiController _sender, int _mouseButton)
	{
		selectedIndex--;
		BackPressed();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ForwardButton_OnPress(XUiController _sender, int _mouseButton)
	{
		selectedIndex++;
		ForwardPressed();
		IsDirty = true;
	}

	public virtual void BackPressed()
	{
		if (selectedIndex < Min)
		{
			selectedIndex = Max;
		}
		currentValue.Text = selectedIndex.ToString();
		if (this.OnSelectedIndexChanged != null)
		{
			this.OnSelectedIndexChanged(selectedIndex);
		}
	}

	public virtual void ForwardPressed()
	{
		if (selectedIndex > Max)
		{
			selectedIndex = Min;
		}
		currentValue.Text = selectedIndex.ToString();
		if (this.OnSelectedIndexChanged != null)
		{
			this.OnSelectedIndexChanged(selectedIndex);
		}
	}
}
