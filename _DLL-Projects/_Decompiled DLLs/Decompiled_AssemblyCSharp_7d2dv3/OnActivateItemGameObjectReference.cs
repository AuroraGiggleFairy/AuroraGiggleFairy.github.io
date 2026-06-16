using UnityEngine;

public class OnActivateItemGameObjectReference : MonoBehaviour
{
	public Transform ActivateGameObjectTransform;

	public void ActivateItem(bool _activate)
	{
		if (ActivateGameObjectTransform != null)
		{
			ActivateGameObjectTransform.gameObject.SetActive(_activate);
		}
	}

	public bool IsActivated()
	{
		if (ActivateGameObjectTransform != null)
		{
			return ActivateGameObjectTransform.gameObject.activeSelf;
		}
		return false;
	}
}
