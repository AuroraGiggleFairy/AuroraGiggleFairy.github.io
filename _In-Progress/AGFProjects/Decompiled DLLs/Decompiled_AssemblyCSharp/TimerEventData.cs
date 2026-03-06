public class TimerEventData
{
	public object Data;

	public bool CloseOnHit;

	public float alternateTime = -1f;

	public float timeLeft = -1f;

	public event TimerEventHandler Event;

	public event TimerEventHandler CloseEvent;

	public event TimerEventHandler AlternateEvent;

	public void HandleEvent()
	{
		if (this.Event != null)
		{
			this.Event(this);
		}
	}

	public void HandleAlternateEvent()
	{
		if (this.AlternateEvent != null)
		{
			this.AlternateEvent(this);
		}
	}

	public void HandleCloseEvent(float _timeLeft)
	{
		timeLeft = _timeLeft;
		if (this.CloseEvent != null)
		{
			this.CloseEvent(this);
		}
	}
}
