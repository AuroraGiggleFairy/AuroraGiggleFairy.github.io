using UnityEngine.Scripting;

[Preserve]
public class BlockLadder : Block
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] climbableRotations = new byte[32]
	{
		1, 1, 1, 1, 1, 1, 1, 1, 0, 1,
		0, 1, 1, 0, 1, 0, 0, 1, 0, 1,
		1, 0, 1, 0, 0, 0, 0, 0, 0, 0,
		0, 0
	};

	public override bool IsElevator()
	{
		return true;
	}

	public override bool IsElevator(int rotation)
	{
		return climbableRotations[rotation] != 0;
	}
}
