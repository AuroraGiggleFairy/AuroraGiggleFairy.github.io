using System;
using UnityEngine;

public class ArchetypePreviewIKController : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Animator animator;

	public bool ikActive;

	public Transform rightHandObj;

	public Transform lookObj;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 leftFootPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 rightFootPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion leftFootRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion rightFootRot;

	public Vector3 FootRotationModifier = new Vector3(-62f, -198f, -93.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		animator = GetComponent<Animator>();
		Transform transform = base.transform.FindInChilds("LeftFoot");
		Transform transform2 = base.transform.FindInChilds("RightFoot");
		leftFootPos = transform.position;
		leftFootRot = transform.rotation;
		rightFootPos = transform2.position;
		rightFootRot = transform2.rotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnAnimatorIK()
	{
		if ((bool)animator)
		{
			animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
			animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPos);
			animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.Euler(leftFootRot.eulerAngles.x + FootRotationModifier.x, leftFootRot.eulerAngles.y - FootRotationModifier.y, leftFootRot.eulerAngles.z + FootRotationModifier.z));
			animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
			animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
			animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPos);
			animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.Euler(rightFootRot.eulerAngles.x + FootRotationModifier.x, rightFootRot.eulerAngles.y + FootRotationModifier.y, rightFootRot.eulerAngles.z + FootRotationModifier.z));
		}
	}
}
