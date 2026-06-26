using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AttachedToEntitySlotInfo
{
	public int slotIdx;

	public Transform enterParentTransform;

	public Vector3 enterPosition;

	public Vector3 enterRotation;

	public Vector2 pitchRestriction;

	public Vector2 yawRestriction;

	public bool bKeep3rdPersonModelVisible;

	public bool bAllow3rdPerson;

	public bool bReplaceLocalInventory;

	public List<AttachedToEntitySlotExit> exits = new List<AttachedToEntitySlotExit>();
}
