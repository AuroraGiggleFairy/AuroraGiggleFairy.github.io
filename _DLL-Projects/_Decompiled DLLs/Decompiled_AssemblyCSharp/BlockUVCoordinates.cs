using UnityEngine;

public class BlockUVCoordinates
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Rect[] m_BlockFaceUvCoordinates = new Rect[6];

	public Rect[] BlockFaceUvCoordinates => m_BlockFaceUvCoordinates;

	public BlockUVCoordinates(Rect topUvCoordinates, Rect sideUvCoordinates, Rect bottomUvCoordinates)
	{
		BlockFaceUvCoordinates[0] = topUvCoordinates;
		BlockFaceUvCoordinates[1] = bottomUvCoordinates;
		BlockFaceUvCoordinates[2] = sideUvCoordinates;
		BlockFaceUvCoordinates[4] = sideUvCoordinates;
		BlockFaceUvCoordinates[3] = sideUvCoordinates;
		BlockFaceUvCoordinates[5] = sideUvCoordinates;
	}

	public BlockUVCoordinates(Rect topUvCoordinates, Rect bottomUvCoordinates, Rect northUvCoordinates, Rect southUvCoordinates, Rect westUvCoordinates, Rect eastUvCoordinates)
	{
		BlockFaceUvCoordinates[0] = topUvCoordinates;
		BlockFaceUvCoordinates[1] = bottomUvCoordinates;
		BlockFaceUvCoordinates[2] = northUvCoordinates;
		BlockFaceUvCoordinates[4] = southUvCoordinates;
		BlockFaceUvCoordinates[3] = westUvCoordinates;
		BlockFaceUvCoordinates[5] = eastUvCoordinates;
	}
}
