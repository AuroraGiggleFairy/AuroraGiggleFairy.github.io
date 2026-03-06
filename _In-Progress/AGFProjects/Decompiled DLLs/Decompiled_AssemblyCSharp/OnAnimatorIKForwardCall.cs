using UnityEngine;

public class OnAnimatorIKForwardCall : MonoBehaviour
{
	public IOnAnimatorIKCallback Callback;

	public void OnAnimatorIK(int layerIndex)
	{
		if (Callback != null)
		{
			Callback.OnAnimatorIK(layerIndex);
		}
	}
}
