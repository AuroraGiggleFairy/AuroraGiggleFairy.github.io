using UnityEngine;

public class ScreenSpaceParticleAspectScaler : MonoBehaviour
{
	public float designTimeAspectRatio;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		float x = (float)Screen.width / (float)Screen.height / designTimeAspectRatio;
		base.transform.localScale = new Vector3(x, 1f, 1f);
	}
}
