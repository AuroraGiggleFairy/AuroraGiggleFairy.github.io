using System;

public class CountdownTimer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public long ms;

	[PublicizedFrom(EAccessModifier.Private)]
	public long offset;

	public TimeSpan Elapsed;

	public bool IsRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime StartTime;

	public long ElapsedMilliseconds
	{
		get
		{
			return (long)Elapsed.TotalMilliseconds;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
		}
	}

	public CountdownTimer(float _seconds, bool _start = true)
	{
		ms = (int)(_seconds * 1000f);
		IsRunning = _start;
		offset = 0L;
		if (IsRunning)
		{
			ResetAndRestart();
		}
		else
		{
			Reset();
		}
	}

	public void SetTimeout(float _seconds)
	{
		ms = (int)(_seconds * 1000f);
	}

	public bool HasPassed()
	{
		bool flag = false;
		if (IsRunning)
		{
			Update();
			flag = ((offset == 0L) ? (ElapsedMilliseconds > ms) : (ElapsedMilliseconds + offset > ms));
			if (flag)
			{
				offset = 0L;
			}
		}
		return flag;
	}

	public void SetPassedIn(float _seconds)
	{
		offset = (long)((float)ms - _seconds * 1000f) - ElapsedMilliseconds;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		Elapsed = DateTime.Now.Subtract(StartTime);
	}

	public void Reset()
	{
		Elapsed = TimeSpan.Zero;
		StartTime = DateTime.Now;
		IsRunning = false;
	}

	public void ResetAndRestart()
	{
		Reset();
		IsRunning = true;
	}

	public void Stop()
	{
		IsRunning = false;
	}
}
