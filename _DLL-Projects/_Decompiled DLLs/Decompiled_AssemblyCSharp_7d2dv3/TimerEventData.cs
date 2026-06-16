public class TimerEventData
{
	public object Data;

	public bool CancelWithActivateButton;

	public bool CloseOnHit;

	public float AlternateTime = -1f;

	public float Completion = -1f;

	public event TimerEventHandler FullTimeFinishEvent;

	public event TimerEventHandler CloseEvent;

	public event TimerEventHandler AlternateEvent;

	public void HandleFullTimeFinished()
	{
		this.FullTimeFinishEvent?.Invoke(this);
	}

	public void HandleAlternateEvent()
	{
		this.AlternateEvent?.Invoke(this);
	}

	public void HandleClosed()
	{
		this.CloseEvent?.Invoke(this);
	}
}
