using System.Threading;
using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
	public float MaxFrames = 9999f;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (MaxFrames < 4f)
		{
			MaxFrames = 4f;
		}
		if (MaxFrames < 60f)
		{
			int num = (int)(1000.0 / (double)MaxFrames - (double)(Time.deltaTime * 1000f));
			if (num > 0)
			{
				Thread.Sleep(num);
			}
		}
	}
}
