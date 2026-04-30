using UnityEngine;

public class GameTimer
{
	public ulong ticks;

	public ulong ticksSincePlayfieldLoaded;

	public int elapsedTicks;

	public float elapsedPartialTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public double elapsedTicksD;

	[PublicizedFrom(EAccessModifier.Private)]
	public float ticksPerSecond;

	[PublicizedFrom(EAccessModifier.Private)]
	public long lastMillis;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameTimer m_Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public MicroStopwatch ms;

	public static GameTimer Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = new GameTimer(20f);
			}
			return m_Instance;
		}
	}

	public GameTimer(float _t)
	{
		ticksPerSecond = _t;
		ms = new MicroStopwatch();
		Reset(0uL);
	}

	public void Reset(ulong _ticks = 0uL)
	{
		elapsedPartialTicks = 0f;
		ticks = _ticks;
		ticksSincePlayfieldLoaded = 0uL;
		elapsedTicksD = 0.0;
		lastMillis = 0L;
		ms.ResetAndRestart();
	}

	public void updateTimer(bool _bServerIsStopped)
	{
		if (_bServerIsStopped)
		{
			Reset(ticks);
			return;
		}
		long elapsedMilliseconds = ms.ElapsedMilliseconds;
		long num = elapsedMilliseconds - lastMillis;
		lastMillis = elapsedMilliseconds;
		elapsedTicksD += (double)(Time.timeScale * (float)num) / 1000.0 * (double)ticksPerSecond;
		elapsedTicks = (int)elapsedTicksD;
		elapsedPartialTicks = (float)(elapsedTicksD - (double)elapsedTicks);
		elapsedTicksD -= elapsedTicks;
		ticks += (ulong)elapsedTicks;
		ticksSincePlayfieldLoaded += (ulong)elapsedTicks;
	}
}
