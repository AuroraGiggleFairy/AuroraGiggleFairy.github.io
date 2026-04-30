using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class DroneRunningLightMan
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<DroneRunningLight> runningLights;

	public static DroneRunningLightMan instance;

	public DroneRunningLightMan()
	{
		instance = this;
		runningLights = new List<DroneRunningLight>();
	}

	public void AddLight(DroneRunningLight _light)
	{
		runningLights.Add(_light);
	}

	public void QueueLight(DroneRunningLight _light)
	{
	}
}
