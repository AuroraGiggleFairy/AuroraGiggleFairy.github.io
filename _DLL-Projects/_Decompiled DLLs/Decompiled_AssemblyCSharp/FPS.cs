using UnityEngine;

public class FPS
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float accum;

	[PublicizedFrom(EAccessModifier.Private)]
	public int frames;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timeleft;

	[PublicizedFrom(EAccessModifier.Private)]
	public float restartTime = 0.5f;

	public float Counter;

	public FPS(float _restartTime)
	{
		restartTime = _restartTime;
		timeleft = _restartTime;
	}

	public bool Update()
	{
		timeleft -= Time.unscaledDeltaTime;
		accum += Time.unscaledDeltaTime;
		frames++;
		if ((double)timeleft <= 0.0)
		{
			Counter = (float)frames / accum;
			timeleft = restartTime;
			accum = 0f;
			frames = 0;
			return true;
		}
		return false;
	}
}
