using UnityEngine;

[ExecuteInEditMode]
public class DangerRoomEnvironmentSim : MonoBehaviour
{
	public bool simulateWind = true;

	[Range(0f, 100f)]
	public float wind = 100f;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (simulateWind)
		{
			Shader.SetGlobalVector("_Wind", new Vector4(wind, 0f, 0f, 0f));
		}
		else
		{
			Shader.SetGlobalVector("_Wind", new Vector4(0f, 0f, 0f, 0f));
		}
	}
}
