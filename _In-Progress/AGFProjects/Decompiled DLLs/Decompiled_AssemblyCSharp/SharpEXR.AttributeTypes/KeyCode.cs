namespace SharpEXR.AttributeTypes;

public struct KeyCode(int filmMfcCode, int filmType, int prefix, int count, int perfOffset, int perfsPerFrame, int perfsPerCount)
{
	public readonly int FilmMfcCode = filmMfcCode;

	public readonly int FilmType = filmType;

	public readonly int Prefix = prefix;

	public readonly int Count = count;

	public readonly int PerfOffset = perfOffset;

	public readonly int PerfsPerFrame = perfsPerFrame;

	public readonly int PerfsPerCount = perfsPerCount;
}
