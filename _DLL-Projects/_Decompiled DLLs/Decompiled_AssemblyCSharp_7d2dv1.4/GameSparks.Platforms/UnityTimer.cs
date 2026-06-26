using System;

namespace GameSparks.Platforms;

public class UnityTimer : IControlledTimer, IGameSparksTimer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Action callback;

	[PublicizedFrom(EAccessModifier.Private)]
	public int interval;

	[PublicizedFrom(EAccessModifier.Private)]
	public long elapsedTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool running;

	[PublicizedFrom(EAccessModifier.Private)]
	public TimerController controller;

	public void SetController(TimerController controller)
	{
		this.controller = controller;
		this.controller.AddTimer(this);
	}

	public void Initialize(int interval, Action callback)
	{
		this.callback = callback;
		this.interval = interval;
		running = true;
	}

	public void Trigger()
	{
	}

	public void Stop()
	{
		running = false;
		callback = null;
		controller.RemoveTimer(this);
	}

	public void Update(long ticks)
	{
		if (!running)
		{
			return;
		}
		elapsedTicks += ticks;
		if (elapsedTicks > interval)
		{
			elapsedTicks -= interval;
			if (callback != null)
			{
				callback();
			}
		}
	}
}
