public class GUIBlinker
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float ms;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastBlinkTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bResult = true;

	public GUIBlinker(float _ms)
	{
		ms = _ms;
	}

	public bool Draw(float _curTime)
	{
		if (_curTime - lastBlinkTime > ms)
		{
			lastBlinkTime = _curTime;
			bResult = !bResult;
		}
		return bResult;
	}
}
