using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Timer : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float fullTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentOpenEventText;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentFillAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentTimeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public TimerEventData eventData;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool skipCloseEvent;

	public override void OnOpen()
	{
		base.OnOpen();
		xui.playerUI.CursorController.SetCursorHidden(_hidden: true);
		xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.CancelButton, "igcoCancel", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
	}

	public override void OnClose()
	{
		base.OnClose();
		updateTimer(0f, 1f);
		fullTime = 0f;
		xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
		xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		if (!skipCloseEvent)
		{
			eventData?.HandleClosed();
		}
		eventData = null;
		xui.playerUI.entityPlayer.SetControllable(_b: true);
		CursorControllerAbs.SetCursor(CursorControllerAbs.ECursorType.Default);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTimer(float _timeLeft, float _fillAmount)
	{
		currentTimeLeft = _timeLeft;
		currentFillAmount = _fillAmount;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void timeReachedNull()
	{
		fullTime = 0f;
		timeLeft = 0f;
		skipCloseEvent = true;
		TimerEventData timerEventData = eventData;
		xui.playerUI.windowManager.Close(windowGroup);
		xui.DragAndDropWindow.InMenu = false;
		timerEventData?.HandleFullTimeFinished();
		skipCloseEvent = false;
		eventData = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void alternateTimeReached()
	{
		fullTime = 0f;
		timeLeft = 0f;
		skipCloseEvent = true;
		TimerEventData timerEventData = eventData;
		xui.playerUI.windowManager.Close(windowGroup);
		xui.DragAndDropWindow.InMenu = false;
		timerEventData?.HandleAlternateEvent();
		skipCloseEvent = false;
		eventData = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (fullTime <= 0f)
		{
			return;
		}
		if (eventData.CloseOnHit && xui.playerUI.entityPlayer != null && xui.playerUI.entityPlayer.hasBeenAttackedTime > 0)
		{
			xui.playerUI.windowManager.Close(windowGroup);
			return;
		}
		timeLeft -= _dt;
		if (timeLeft < 0f)
		{
			timeLeft = 0f;
		}
		float fillAmount = timeLeft / fullTime;
		updateTimer(timeLeft, fillAmount);
		eventData.Completion = (fullTime - timeLeft) / fullTime;
		if (timeLeft <= 0f)
		{
			timeReachedNull();
		}
		else if (eventData.AlternateTime >= 0f && fullTime - timeLeft > eventData.AlternateTime)
		{
			alternateTimeReached();
		}
		else if (eventData.CancelWithActivateButton && xui.playerUI.playerInput.PermanentActions.Activate.WasPressed)
		{
			xui.playerUI.windowManager.Close(windowGroup);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setTimer(float _fullTime, TimerEventData _eventData, float _startTime = -1f, string _labelText = "")
	{
		currentOpenEventText = _labelText;
		fullTime = _fullTime;
		timeLeft = ((_startTime < 0f) ? fullTime : _startTime);
		float fillAmount = 1f;
		if (fullTime > _startTime && fullTime > 0f)
		{
			fillAmount = timeLeft / fullTime;
		}
		eventData = _eventData;
		updateTimer(timeLeft, fillAmount);
	}

	public static void OpenTimer(XUi _xui, float _fullTime, TimerEventData _eventData, float _startTime = -1f, string _labelText = "", bool _modal = true)
	{
		XUiC_Timer childByType = _xui.GetChildByType<XUiC_Timer>();
		_xui.playerUI.windowManager.Open(childByType.windowGroup, _modal);
		childByType.setTimer(_fullTime, _eventData, _startTime, _labelText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "timeleft":
			_value = currentTimeLeft.ToCultureInvariantString("0.0");
			return true;
		case "percent":
			_value = currentFillAmount.ToCultureInvariantString();
			return true;
		case "caption":
			_value = currentOpenEventText ?? "";
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
