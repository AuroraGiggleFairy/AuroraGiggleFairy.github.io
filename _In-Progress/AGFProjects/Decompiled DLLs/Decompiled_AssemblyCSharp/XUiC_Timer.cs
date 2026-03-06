using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Timer : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float fullTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timeLeft;

	public string currentOpenEventText;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentFillAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentTimeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public TimerEventData eventData;

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.CursorController.SetCursorHidden(_hidden: true);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.CancelButton, "igcoCancel", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
	}

	public override void OnClose()
	{
		base.OnClose();
		UpdateTimer(0f, 1f);
		fullTime = 0f;
		base.xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		if (timeLeft > 0f)
		{
			eventData.timeLeft = timeLeft;
			eventData.HandleCloseEvent(timeLeft);
		}
		base.xui.playerUI.entityPlayer.SetControllable(_b: true);
		CursorControllerAbs.SetCursor(CursorControllerAbs.ECursorType.Default);
	}

	public void UpdateTimer(float _timeLeft, float _fillAmount)
	{
		currentTimeLeft = _timeLeft;
		currentFillAmount = _fillAmount;
		RefreshBindings();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!(fullTime > 0f))
		{
			return;
		}
		if (eventData.CloseOnHit && base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.hasBeenAttackedTime > 0)
		{
			base.xui.playerUI.windowManager.Close("timer");
			return;
		}
		timeLeft -= _dt;
		float fillAmount = timeLeft / fullTime;
		UpdateTimer(timeLeft, fillAmount);
		if (eventData.alternateTime != -1f && fullTime - timeLeft > eventData.alternateTime)
		{
			eventData.timeLeft = timeLeft;
			timeLeft = 0f;
			fullTime = 0f;
			base.xui.playerUI.windowManager.Close("timer");
			GameManager.Instance.SetPauseWindowEffects(_bOn: false);
			base.xui.dragAndDrop.InMenu = false;
			eventData.HandleAlternateEvent();
		}
		else if (timeLeft <= 0f)
		{
			timeLeft = 0f;
			fullTime = 0f;
			base.xui.playerUI.windowManager.Close("timer");
			GameManager.Instance.SetPauseWindowEffects(_bOn: false);
			base.xui.dragAndDrop.InMenu = false;
			eventData.HandleEvent();
		}
	}

	public void SetTimer(float _fullTime, TimerEventData _eventData, float startTime = -1f, string _labelText = "")
	{
		currentOpenEventText = _labelText;
		fullTime = _fullTime;
		if (startTime == -1f)
		{
			timeLeft = fullTime;
		}
		else
		{
			timeLeft = startTime;
		}
		eventData = _eventData;
		UpdateTimer(timeLeft, timeLeft / fullTime);
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
