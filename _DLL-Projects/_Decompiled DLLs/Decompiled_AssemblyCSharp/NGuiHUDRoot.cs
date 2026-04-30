using UnityEngine;

public class NGuiHUDRoot : MonoBehaviour
{
	public static GameObject go;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		go = base.gameObject;
	}
}
