using UnityEngine;

public class LocalPlayerFinalCamera : MonoBehaviour
{
	public EntityPlayerLocal entityPlayerLocal;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreCull()
	{
		entityPlayerLocal.finalCamera.fieldOfView = entityPlayerLocal.playerCamera.fieldOfView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreRender()
	{
		entityPlayerLocal.renderManager.DynamicResolutionRender();
	}
}
