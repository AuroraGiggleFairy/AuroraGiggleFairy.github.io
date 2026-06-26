using System;
using System.Collections.Generic;

namespace GameSparks.Platforms;

public class TimerController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public long timeOfLastUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IControlledTimer> timers = new List<IControlledTimer>();

	public void Initialize()
	{
		timeOfLastUpdate = DateTime.UtcNow.Ticks;
	}

	public void Update()
	{
		long num = DateTime.UtcNow.Ticks - timeOfLastUpdate;
		timeOfLastUpdate += num;
		foreach (IControlledTimer timer in timers)
		{
			timer.Update(num);
		}
	}

	public void AddTimer(IControlledTimer timer)
	{
		timers.Add(timer);
	}

	public void RemoveTimer(IControlledTimer timer)
	{
		timers.Remove(timer);
	}
}
