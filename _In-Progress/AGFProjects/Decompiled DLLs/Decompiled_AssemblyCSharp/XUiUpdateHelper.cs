using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public class XUiUpdateHelper : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		XUiUpdater.Update();
	}
}
