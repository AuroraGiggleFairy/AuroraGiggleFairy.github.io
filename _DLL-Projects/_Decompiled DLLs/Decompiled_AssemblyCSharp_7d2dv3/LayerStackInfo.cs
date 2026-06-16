public struct LayerStackInfo(int startCompStackIndex, int startUVStackIndex)
{
	public int GroupDepth = 0;

	public int MaxCompStackIndex = startCompStackIndex;

	public int MaxUVStackIndex = startUVStackIndex;
}
