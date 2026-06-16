using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKController : MonoBehaviour
{
	[Serializable]
	public struct Target
	{
		public AvatarIKGoal avatarGoal;

		public Transform transform;

		public Vector3 position;

		public Vector3 rotation;
	}

	[Serializable]
	public struct Constraint
	{
		public TwoBoneIKConstraint tbConstraint;

		public float originalWeight;

		public Transform originalTargetT;
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTargetTypeCount = 4;

	public static string[] IKNames = new string[4] { "IKFootL", "IKFootR", "IKHandL", "IKHandR" };

	public List<Target> targets;

	public Constraint[] rigConstraints = new Constraint[4];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Animator animator;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Rig rig;

	public void Start()
	{
		animator = GetComponent<Animator>();
		Transform transform = base.transform.Find("IKRig");
		if (!transform)
		{
			return;
		}
		rig = transform.GetComponent<Rig>();
		int childCount = transform.childCount;
		Constraint constraint = default(Constraint);
		for (int i = 0; i < childCount; i++)
		{
			TwoBoneIKConstraint component = transform.GetChild(i).GetComponent<TwoBoneIKConstraint>();
			if (!component)
			{
				continue;
			}
			TwoBoneIKConstraintData data = component.data;
			Transform target = data.target;
			if ((bool)target)
			{
				int num = NameToIndex(target.name);
				if (num >= 0)
				{
					constraint.tbConstraint = component;
					constraint.originalWeight = component.weight;
					constraint.originalTargetT = target;
					rigConstraints[num] = constraint;
				}
			}
		}
		ModifyRig();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int NameToIndex(string name)
	{
		for (int i = 0; i < IKNames.Length; i++)
		{
			if (name == IKNames[i])
			{
				return i;
			}
		}
		return -1;
	}

	public void SetTargets(List<Target> _targets)
	{
		targets = _targets;
	}

	public void Cleanup()
	{
		targets = null;
		if ((bool)rig)
		{
			ModifyRig();
		}
	}

	public void ModifyRig()
	{
		if (targets == null)
		{
			for (int i = 0; i < 4; i++)
			{
				Constraint constraint = rigConstraints[i];
				if ((bool)constraint.originalTargetT)
				{
					TwoBoneIKConstraint tbConstraint = constraint.tbConstraint;
					tbConstraint.weight = constraint.originalWeight;
					tbConstraint.data.target = constraint.originalTargetT;
					Transform obj = tbConstraint.transform;
					obj.position = Vector3.zero;
					obj.rotation = Quaternion.identity;
				}
			}
		}
		else
		{
			Transform transform = base.transform;
			for (int j = 0; j < targets.Count; j++)
			{
				Target target = targets[j];
				int avatarGoal = (int)target.avatarGoal;
				TwoBoneIKConstraint tbConstraint2 = rigConstraints[avatarGoal].tbConstraint;
				if ((bool)tbConstraint2)
				{
					Transform transform2 = target.transform;
					if (!transform2)
					{
						transform2 = tbConstraint2.transform;
						Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
						Vector3 position = localToWorldMatrix.MultiplyPoint(target.position);
						transform2.position = position;
						Quaternion rotation = localToWorldMatrix.rotation * Quaternion.Euler(target.rotation);
						transform2.rotation = rotation;
					}
					tbConstraint2.weight = 1f;
					tbConstraint2.data.target = transform2;
				}
			}
		}
		GetComponent<RigBuilder>().Build();
	}

	public void OnAnimatorIK()
	{
		if (!animator)
		{
			return;
		}
		if (targets == null)
		{
			for (int i = 0; i < 4; i++)
			{
				animator.SetIKPositionWeight((AvatarIKGoal)i, 0f);
				animator.SetIKRotationWeight((AvatarIKGoal)i, 0f);
			}
			return;
		}
		Transform transform = base.transform;
		for (int j = 0; j < targets.Count; j++)
		{
			Target target = targets[j];
			animator.SetIKPositionWeight(target.avatarGoal, 1f);
			animator.SetIKRotationWeight(target.avatarGoal, 1f);
			Transform transform2 = target.transform;
			if (!transform2)
			{
				Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
				Vector3 goalPosition = localToWorldMatrix.MultiplyPoint(target.position);
				animator.SetIKPosition(target.avatarGoal, goalPosition);
				Quaternion goalRotation = localToWorldMatrix.rotation * Quaternion.Euler(target.rotation);
				animator.SetIKRotation(target.avatarGoal, goalRotation);
			}
			else
			{
				animator.SetIKPosition(target.avatarGoal, transform2.position);
				animator.SetIKRotation(target.avatarGoal, transform2.rotation);
			}
		}
	}
}
