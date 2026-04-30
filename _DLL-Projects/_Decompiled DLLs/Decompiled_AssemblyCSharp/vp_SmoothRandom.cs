using UnityEngine;

public class vp_SmoothRandom
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static vp_FractalNoise s_Noise;

	public static Vector3 GetVector3(float speed)
	{
		float x = Time.time * 0.01f * speed;
		return new Vector3(Get().HybridMultifractal(x, 15.73f, 0.58f), Get().HybridMultifractal(x, 63.94f, 0.58f), Get().HybridMultifractal(x, 0.2f, 0.58f));
	}

	public static Vector3 GetVector3Centered(float speed)
	{
		float x = Time.time * 0.01f * speed;
		float x2 = (Time.time - 1f) * 0.01f * speed;
		Vector3 vector = new Vector3(Get().HybridMultifractal(x, 15.73f, 0.58f), Get().HybridMultifractal(x, 63.94f, 0.58f), Get().HybridMultifractal(x, 0.2f, 0.58f));
		Vector3 vector2 = new Vector3(Get().HybridMultifractal(x2, 15.73f, 0.58f), Get().HybridMultifractal(x2, 63.94f, 0.58f), Get().HybridMultifractal(x2, 0.2f, 0.58f));
		return vector - vector2;
	}

	public static Vector3 GetVector3Centered(float time, float speed)
	{
		float x = time * 0.01f * speed;
		float x2 = (time - 1f) * 0.01f * speed;
		Vector3 vector = new Vector3(Get().HybridMultifractal(x, 15.73f, 0.58f), Get().HybridMultifractal(x, 63.94f, 0.58f), Get().HybridMultifractal(x, 0.2f, 0.58f));
		Vector3 vector2 = new Vector3(Get().HybridMultifractal(x2, 15.73f, 0.58f), Get().HybridMultifractal(x2, 63.94f, 0.58f), Get().HybridMultifractal(x2, 0.2f, 0.58f));
		return vector - vector2;
	}

	public static float Get(float speed)
	{
		float num = Time.time * 0.01f * speed;
		return Get().HybridMultifractal(num * 0.01f, 15.7f, 0.65f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static vp_FractalNoise Get()
	{
		if (s_Noise == null)
		{
			s_Noise = new vp_FractalNoise(1.27f, 2.04f, 8.36f);
		}
		return s_Noise;
	}
}
