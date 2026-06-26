public struct SNetPackageInfo(int _id, int _size)
{
	public readonly ulong Tick = GameTimer.Instance.ticks;

	public readonly int Id = _id;

	public readonly int Size = _size;
}
