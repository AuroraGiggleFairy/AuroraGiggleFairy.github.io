using UnityEngine.Scripting;

[Preserve]
public class XUiC_Counter : XUiController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController countUp;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController countDown;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController countMax;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController counter;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_TextInput textInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public int count = 1;

	public int MaxCount = 1;

	public int Step = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiEvent_InputOnChangedEventHandler textInputChangedDelegate;

	public int Count
	{
		get
		{
			return count;
		}
		set
		{
			count = value;
			IsDirty = true;
		}
	}

	public event XUiEvent_OnCountChanged OnCountChanged;

	public override void Init()
	{
		base.Init();
		countUp = GetChildById("countUp");
		countDown = GetChildById("countDown");
		countMax = GetChildById("countMax");
		counter = GetChildById("text");
		countUp.OnPress += HandleCountUpOnPress;
		countDown.OnPress += HandleCountDownOnPress;
		countMax.OnPress += HandleMaxCountOnPress;
		countUp.OnHold += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, EHoldType _event, float _duration, float _timedEvent) =>
		{
			if (_event == EHoldType.HoldTimed)
			{
				HandleCountUpOnPress(_sender, -1);
			}
		};
		countDown.OnHold += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, EHoldType _event, float _duration, float _timedEvent) =>
		{
			if (_event == EHoldType.HoldTimed)
			{
				HandleCountDownOnPress(_sender, -1);
			}
		};
		textInputChangedDelegate = TextInput_OnChangeHandler;
		textInput = GetChildByType<XUiC_TextInput>();
		textInput.OnChangeHandler += textInputChangedDelegate;
		textInput.OnInputSelectedHandler += TextInput_HandleInputDeselected;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleCountChangedEvent()
	{
		if (this.OnCountChanged != null)
		{
			OnCountChangedEventArgs e = new OnCountChangedEventArgs();
			e.Count = Count;
			this.OnCountChanged(this, e);
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TextInput_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		int result = 0;
		if (int.TryParse(_text, out result))
		{
			Count = result;
			if (Count > MaxCount)
			{
				Count = MaxCount;
				((XUiC_TextInput)_sender).Text = Count.ToString();
			}
			else if (Count <= 0)
			{
				Count = Step;
				((XUiC_TextInput)_sender).Text = Count.ToString();
			}
			HandleCountChangedEvent();
		}
		else
		{
			Count = Step;
			HandleCountChangedEvent();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleMaxCountOnPress(XUiController _sender, int _mouseButton)
	{
		Count = MaxCount;
		HandleCountChangedEvent();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			textInput.OnChangeHandler -= textInputChangedDelegate;
			XUiV_Label obj = (XUiV_Label)counter.ViewComponent;
			string text = (textInput.Text = ((textInput.Text == "") ? "" : ((Count > 0) ? Count.ToString() : "-")));
			obj.Text = text;
			textInput.OnChangeHandler += textInputChangedDelegate;
			IsDirty = false;
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleCountDownOnPress(XUiController _sender, int _mouseButton)
	{
		if (Count > 1)
		{
			Count -= Step;
			HandleStepClamping();
		}
		HandleCountChangedEvent();
		ForceTextRefresh();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleCountUpOnPress(XUiController _sender, int _mouseButton)
	{
		if (Count < MaxCount)
		{
			Count += Step;
			HandleStepClamping();
			HandleCountChangedEvent();
			ForceTextRefresh();
		}
	}

	public void SetToMaxCount()
	{
		Count = MaxCount;
		HandleStepClamping();
		HandleCountChangedEvent();
		ForceTextRefresh();
	}

	public void SetCount(int count)
	{
		if (Count != count)
		{
			Count = count;
			HandleCountChangedEvent();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleStepClamping()
	{
		if (Step > 1)
		{
			int num = Count % Step;
			if (num != 0)
			{
				Count -= num;
			}
		}
	}

	public void ForceTextRefresh()
	{
		XUiV_Label obj = (XUiV_Label)counter.ViewComponent;
		string text = (textInput.Text = count.ToString());
		obj.Text = text;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TextInput_HandleInputDeselected(XUiController _sender, bool _selected)
	{
		if (!_selected)
		{
			ForceTextRefresh();
		}
	}
}
