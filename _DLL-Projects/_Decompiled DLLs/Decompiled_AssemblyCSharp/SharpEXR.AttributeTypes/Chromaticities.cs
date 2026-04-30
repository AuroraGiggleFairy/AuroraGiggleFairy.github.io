namespace SharpEXR.AttributeTypes;

public struct Chromaticities(float redX, float redY, float greenX, float greenY, float blueX, float blueY, float whiteX, float whiteY)
{
	public readonly float RedX = redX;

	public readonly float RedY = redY;

	public readonly float GreenX = greenX;

	public readonly float GreenY = greenY;

	public readonly float BlueX = blueX;

	public readonly float BlueY = blueY;

	public readonly float WhiteX = whiteX;

	public readonly float WhiteY = whiteY;
}
