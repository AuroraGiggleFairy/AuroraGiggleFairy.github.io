using System.Collections;
using UnityEngine;

public class RemoveSelfLater : MonoBehaviour
{
	public float WaitSeconds = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		StartCoroutine(remove());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator remove()
	{
		yield return new WaitForSeconds(WaitSeconds);
		Object.Destroy(base.gameObject);
	}
}
