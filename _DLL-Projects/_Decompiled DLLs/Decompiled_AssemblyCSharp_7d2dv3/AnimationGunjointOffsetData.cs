using UnityEngine;

public class AnimationGunjointOffsetData
{
	public struct AnimationGunjointOffsets(Vector3 _position, Vector3 _rotation)
	{
		public Vector3 position = _position;

		public Vector3 rotation = _rotation;
	}

	public static AnimationGunjointOffsets[] AnimationGunjointOffset;

	public static void InitStatic()
	{
		AnimationGunjointOffset = new AnimationGunjointOffsets[100];
		for (int i = 0; i < AnimationGunjointOffset.Length; i++)
		{
			AnimationGunjointOffset[i] = new AnimationGunjointOffsets(Vector3.zero, Vector3.zero);
		}
	}

	public static void Cleanup()
	{
		InitStatic();
	}
}
