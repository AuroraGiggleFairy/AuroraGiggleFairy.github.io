public interface IFeaturePhysicalCapabilities
{
	bool IsMovementBlocked(Vector3i _blockPos, BlockValue _blockValue, BlockFace _face);

	bool IsMovementBlocked(Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides);

	bool IsSeeThrough(Vector3i _blockPos, BlockValue _blockValue);

	float GetStepHeight(Vector3i _blockPos, BlockValue _blockValue, BlockFace crossingFace);
}
