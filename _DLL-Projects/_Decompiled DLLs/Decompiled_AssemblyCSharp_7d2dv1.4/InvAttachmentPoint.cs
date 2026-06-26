using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Item Attachment Point")]
public class InvAttachmentPoint : MonoBehaviour
{
	public InvBaseItem.Slot slot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject mPrefab;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject mChild;

	public GameObject Attach(GameObject prefab)
	{
		if (mPrefab != prefab)
		{
			mPrefab = prefab;
			if (mChild != null)
			{
				UnityEngine.Object.Destroy(mChild);
			}
			if (mPrefab != null)
			{
				Transform transform = base.transform;
				mChild = UnityEngine.Object.Instantiate(mPrefab, transform.position, transform.rotation);
				Transform obj = mChild.transform;
				obj.parent = transform;
				obj.localPosition = Vector3.zero;
				obj.localRotation = Quaternion.identity;
				obj.localScale = Vector3.one;
			}
		}
		return mChild;
	}
}
