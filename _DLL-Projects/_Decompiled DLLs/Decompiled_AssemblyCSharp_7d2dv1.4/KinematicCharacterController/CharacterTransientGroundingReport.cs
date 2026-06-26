using UnityEngine;

namespace KinematicCharacterController;

public struct CharacterTransientGroundingReport
{
	public bool FoundAnyGround;

	public bool IsStableOnGround;

	public bool SnappingPrevented;

	public Vector3 GroundNormal;

	public Vector3 InnerGroundNormal;

	public Vector3 OuterGroundNormal;

	public void CopyFrom(CharacterGroundingReport groundingReport)
	{
		FoundAnyGround = groundingReport.FoundAnyGround;
		IsStableOnGround = groundingReport.IsStableOnGround;
		SnappingPrevented = groundingReport.SnappingPrevented;
		GroundNormal = groundingReport.GroundNormal;
		InnerGroundNormal = groundingReport.InnerGroundNormal;
		OuterGroundNormal = groundingReport.OuterGroundNormal;
	}
}
