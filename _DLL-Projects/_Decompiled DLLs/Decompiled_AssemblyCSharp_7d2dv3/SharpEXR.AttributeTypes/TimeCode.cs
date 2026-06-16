namespace SharpEXR.AttributeTypes;

public struct TimeCode(uint timeAndFlags, uint userData)
{
	public readonly uint TimeAndFlags = timeAndFlags;

	public readonly uint UserData = userData;
}
