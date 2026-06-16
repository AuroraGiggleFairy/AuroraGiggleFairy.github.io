using UnityEngine;

public class TimeRotateObject : MonoBehaviour
{
	public Transform hourTransform;

	public Transform minuteTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!GameManager.Instance)
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (world != null)
		{
			uint num = GameUtils.WorldTimeToTotalMinutes(world.worldTime) % 60;
			if ((bool)hourTransform)
			{
				int num2 = GameUtils.WorldTimeToTotalHours(world.worldTime);
				hourTransform.localEulerAngles = new Vector3(0f, 0f, (float)(num2 % 12) * 30f + (float)num * 0.5f);
			}
			if ((bool)minuteTransform)
			{
				minuteTransform.localEulerAngles = new Vector3(0f, 0f, num * 6);
			}
		}
	}
}
