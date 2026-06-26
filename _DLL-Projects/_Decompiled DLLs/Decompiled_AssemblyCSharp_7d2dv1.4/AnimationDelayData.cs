using UnityEngine.Scripting;

[Preserve]
public class AnimationDelayData
{
	public struct AnimationDelays(float _rayCast, float _rayCastMoving, float _holster, float _unholster, bool _twoHanded)
	{
		public float RayCast = _rayCast;

		public float RayCastMoving = _rayCastMoving;

		public float Holster = _holster;

		public float Unholster = _unholster;

		public bool TwoHanded = _twoHanded;
	}

	public static AnimationDelays[] AnimationDelay;

	public static void InitStatic()
	{
		AnimationDelay = new AnimationDelays[100];
		for (int i = 0; i < AnimationDelay.Length; i++)
		{
			AnimationDelay[i] = new AnimationDelays(0f, 0f, Constants.cMinHolsterTime, Constants.cMinUnHolsterTime, _twoHanded: false);
		}
	}

	public static void Cleanup()
	{
		InitStatic();
	}
}
