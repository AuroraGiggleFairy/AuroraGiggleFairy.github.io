using Unity.Mathematics;

public struct GroundWaterBounds
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte state;

	public byte waterHeight;

	public byte bottom;

	public bool IsGroundWater => state != 0;

	public GroundWaterBounds(int _groundHeight, int _waterHeight)
	{
		state = 1;
		waterHeight = (byte)math.clamp(_waterHeight, 0, 255);
		bottom = (byte)math.clamp(_groundHeight, 0, waterHeight);
	}
}
